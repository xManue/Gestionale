using Backend.DTO;
using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services
{
    public class InterventionService
    {
        private readonly AppDbContext _appDbContext;
        public InterventionService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public int CreateIntervention(InterventionDTO dto)
        {
            if (dto == null)
                throw new ValidationException("Dati non validi");

            // Validazione enum corretta
            if (!Enum.IsDefined(typeof(InterventionPriority), dto.Priority))
                throw new ValidationException("Priorità non valida");

            if (dto.WorkTypeId <= 0)
                throw new ValidationException("WorkTypeId non valido");

            if (dto.DateStart == default)
                throw new ValidationException("Data di inizio obbligatoria");


            if (dto.DateEnd.HasValue && dto.DateEnd < dto.DateStart)
                throw new ValidationException("La data di fine non può essere precedente alla data di inizio");

            var workType = _appDbContext.WorkTypes
                .SingleOrDefault(wt => wt.Id == dto.WorkTypeId);

            if (workType == null)
                throw new NotFoundException("Tipo di lavoro non valido");

            var intervention = new Intervention
            {
                WorkTypeId = dto.WorkTypeId,
                DateStart = dto.DateStart,
                DateEnd = dto.DateEnd,
                CreatedAt = DateTimeOffset.UtcNow,
                Priority = dto.Priority,
                Location = dto.Location,
                Stato = InterventionStatus.Planned,
                TitleOverride = dto.TitleOverride,
                DescriptionOverride = dto.DescriptionOverride
            };

            if (dto.UserIds != null && dto.UserIds.Any())
            {
                foreach (var userId in dto.UserIds)
                {
                    intervention.Assignments.Add(new Assignment
                    {
                        UserId = userId
                    });
                }
                intervention.Stato = InterventionStatus.Assigned;
            }

            _appDbContext.Interventions.Add(intervention);
            _appDbContext.SaveChanges();

            return intervention.Id;
        }

        public void UpdateIntervention(int id, UpdateInterventionDTO dto)
        {
            var intervention = _appDbContext.Interventions
                .Include(i => i.Assignments)
                .SingleOrDefault(i => i.Id == id);

            if (intervention == null)
                throw new NotFoundException("Intervento non trovato");

            // Update only provided fields
            if (dto.TitleOverride != null)
                intervention.TitleOverride = dto.TitleOverride;

            if (dto.DescriptionOverride != null)
                intervention.DescriptionOverride = dto.DescriptionOverride;

            if (dto.Location != null)
                intervention.Location = dto.Location;

            if (dto.Priority.HasValue)
                intervention.Priority = dto.Priority.Value;

            if (dto.DateStart.HasValue)
                intervention.DateStart = dto.DateStart.Value;

            if (dto.DateEnd.HasValue)
                intervention.DateEnd = dto.DateEnd.Value;

            // Update assignments if provided
            if (dto.UserIds != null)
            {
                // Remove old assignments
                _appDbContext.Assignments.RemoveRange(intervention.Assignments);

                // Add new assignments
                foreach (var userId in dto.UserIds)
                {
                    intervention.Assignments.Add(new Assignment
                    {
                        InterventionId = id,
                        UserId = userId
                    });
                }

                // Update status: if we have assigned users and status is still Planned, move to Assigned
                if (dto.UserIds.Any() && intervention.Stato == InterventionStatus.Planned)
                {
                    intervention.Stato = InterventionStatus.Assigned;
                }
                // If no users assigned and status is Assigned, move back to Planned
                else if (!dto.UserIds.Any() && intervention.Stato == InterventionStatus.Assigned)
                {
                    intervention.Stato = InterventionStatus.Planned;
                }
            }

            _appDbContext.SaveChanges();
        }

        public void DeleteIntervention(int id)
        {
            var intervention = _appDbContext.Interventions
                .Include(i => i.Assignments)
                .Include(i => i.Materials)
                .Include(i => i.Logs)
                .Include(i => i.ChecklistItems)
                .SingleOrDefault(i => i.Id == id);

            if (intervention == null)
                throw new NotFoundException("Intervento non trovato");

            // Remove all related records
            _appDbContext.Assignments.RemoveRange(intervention.Assignments);
            _appDbContext.InterventionMaterials.RemoveRange(intervention.Materials);
            _appDbContext.InterventionLogs.RemoveRange(intervention.Logs);
            _appDbContext.RemoveRange(intervention.ChecklistItems);
            _appDbContext.Interventions.Remove(intervention);

            _appDbContext.SaveChanges();
        }

        public void ChangeInterventionStatus(int id, ChangeStatusDTO changeStatusDTO)
        {
            var intervention = _appDbContext.Interventions.SingleOrDefault(i => i.Id == id);
            if (intervention == null)
                throw new NotFoundException("Intervento non trovato");

            var currentStatus = intervention.Stato;

            if(!IsValidTransition(currentStatus, changeStatusDTO.newStatus))
                throw new ValidationException($"Transizione non valida da {currentStatus} a {changeStatusDTO.newStatus}");

            intervention.Stato = changeStatusDTO.newStatus;

            if (changeStatusDTO.newStatus == InterventionStatus.Completed)
            {
                intervention.CompletedAt = DateTimeOffset.UtcNow;
                intervention.CompletedByUserId = changeStatusDTO.UserId;
            }

            _appDbContext.SaveChanges();
        }

        private bool IsValidTransition(InterventionStatus current, InterventionStatus next)
        {
            return current switch
            {
                InterventionStatus.Planned => next == InterventionStatus.Assigned || next == InterventionStatus.Cancelled,

                InterventionStatus.Assigned => next == InterventionStatus.InProgress || next == InterventionStatus.Cancelled,

                InterventionStatus.InProgress => next == InterventionStatus.Completed || next == InterventionStatus.OnHold,

                InterventionStatus.OnHold => next == InterventionStatus.InProgress || next == InterventionStatus.Cancelled,

                InterventionStatus.Completed => false,

                InterventionStatus.Cancelled => false,

                _ => false
            };
        }
    }
}
