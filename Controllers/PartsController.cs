using api_details.Authorization;
using api_details.DataTransfer;
using api_details.Services;
using Microsoft.AspNetCore.Authorization;
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

        // Запрос для получения детали по ID
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

        // Запрос для получения деталей по категории и фильтрам
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
            [FromQuery] string? mountingType,
            [FromQuery] int? brandId,
            [FromQuery] int? modelId,
            [FromQuery] int? generationId,
            [FromQuery] int? bodyTypeId
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
                mountingType,
                brandId,
                modelId,
                generationId,
                bodyTypeId
            );

            if (parts == null || !parts.Any())
            {
                return NotFound(new { message = "Детали, соответствующие указанным параметрам, не найдены." });
            }

            return Ok(parts);
        }

        [AdminStatusAuthorize]
        [HttpPost]
        public async Task<IActionResult> CreatePart([FromBody] PartDto partDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(ModelState);
                }
                var partDtoResult = await _partService.CreatePart(partDto);
                return CreatedAtAction(nameof(GetPartById), new { partId = partDtoResult.PartId }, partDtoResult);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Внутренняя ошибка сервера", detail = ex.Message });
            }
        }

        [AdminStatusAuthorize]
        [HttpPut("{partId}")]
        public async Task<IActionResult> UpdatePart(int partId, [FromBody] PartDto partDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return BadRequest(ModelState);
                }
                var partDtoResult = await _partService.UpdatePart(partId, partDto);
                if (partDtoResult == null)
                {
                    return NotFound(new { message = "Запчасть не найдена" });
                }
                return Ok(partDtoResult);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Внутренняя ошибка сервера", detail = ex.Message });
            }
        }
    }
}
