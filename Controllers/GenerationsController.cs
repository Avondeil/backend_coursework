using api_details.Services;
using Microsoft.AspNetCore.Mvc;

namespace api_details.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenerationsController : ControllerBase
    {
        private readonly IGenerationService _generationService;

        public GenerationsController(IGenerationService generationService)
        {
            _generationService = generationService;
        }

        [HttpGet("ByModel/{modelId}")]
        public async Task<IActionResult> GetGenerationsByModel(int modelId)
        {
            var generations = await _generationService.GetGenerationsByModelIdAsync(modelId);
            return Ok(generations);
        }
    }

}
