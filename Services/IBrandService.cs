using api_details.Data;
using api_details.Models;
using Microsoft.EntityFrameworkCore;

namespace api_details.Services
{
    public interface IBrandService
    {
        Task<List<Brand>> GetAllBrandsAsync();
    }

    public class BrandService : IBrandService
    {
        private readonly ApplicationDbContext _context;

        public BrandService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Brand>> GetAllBrandsAsync()
        {
            return await _context.Brands.ToListAsync();
        }
    }
}
