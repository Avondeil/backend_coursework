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

        [HttpGet("{partId}")]
        public async Task<IActionResult> GetPartById(int partId)
        {
            var part = await _partService.GetPartById(partId);

            if (part == null)
            {
                return NotFound(new { message = "Запчасть с указанным id не найдена." });
            }

            return Ok(part);
        }

        [HttpGet("ByCategory/{category}")]
        public async Task<IActionResult> GetPartsByCategory(
            string category,
            [FromQuery] string? countryOfOrigin,
            [FromQuery] string? color,
            [FromQuery] string? dimensionsMm,
            [FromQuery] int? lengthCm,
            [FromQuery] int? loadKg,
            [FromQuery] int? volumeL,
            [FromQuery] string? material,
            [FromQuery] string? openingSystem,
            [FromQuery] string? crossbarShape,
            [FromQuery] string? mountingType
        )
        {
            // Категории для вывода запчастей
            var allowedCategories = new[] { "all", "autoboxes", "roof_racks", "parts_accessories" };

            if (!allowedCategories.Contains(category.ToLower()))
            {
                return BadRequest(new { message = $"Категория '{category}' недопустима. Допустимые значения: {string.Join(", ", allowedCategories)}." });
            }

            var parts = await _partService.GetPartsByCategoryAndFilters(
                category,
                countryOfOrigin,
                color,
                dimensionsMm,
                lengthCm,
                loadKg,
                volumeL,
                material,
                openingSystem,
                crossbarShape,
                mountingType
            );

            if (parts == null || !parts.Any())
            {
                return NotFound(new { message = "Детали, соответствующие указанным параметрам, не найдены." });
            }

            return Ok(parts);
        }
    }
}
