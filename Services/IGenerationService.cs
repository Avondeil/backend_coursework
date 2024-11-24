using api_details.Data;
using api_details.Models;
using Microsoft.EntityFrameworkCore;

namespace api_details.Services
{
    public interface IGenerationService
    {
        Task<List<Generation>> GetGenerationsByModelIdAsync(int modelId);
    }

    public class GenerationService : IGenerationService
    {
        private readonly ApplicationDbContext _context;

        public GenerationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Generation>> GetGenerationsByModelIdAsync(int modelId)
        {
            return await _context.Generations
                                 .Where(g => g.ModelId == modelId)
                                 .ToListAsync();
        }
    }

}
