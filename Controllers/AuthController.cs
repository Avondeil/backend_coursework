using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using api_details.Models;
using api_details.DataTransfer;
using System.Security.Cryptography;
using api_details.Data;

namespace api_details.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (string.IsNullOrEmpty(request.FullName) || string.IsNullOrEmpty(request.Email) ||
                string.IsNullOrEmpty(request.Password) || string.IsNullOrEmpty(request.Phone))
            {
                return BadRequest(new { message = "Все поля обязательны для заполнения" });
            }

            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                return BadRequest(new { message = "Пользователь с таким email уже существует" });
            }

            var hashedPassword = HashPassword(request.Password);

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                Password = hashedPassword,
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var response = new UserResponseDto
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                CreatedAt = DateTime.Now
            };

            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { message = "Email и пароль обязательны для входа" });
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || user.Password != HashPassword(request.Password))
            {
                return Unauthorized(new { message = "Неверный логин или пароль" });
            }

            var token = GenerateJwtToken(user, request.RememberMe);

            return Ok(new { token });
        }

        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hashedBytes = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hashedBytes);
            }
        }

        private string GenerateJwtToken(User user, bool rememberMe)
        {
            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("statusAdmin", user.StatusAdmin.ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = rememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddMinutes(30); // если "Запомнить меня", то токен живет 30 дней, иначе 30 минут

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
