using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoApi.Data;
using TodoApi.Models;
using TodoApi.Models.DTO;
using TodoApi.Services;
using System.Security.Cryptography;
using System.Text;

namespace TodoApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]

    public class AuthController : ControllerBase
    {

        private readonly TodoContext _userContext;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;

        public AuthController(TodoContext usercontext, ITokenService tokenService, IConfiguration configuration)
        {
            _userContext = usercontext;
            _tokenService = tokenService;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoApi.Models.DTO.UserResponse>>> GetUsers()
        {
            var users = await _userContext.Users
                .AsNoTracking()
                .Select(u => new TodoApi.Models.DTO.UserResponse
                {
                    Id = u.Id,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPost("register")]
        public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if user already exists
            var existingUser = await _userContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (existingUser != null)
            {
                return Conflict(new { message = "User with this email already exists" });
            }

            // Hash the password
            var passwordHash = HashPassword(request.Password);

            // Create new user
            var newUser = new UsersModel
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                PasswordHash = passwordHash
            };

            _userContext.Users.Add(newUser);
            await _userContext.SaveChangesAsync();

            // Generate token
            var token = _tokenService.GenerateToken(newUser);
            var jwtSection = _configuration.GetSection("Jwt");
            var expiresMinutes = int.Parse(jwtSection["ExpireMinutes"] ?? "60");

            var authResponse = new AuthResponse
            {
                Token = token,
                ExpiresInMinutes = expiresMinutes
            };

            return CreatedAtAction(nameof(GetUsers), new { id = newUser.Id }, authResponse);
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Find user by email
            var user = await _userContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Verify password
            if (!VerifyPassword(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password" });
            }

            // Generate token
            var token = _tokenService.GenerateToken(user);
            var jwtSection = _configuration.GetSection("Jwt");
            var expiresMinutes = int.Parse(jwtSection["ExpireMinutes"] ?? "60");

            var authResponse = new AuthResponse
            {
                Token = token,
                ExpiresInMinutes = expiresMinutes
            };

            return Ok(authResponse);
        }

        private static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        private static bool VerifyPassword(string password, string hashedPassword)
        {
            var hashedInput = HashPassword(password);
            return hashedInput == hashedPassword;
        }


    }
}
