using api_details.Data;
using api_details.Models;
using Microsoft.EntityFrameworkCore;

namespace api_details.Services
{
    public interface IPartService
    {
        Task<IEnumerable<Part>> GetPartsByCategory(string category);
    }

    public class PartService : IPartService
    {
        private readonly ApplicationDbContext _context;

        public PartService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Part>> GetPartsByCategory(string category)
        {
            IQueryable<Part> query = _context.Parts;

            // Фильтруем по категории
            switch (category.ToLower())
            {
                case "autoboxes":
                    query = query.Where(p => p.AutoboxParameter != null); // Только автобоксы
                    break;
                case "roof_racks":
                    query = query.Where(p => p.RoofRackParameter != null); // Только багажники
                    break;
                case "parts_accessories":
                    query = query.Where(p => p.SparePartsParameter != null); // Только запчасти
                    break;
                default:
                    return Enumerable.Empty<Part>(); // Если категория не найдена, возвращаем пустой список
            }

            return await query.ToListAsync();
        }
    }
}
