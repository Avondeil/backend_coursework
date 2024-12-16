using api_details.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api_details.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutoboxParametersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AutoboxParametersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetAutoboxParameters()
        {
            var autoboxParameters = await _context.AutoboxParameters
                .Select(p => new
                {
                    p.PartId,
                    p.DimensionsMm,
                    p.LoadKg,
                    p.VolumeL,
                    p.OpeningSystem,
                    p.CountryOfOrigin,
                    p.Color
                })
                .Distinct()
                .ToListAsync();

            return Ok(autoboxParameters);
        }

        [HttpGet("{partId}")]
        public async Task<ActionResult<object>> GetAutoboxParameterByPartId(int partId)
        {
            var autoboxParameter = await _context.AutoboxParameters
                .Where(p => p.PartId == partId)
                .Select(p => new
                {
                    p.PartId,
                    p.DimensionsMm,
                    p.LoadKg,
                    p.VolumeL,
                    p.OpeningSystem,
                    p.CountryOfOrigin,
                    p.Color
                })
                .FirstOrDefaultAsync();

            if (autoboxParameter == null)
            {
                return NotFound();
            }

            return Ok(autoboxParameter);
        }

    }
}
