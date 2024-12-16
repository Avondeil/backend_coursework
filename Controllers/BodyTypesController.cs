using api_details.Services;
using Microsoft.AspNetCore.Mvc;

namespace api_details.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BodyTypesController : ControllerBase
    {
        private readonly IBodyTypeService _bodyTypeService;

        public BodyTypesController(IBodyTypeService bodyTypeService)
        {
            _bodyTypeService = bodyTypeService;
        }

        [HttpGet("ByGeneration/{generationId}")]
        public async Task<IActionResult> GetBodyTypesByGeneration(int generationId)
        {
            var bodyTypes = await _bodyTypeService.GetBodyTypesByGenerationIdAsync(generationId);
            return Ok(bodyTypes);
        }
    }

}
