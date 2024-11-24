using api_details.Data;
using api_details.Models;
using Microsoft.EntityFrameworkCore;

namespace api_details.Services
{
    public interface IModelService
    {
        Task<List<Model>> GetAllModelsAsync();
    }

    public class ModelService : IModelService
    {
        private readonly ApplicationDbContext _context;

        public ModelService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Model>> GetAllModelsAsync()
        {
            return await _context.Models.ToListAsync();
        }

        public async Task<List<Model>> GetModelsByBrandAsync(int brandId)
        {
            return await _context.Models.Where(m => m.BrandId == brandId).ToListAsync();
        }
    }

}
