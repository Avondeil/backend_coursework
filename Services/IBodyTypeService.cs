using api_details.Data;
using api_details.DataTransfer;
using Microsoft.EntityFrameworkCore;

namespace api_details.Services
{
    public interface IBodyTypeService
    {
        Task<List<BodyTypesCarResponse>> GetBodyTypesByGenerationIdAsync(int generationId);
    }

    public class BodyTypeService : IBodyTypeService
    {
        private readonly ApplicationDbContext _context;

        public BodyTypeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<BodyTypesCarResponse>> GetBodyTypesByGenerationIdAsync(int generationId)
        {
            var bodyTypes = await _context.BodytypesCars
            .Where(bt => bt.GenerationId == generationId)
            .Include(bt => bt.BodyType) 
            .Select(bt => new BodyTypesCarResponse
            {
               Bodytypeid = bt.Bodytypeid,
               GenerationId = bt.GenerationId,
               BodyTypeName = bt.BodyType.Name
            }).ToListAsync();
            return bodyTypes;
        }


    }
}
