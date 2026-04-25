using Backend.DTO;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Backend.Controllers
{
    [ApiController]
    public class InterventionController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly InterventionService _interventionService;
        private readonly ILogger<InterventionController> _logger;

        public InterventionController(AppDbContext appDbContext, InterventionService interventionService, ILogger<InterventionController> logger)
        {
            _appDbContext = appDbContext;
            _interventionService = interventionService;
            _logger = logger;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("/api/intervention/getall")]
        public ActionResult<IEnumerable<InterventionResponseDTO>> GetAllInterventions()
        {
            var interventions = _appDbContext.Interventions
                .Include(i => i.Assignments)
                .ThenInclude(a => a.User)
                .OrderBy(i => i.DateStart)
                .ToList();

            if (!interventions.Any())
                return Ok(new List<InterventionResponseDTO>());

            return Ok(interventions.Select(ToResponseDTO).ToList());
        }

        [Authorize]
        [HttpGet("/api/intervention/get/{id}")]
        public ActionResult<InterventionResponseDTO> GetIntervention(int id)
        {
            var intervention = _appDbContext.Interventions
                .Include(i => i.Assignments)
                .ThenInclude(a => a.User)
                .Include(i => i.Materials)
                .Include(i => i.Logs)
                .FirstOrDefault(i => i.Id == id);

            if (intervention == null)
                return NotFound();

            return Ok(ToResponseDTO(intervention));
        }

        [Authorize]
        [HttpGet("/api/intervention/getbyfilter")]
        public ActionResult<IEnumerable<InterventionResponseDTO>> GetInterventionsByFilter(
            [FromQuery] string? filter,
            [FromQuery] DateTimeOffset? from,
            [FromQuery] DateTimeOffset? to,
            [FromQuery] InterventionStatus? status)
        {
            var query = _appDbContext.Interventions
                .Include(i => i.Assignments)
                .ThenInclude(a => a.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(filter))
                query = query.Where(i => i.TitleOverride.Contains(filter));

            if (status.HasValue)
                query = query.Where(i => i.Stato == status);

            if (from.HasValue)
                query = query.Where(i => i.DateStart >= from);

            if (to.HasValue)
                query = query.Where(i => i.DateEnd <= to);

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin")
                query = query.Where(i => i.Assignments.Any(a => a.UserId == userId));

            var result = query
                .OrderBy(i => i.DateStart)
                .ToList();

            return Ok(result.Select(ToResponseDTO).ToList());
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("/api/intervention")]
        public ActionResult CreateIntervention([FromBody] InterventionDTO interventionDTO)
        {
            var id = _interventionService.CreateIntervention(interventionDTO);
            _logger.LogInformation("Intervention created — Id: {InterventionId}", id);
            return Ok(new { id, status = "Planned" });
        }

        [Authorize]
        [HttpPatch("/api/update/status/{id}")]
        public ActionResult ChangeInterventionStatus(int id, [FromBody] ChangeStatusDTO changeStatusDTO)
        {
            _interventionService.ChangeInterventionStatus(id, changeStatusDTO);
            _logger.LogInformation("Intervention {InterventionId} status changed to {NewStatus}", id, changeStatusDTO.newStatus);
            return Ok();
        }

        [Authorize]
        [HttpGet("/api/get/myintervention")]
        public ActionResult<List<InterventionResponseDTO>> GetMyIntervention()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized();

            var userId = int.Parse(userIdClaim.Value);

            var user = _appDbContext.Users.FirstOrDefault(u => u.IsActive && u.Id == userId);
            if (user == null)
                return Unauthorized();

            var interventions = _appDbContext.Interventions
                .Include(i => i.Assignments)
                .ThenInclude(a => a.User)
                .Where(i => i.Assignments.Any(q => q.UserId == userId))
                .OrderBy(i => i.DateStart)
                .ToList();

            return Ok(interventions.Select(ToResponseDTO).ToList());
        }

        /// <summary>
        /// Maps an EF Intervention entity to a clean response DTO.
        /// Normalizes field names: TitleOverride→Title, Stato→Status, DescriptionOverride→Description.
        /// </summary>
        private static InterventionResponseDTO ToResponseDTO(Intervention intervention)
        {
            return new InterventionResponseDTO
            {
                Id = intervention.Id,
                Title = intervention.TitleOverride,
                Location = intervention.Location,
                Description = intervention.DescriptionOverride,
                Status = intervention.Stato.ToString(),
                Priority = intervention.Priority.ToString(),
                DateStart = intervention.DateStart,
                DateEnd = intervention.DateEnd,
                CreatedAt = intervention.CreatedAt,
                CompletedAt = intervention.CompletedAt
            };
        }
    }
}
