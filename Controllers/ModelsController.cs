using api_details.Services;
using Microsoft.AspNetCore.Mvc;

namespace api_details.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModelsController : ControllerBase
    {
        private readonly IModelService _modelService;

        public ModelsController(IModelService modelService)
        {
            _modelService = modelService;
        }

        // Получить все модели
        [HttpGet]
        public async Task<IActionResult> GetAllModels()
        {
            var models = await _modelService.GetAllModelsAsync();
            return Ok(models);
        }

        // Получить модели по ID марки
        [HttpGet("ByBrand/{brandId}")]
        public async Task<IActionResult> GetModelsByBrand(int brandId)
        {
            var models = await _modelService.GetAllModelsAsync();
            var filteredModels = models.Where(m => m.BrandId == brandId).ToList();
            return Ok(filteredModels);
        }
    }
}
