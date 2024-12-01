using api_details.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api_details.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SparePartsParametersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SparePartsParametersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/SparePartsParameters
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetSparePartsParameters()
        {
            var sparePartsParameters = await _context.SparePartsParameters
                .Select(p => new
                {
                    p.PartId,
                    p.CountryOfOrigin,
                    p.Color
                })
                .Distinct()
                .ToListAsync();

            return Ok(sparePartsParameters);
        }
    }
}
