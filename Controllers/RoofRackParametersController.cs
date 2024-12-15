using api_details.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api_details.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoofRackParametersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RoofRackParametersController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetRoofRackParameters()
        {
            var roofRackParameters = await _context.RoofRackParameters
                .Select(p => new
                {
                    p.PartId,
                    p.LengthCm,
                    p.Material,
                    p.LoadKg,
                    p.MountingType,
                    p.CrossbarShape,
                    p.CountryOfOrigin,
                    p.Color
                })
                .Distinct()
                .ToListAsync();

            return Ok(roofRackParameters);
        }

        [HttpGet("{partId}")]
        public async Task<ActionResult<object>> GetRoofRackParameterByPartId(int partId)
        {
            var roofRackParameter = await _context.RoofRackParameters
                .Where(p => p.PartId == partId)
                .Select(p => new
                {
                    p.PartId,
                    p.LengthCm,
                    p.Material,
                    p.LoadKg,
                    p.MountingType,
                    p.CrossbarShape,
                    p.CountryOfOrigin,
                    p.Color
                })
                .FirstOrDefaultAsync();

            if (roofRackParameter == null)
            {
                return NotFound();
            }

            return Ok(roofRackParameter);
        }
    }
}
