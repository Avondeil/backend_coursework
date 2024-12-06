using api_details.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace api_details.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult GetCurrentUser()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var statusAdmin = User.FindFirst("statusAdmin")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("Пользователь не найден или не авторизован.");
                }

                var user = _context.Users.FirstOrDefault(u => u.UserId == int.Parse(userId));
                if (user == null)
                {
                    return NotFound("Пользователь не найден в базе данных.");
                }

                return Ok(new
                {
                    FullName = user.FullName,
                    Email = user.Email,
                    Phone = user.Phone,
                    StatusAdmin = user.StatusAdmin
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Внутренняя ошибка сервера: {ex.Message}");
            }
        }
    }
}
