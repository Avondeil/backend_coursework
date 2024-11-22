using api_details.Models;
using api_details.Services;
using Microsoft.AspNetCore.Mvc;

namespace api_details.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        private readonly IBrandService _brandService;

        public BrandsController(IBrandService brandService)
        {
            _brandService = brandService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Brand>>> GetBrands()
        {
            var brands = await _brandService.GetAllBrandsAsync();
            return Ok(brands);
        }
    }

}
