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
            var listaUsers = _appDbContext.Users.Where(u => u.IsActive).ToList();
            if (listaUsers.Count == 0)
                return NotFound();
            var result = listaUsers.Select(u => new UserDTO
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email
            }).ToList();
            return Ok(result);
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
