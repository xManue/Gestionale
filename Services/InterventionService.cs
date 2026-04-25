using Backend.DTO;
using Backend.Models;

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

            if (dto.DateStart < DateTimeOffset.UtcNow)
                throw new ValidationException("La data di inizio non può essere nel passato");

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

            _appDbContext.Interventions.Add(intervention);
            _appDbContext.SaveChanges();

            return intervention.Id;
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
