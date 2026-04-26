using Backend.Models;
using Backend.ModelsDTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;

        public UserController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("/api/users")]
        public ActionResult<List<UserDTO>> GetUsers()
        {
            var listaUsers = _appDbContext.Users.ToList();
            var result = listaUsers.Select(u => new UserDTO
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive
            }).ToList();
            return Ok(result);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("/api/users")]
        public ActionResult<UserDTO> CreateUser([FromBody] Backend.DTO.RegisterDTO req)
        {
            if (_appDbContext.Users.Any(u => u.Email == req.Email))
                return BadRequest("Email already exists");

            var user = new User
            {
                Name = req.Name,
                Email = req.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password),
                Role = RoleEnum.Employee,
                IsActive = true
            };

            _appDbContext.Users.Add(user);
            _appDbContext.SaveChanges();

            return Ok(new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                IsActive = user.IsActive
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("/api/users/{id}")]
        public ActionResult DeleteUser(int id)
        {
            var user = _appDbContext.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
                return NotFound();

            if (user.Role == RoleEnum.Admin && user.Email == "admin@test.com")
                return BadRequest("Cannot delete the main admin");

            // Cleanup dependencies to avoid FK errors
            var assignments = _appDbContext.Assignments.Where(a => a.UserId == id).ToList();
            _appDbContext.Assignments.RemoveRange(assignments);

            var interventions = _appDbContext.Interventions.Where(i => i.CompletedByUserId == id).ToList();
            foreach (var i in interventions) i.CompletedByUserId = null;

            _appDbContext.Users.Remove(user);
            _appDbContext.SaveChanges();

            return Ok(new { Deleted = true });
        }

        [Authorize]
        [HttpGet("/api/users/{id}")]
        public ActionResult<UserDTO> GetUser(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (role != "Admin" && userId != id)
                return Forbid();

            var user = _appDbContext.Users
                .FirstOrDefault(u => u.IsActive && u.Id == id);

            if (user == null)
                return NotFound();

            return Ok(new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            });
        }

        [Authorize]
        [HttpGet("/api/users/me")]
        public ActionResult<UserDTO> GetCurrentUser()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var user = _appDbContext.Users.FirstOrDefault(u => u.IsActive && u.Id == userId);
            if (user == null)
                return NotFound();

            var result = new UserDTO
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email
            };
            return Ok(result);
        }
    }
}
