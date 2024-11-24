using api_details.Data;
using api_details.Models;
using Microsoft.EntityFrameworkCore;

namespace api_details.Services
{
    public interface IBodyTypeService
    {
        Task<List<Bodytype>> GetBodyTypesByGenerationIdAsync(int generationId);
    }

    public class BodyTypeService : IBodyTypeService
    {
        private readonly ApplicationDbContext _context;

        public BodyTypeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Bodytype>> GetBodyTypesByGenerationIdAsync(int generationId)
        {
            return await _context.Bodytypes
                                 .Where(bt => bt.GenerationId == generationId)
                                 .ToListAsync();
        }
    }
}
