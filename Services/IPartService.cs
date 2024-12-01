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
            string? dimensionsMm,
            int? lengthCm,
            int? loadKg,
            int? volumeL,
            string? material,
            string? openingSystem,
            string? crossbarShape,
            string? mountingType
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
            string? dimensionsMm,
            int? lengthCm,
            int? loadKg,
            int? volumeL,
            string? material,
            string? openingSystem,
            string? crossbarShape,
            string? mountingType
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


            // Фильтрация по размерам
            if (!string.IsNullOrEmpty(dimensionsMm))
            {
                query = query.Where(p => p.AutoboxParameter.DimensionsMm == dimensionsMm);
            }

            // Фильтрация по длине (для багажников)
            if (lengthCm.HasValue)
            {
                query = query.Where(p => p.RoofRackParameter.LengthCm == lengthCm);
            }


            // Фильтрация по Весу
            if (loadKg.HasValue)
            {
                query = query.Where(p => p.AutoboxParameter.LoadKg == loadKg ||
                                         p.RoofRackParameter.LoadKg == loadKg);
            }

            // Фильтрация по Объему
            if (volumeL.HasValue)
            {
                query = query.Where(p => p.AutoboxParameter.VolumeL == volumeL);

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
            if (!string.IsNullOrEmpty(crossbarShape))
            {
                query = query.Where(p => p.RoofRackParameter.CrossbarShape == crossbarShape);
            }

            // Фильтрация по Типу установки
            if (!string.IsNullOrEmpty(mountingType))
            {
                query = query.Where(p => p.RoofRackParameter.MountingType == mountingType);
            }

            return await query.ToListAsync();
        }
    }
}
