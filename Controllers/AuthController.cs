using Backend.DTO;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _appDbContext;
        private readonly AuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDbContext appDbContext, AuthService authService, ILogger<AuthController> logger)
        {
            _appDbContext = appDbContext;
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("/api/auth/login")]
        public ActionResult<LoginResponse> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) ||
        string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest("Email e password obbligatorie");
            }

            // 2. cerca utente
            var user = _appDbContext.Users
                .FirstOrDefault(u => u.Email == request.Email);

            if (user == null)
            {
                _logger.LogWarning("Login failed — user not found: {Email}", request.Email);
                return Unauthorized("Credenziali non valide");
            }

            // 3. controlla attivo
            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed — user inactive: {Email}", request.Email);
                return Unauthorized("Credenziali non valide");
            }

            var isValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isValid)
            {
                _logger.LogWarning("Login failed — wrong password: {Email}", request.Email);
                return Unauthorized("Credenziali non valide");
            }

            var token = _authService.GenerateJwt(user);

            _logger.LogInformation("Login successful — User {UserId} ({Email})", user.Id, user.Email);
            return Ok(new LoginResponse { Token = token });
        }

        [HttpPost("/api/auth/register")]
        public IActionResult Register([FromBody] RegisterDTO dto)
        {
            if (dto == null ||
        string.IsNullOrWhiteSpace(dto.Name) ||
        string.IsNullOrWhiteSpace(dto.Email) ||
        string.IsNullOrWhiteSpace(dto.Password))
            {
                return BadRequest("Devi inserire tutti i campi");
            }

            var userEsistente = _appDbContext.Users
        .FirstOrDefault(u => u.Email == dto.Email);

            if (userEsistente != null)
                return BadRequest("Utente gia registrato");

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = RoleEnum.Employee,
            };
            _appDbContext.Users.Add(user);
            _appDbContext.SaveChanges();

            _logger.LogInformation("User registered — {UserId} ({Email})", user.Id, user.Email);
            return Ok("Utente registrato con successo");
        }
    }
}
