using api_details.Services;
using Microsoft.AspNetCore.Mvc;

namespace api_details.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PartsController : ControllerBase
    {
        private readonly IPartService _partService;

        public PartsController(IPartService partService)
        {
            _partService = partService;
        }

        // Эндпоинт для получения деталей по категории (autoboxes, roofracks, spareparts)
        [HttpGet("ByCategory/{category}")]
        public async Task<IActionResult> GetPartsByCategory(string category)
        {
            // Получаем детали по категории
            var parts = await _partService.GetPartsByCategory(category);

            // Если детали не найдены, возвращаем ошибку
            if (parts == null || !parts.Any())
            {
                return NotFound(new { message = "Детали для указанной категории не найдены." });
            }

            // Возвращаем найденные детали
            return Ok(parts);
        }
    }
}
