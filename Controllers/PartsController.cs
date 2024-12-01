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

        [HttpGet("ByCategory/{category}")]
        public async Task<IActionResult> GetPartsByCategory(
            string category,
            [FromQuery] string? countryOfOrigin,
            [FromQuery] string? color,
            [FromQuery] string? size,
            [FromQuery] int? weight,
            [FromQuery] int? volume,
            [FromQuery] string? material,
            [FromQuery] string? openingSystem,
            [FromQuery] string? crossBarShape,
            [FromQuery] string? installationType
        )
        {
            var parts = await _partService.GetPartsByCategoryAndFilters(
                category,
                countryOfOrigin,
                color,
                size,
                weight,
                volume,
                material,
                openingSystem,
                crossBarShape,
                installationType
            );

            if (parts == null || !parts.Any())
            {
                return NotFound(new { message = "Детали для указанной категории не найдены." });
            }

            return Ok(parts);
        }
    }
}
