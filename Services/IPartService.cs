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

        Task<Part> GetPartById(int partId); // Для получения детали по id
    }

    public class PartService : IPartService
    {
        private readonly ApplicationDbContext _context;

        public PartService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Part> GetPartById(int partId)
        {
            // Поиск детали по partId
            var part = await _context.Parts
                .Include(p => p.AutoboxParameter)
                .Include(p => p.RoofRackParameter)
                .Include(p => p.SparePartsParameter)
                .FirstOrDefaultAsync(p => p.PartId == partId);

            return part;
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
            // Создаём базовый запрос
            IQueryable<Part> query = _context.Parts
                .Include(p => p.AutoboxParameter)
                .Include(p => p.RoofRackParameter)
                .Include(p => p.SparePartsParameter);

            // Проверка категории
            if (category.ToLower() != "all")
            {
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
            }

            // Фильтрация по стране происхождения
            if (!string.IsNullOrEmpty(countryOfOrigin))
            {
                query = query.Where(p =>
                    (p.AutoboxParameter != null && p.AutoboxParameter.CountryOfOrigin == countryOfOrigin) ||
                    (p.RoofRackParameter != null && p.RoofRackParameter.CountryOfOrigin == countryOfOrigin) ||
                    (p.SparePartsParameter != null && p.SparePartsParameter.CountryOfOrigin == countryOfOrigin)
                );
            }

            // Фильтрация по цвету
            if (!string.IsNullOrEmpty(color))
            {
                query = query.Where(p =>
                    (p.AutoboxParameter != null && p.AutoboxParameter.Color == color) ||
                    (p.RoofRackParameter != null && p.RoofRackParameter.Color == color) ||
                    (p.SparePartsParameter != null && p.SparePartsParameter.Color == color)
                );
            }

            // Фильтрация по размерам
            if (!string.IsNullOrEmpty(dimensionsMm))
            {
                query = query.Where(p => p.AutoboxParameter != null && p.AutoboxParameter.DimensionsMm == dimensionsMm);
            }

            // Фильтрация по длине (для багажников)
            if (lengthCm.HasValue)
            {
                query = query.Where(p => p.RoofRackParameter != null && p.RoofRackParameter.LengthCm == lengthCm);
            }

            // Фильтрация по весу
            if (loadKg.HasValue)
            {
                query = query.Where(p =>
                    (p.AutoboxParameter != null && p.AutoboxParameter.LoadKg == loadKg) ||
                    (p.RoofRackParameter != null && p.RoofRackParameter.LoadKg == loadKg)
                );
            }

            // Фильтрация по объёму
            if (volumeL.HasValue)
            {
                query = query.Where(p => p.AutoboxParameter != null && p.AutoboxParameter.VolumeL == volumeL);
            }

            // Фильтрация по материалу
            if (!string.IsNullOrEmpty(material))
            {
                query = query.Where(p => p.RoofRackParameter != null && p.RoofRackParameter.Material == material);
            }

            // Фильтрация по системе открывания
            if (!string.IsNullOrEmpty(openingSystem))
            {
                query = query.Where(p => p.AutoboxParameter != null && p.AutoboxParameter.OpeningSystem == openingSystem);
            }

            // Фильтрация по форме поперечин
            if (!string.IsNullOrEmpty(crossbarShape))
            {
                query = query.Where(p => p.RoofRackParameter != null && p.RoofRackParameter.CrossbarShape == crossbarShape);
            }

            // Фильтрация по типу установки
            if (!string.IsNullOrEmpty(mountingType))
            {
                query = query.Where(p => p.RoofRackParameter != null && p.RoofRackParameter.MountingType == mountingType);
            }

            // Выполняем запрос и возвращаем результат
            return await query.ToListAsync();
        }
    }
}
