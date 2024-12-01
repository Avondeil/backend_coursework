using api_details.Data;
using api_details.Models;
using Microsoft.EntityFrameworkCore;

namespace api_details.Services
{
    public interface IPartService
    {
        Task<IEnumerable<Part>> GetPartsByCategoryAndFilters(
            string category,
            string? countryOfOrigin,
            string? color,
            string? size,
            int? weight,
            int? volume,
            string? material,
            string? openingSystem,
            string? crossBarShape,
            string? installationType
        );
    }

    public class PartService : IPartService
    {
        private readonly ApplicationDbContext _context;

        public PartService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Part>> GetPartsByCategoryAndFilters(
            string category,
            string? countryOfOrigin,
            string? color,
            string? size,
            int? weight,
            int? volume,
            string? material,
            string? openingSystem,
            string? crossBarShape,
            string? installationType
        )
        {
            IQueryable<Part> query = _context.Parts.Include(p => p.AutoboxParameter)
                                                   .Include(p => p.RoofRackParameter)
                                                   .Include(p => p.SparePartsParameter);

            // Фильтруем по категории
            switch (category.ToLower())
            {
                case "autoboxes":
                    query = query.Where(p => p.AutoboxParameter != null);
                    break;
                case "roof_racks":
                    query = query.Where(p => p.RoofRackParameter != null);
                    break;
                case "parts_accessories":
                    query = query.Where(p => p.SparePartsParameter != null);
                    break;
                default:
                    return Enumerable.Empty<Part>();
            }

            // Фильтрация по Стране происхождения
            if (!string.IsNullOrEmpty(countryOfOrigin))
            {
                switch (category.ToLower())
                {
                    case "autoboxes":
                        query = query.Where(p => p.AutoboxParameter.CountryOfOrigin == countryOfOrigin);
                        break;
                    case "roof_racks":
                        query = query.Where(p => p.RoofRackParameter.CountryOfOrigin == countryOfOrigin);
                        break;
                    case "parts_accessories":
                        query = query.Where(p => p.SparePartsParameter.CountryOfOrigin == countryOfOrigin);
                        break;
                }
            }

            // Фильтрация по Цвету
            if (!string.IsNullOrEmpty(color))
            {
                query = query.Where(p => p.AutoboxParameter.Color == color ||
                                         p.RoofRackParameter.Color == color ||
                                         p.SparePartsParameter.Color == color);
            }

            // Фильтрация по Размеру
            if (!string.IsNullOrEmpty(size))
            {
                switch (category.ToLower())
                {
                    case "autoboxes":
                        query = query.Where(p => p.AutoboxParameter.DimensionsMm == size);
                        break;
                    case "roof_racks":
                        query = query.Where(p => p.RoofRackParameter.LengthCm.ToString() == size);
                        break;
                }
            }

            // Фильтрация по Весу
            if (weight.HasValue)
            {
                query = query.Where(p => p.AutoboxParameter.LoadKg == weight ||
                                         p.RoofRackParameter.LoadKg == weight);
            }

            // Фильтрация по Объему
            if (volume.HasValue)
            {
                query = query.Where(p => p.AutoboxParameter.VolumeL == volume);
            }

            // Фильтрация по Материалу
            if (!string.IsNullOrEmpty(material))
            {
                query = query.Where(p => p.RoofRackParameter.Material == material);
            }

            // Фильтрация по Системе открывания
            if (!string.IsNullOrEmpty(openingSystem))
            {
                query = query.Where(p => p.AutoboxParameter.OpeningSystem == openingSystem);
            }

            // Фильтрация по Форме поперечин
            if (!string.IsNullOrEmpty(crossBarShape))
            {
                query = query.Where(p => p.RoofRackParameter.CrossbarShape == crossBarShape);
            }

            // Фильтрация по Типу установки
            if (!string.IsNullOrEmpty(installationType))
            {
                query = query.Where(p => p.RoofRackParameter.MountingType == installationType);
            }

            return await query.ToListAsync();
        }
    }
}
