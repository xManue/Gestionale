using Backend.DTO;
using Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    public class WorkTypeController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public WorkTypeController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("/api/worktypes")]
        public ActionResult<List<WorkTypeDTO>> GetAll()
        {
            var workTypes = _appDbContext.WorkTypes
                .Include(w => w.Materials)
                .Include(w => w.Tools)
                .OrderBy(w => w.Name)
                .ToList();

            var result = workTypes.Select(w => new WorkTypeDTO
            {
                Id = w.Id,
                Name = w.Name,
                Description = w.Description,
                Materials = w.Materials
                    .OrderBy(m => m.Name)
                    .Select(m => m.Name)
                    .ToList(),
                Tools = w.Tools
                    .OrderBy(t => t.Name)
                    .Select(t => t.Name)
                    .ToList()
            }).ToList();

            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("/api/worktypes")]
        public ActionResult<WorkTypeDTO> Create([FromBody] CreateWorkTypeDTO dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("Nome obbligatorio");

            var name = dto.Name.Trim();
            if (_appDbContext.WorkTypes.Any(w => w.Name.ToLower() == name.ToLower()))
                return BadRequest("Esiste già un template con questo nome");

            var workType = new WorkType
            {
                Name = name,
                Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
                Materials = new List<WorkTypeMaterial>(),
                Tools = new List<WorkTypeTool>()
            };

            foreach (var m in (dto.Materials ?? new List<string>()))
            {
                var mat = (m ?? string.Empty).Trim();
                if (mat.Length == 0) continue;
                workType.Materials.Add(new WorkTypeMaterial { Name = mat });
            }

            foreach (var t in (dto.Tools ?? new List<string>()))
            {
                var tool = (t ?? string.Empty).Trim();
                if (tool.Length == 0) continue;
                workType.Tools.Add(new WorkTypeTool { Name = tool });
            }

            _appDbContext.WorkTypes.Add(workType);
            _appDbContext.SaveChanges();

            return Ok(new WorkTypeDTO
            {
                Id = workType.Id,
                Name = workType.Name,
                Description = workType.Description,
                Materials = workType.Materials.Select(x => x.Name).ToList(),
                Tools = workType.Tools.Select(x => x.Name).ToList()
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("/api/worktypes/{id}")]
        public IActionResult Delete(int id)
        {
            var wt = _appDbContext.WorkTypes
                .Include(w => w.Materials)
                .Include(w => w.Tools)
                .FirstOrDefault(w => w.Id == id);

            if (wt == null)
                return NotFound();

            var usedByInterventions = _appDbContext.Interventions.Any(i => i.WorkTypeId == id);
            if (usedByInterventions)
                return BadRequest("Template usato da interventi: non eliminabile");

            _appDbContext.WorkTypeMaterials.RemoveRange(wt.Materials);
            _appDbContext.WorkTypeTools.RemoveRange(wt.Tools);
            _appDbContext.WorkTypes.Remove(wt);
            _appDbContext.SaveChanges();

            return Ok();
        }
    }
}

