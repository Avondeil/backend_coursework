using api_details.Data;
using api_details.DataTransfer;
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
            string? mountingType,
            int? brandId,
            int? modelId,
            int? generationId,
            int? bodyTypeId
        );

        Task<PartDto> GetPartById(int partId); // Для получения детали по id
        Task<PartDto> CreatePart(PartDto partDto); // Для добавления новой детали
        Task<PartDto> UpdatePart(int partId, PartDto partDto); // Для редактирования детали

    }

    public class PartService : IPartService
    {
        private readonly ApplicationDbContext _context;

        public PartService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PartDto> GetPartById(int partId)
        {
            var part = await _context.Parts
                .Include(p => p.AutoboxParameter)
                .Include(p => p.RoofRackParameter)
                .Include(p => p.SparePartsParameter)
                .Include(p => p.PartsAutos)
                    .ThenInclude(pa => pa.Brand)
                .Include(p => p.PartsAutos)
                    .ThenInclude(pa => pa.Model)
                .Include(p => p.PartsAutos)
                    .ThenInclude(pa => pa.Generation)
                .Include(p => p.PartsAutos)
                    .ThenInclude(pa => pa.Bodytype)
                    .ThenInclude(bc => bc.BodyType)
                .FirstOrDefaultAsync(p => p.PartId == partId);

            if (part == null)
            {
                return null;
            }

            return new PartDto
            {
                PartId = part.PartId,
                Name = part.Name,
                Description = part.Description,
                Price = part.Price,
                StockQuantity = part.StockQuantity,
                ProductTypeId = part.ProductTypeId,
                ImageUrl = part.ImageUrl,
                AutoboxParameter = part.AutoboxParameter != null ? new AutoboxParameterDto
                {
                    DimensionsMm = part.AutoboxParameter.DimensionsMm,
                    LoadKg = part.AutoboxParameter.LoadKg,
                    VolumeL = part.AutoboxParameter.VolumeL,
                    OpeningSystem = part.AutoboxParameter.OpeningSystem,
                    CountryOfOrigin = part.AutoboxParameter.CountryOfOrigin,
                    Color = part.AutoboxParameter.Color
                } : null,
                RoofRackParameter = part.RoofRackParameter != null ? new RoofRackParameterDto
                {
                    LengthCm = part.RoofRackParameter.LengthCm,
                    Material = part.RoofRackParameter.Material,
                    LoadKg = part.RoofRackParameter.LoadKg,
                    MountingType = part.RoofRackParameter.MountingType,
                    CrossbarShape = part.RoofRackParameter.CrossbarShape,
                    CountryOfOrigin = part.RoofRackParameter.CountryOfOrigin,
                    Color = part.RoofRackParameter.Color
                } : null,
                SparePartsParameter = part.SparePartsParameter != null ? new SparePartsParameterDto
                {
                    CountryOfOrigin = part.SparePartsParameter.CountryOfOrigin,
                    Color = part.SparePartsParameter.Color
                } : null,
                Compatibilities = part.PartsAutos.Select(pa => new PartAutoCompatibilityDto
                {
                    BrandId = pa.BrandId,
                    BrandName = pa.Brand?.Name ?? "Неизвестный бренд",
                    ModelId = pa.ModelId,
                    ModelName = pa.Model?.Name ?? "Неизвестная модель",
                    GenerationId = pa.GenerationId,
                    GenerationYear = pa.Generation?.Year.ToString() ?? "Неизвестное поколение",
                    BodytypeId = pa.Bodytypeid,
                    BodytypeName = pa.Bodytype?.BodyType?.Name ?? "Неизвестный тип кузова"
                }).ToList()
            };
        }

        // Метод для получения деталей по категории и фильтрам
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
            string? mountingType,
            int? brandId,
            int? modelId,
            int? generationId,
            int? bodyTypeId
        )
        {
            // Создаём базовый запрос
            IQueryable<Part> query = _context.Parts
                .Include(p => p.AutoboxParameter)
                .Include(p => p.RoofRackParameter)
                .Include(p => p.SparePartsParameter)
                .Include(p => p.PartsAutos) // Исправление: связи с PartsAutos
                    .ThenInclude(pa => pa.Brand)
                .Include(p => p.PartsAutos)
                    .ThenInclude(pa => pa.Model)
                .Include(p => p.PartsAutos)
                    .ThenInclude(pa => pa.Generation)
                .Include(p => p.PartsAutos)
                    .ThenInclude(pa => pa.Bodytype);

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

            // Фильтрация по марке автомобиля
            if (brandId.HasValue)
            {
                query = query.Where(p => p.PartsAutos.Any(pa => pa.BrandId == brandId));
            }

            // Фильтрация по модели автомобиля
            if (modelId.HasValue)
            {
                query = query.Where(p => p.PartsAutos.Any(pa => pa.ModelId == modelId));
            }

            // Фильтрация по поколению автомобиля
            if (generationId.HasValue)
            {
                query = query.Where(p => p.PartsAutos.Any(pa => pa.GenerationId == generationId));
            }

            // Фильтрация по типу кузова
            if (bodyTypeId.HasValue)
            {
                query = query.Where(p => p.PartsAutos.Any(pa => pa.Bodytypeid == bodyTypeId));
            }

            return await query.ToListAsync();
        }

        public async Task<PartDto> CreatePart(PartDto partDto)
        {
            try
            {
                if (partDto.ProductTypeId < 1 || partDto.ProductTypeId > 3)
                {
                    throw new ArgumentException("Недопустимый ProductTypeId. Допустимые значения: 1, 2, 3.");
                }

                var part = new Part
                {
                    Name = partDto.Name,
                    Description = partDto.Description,
                    Price = partDto.Price,
                    StockQuantity = partDto.StockQuantity,
                    ProductTypeId = partDto.ProductTypeId,
                    ImageUrl = partDto.ImageUrl
                };

                if (partDto.ProductTypeId == 1 && partDto.AutoboxParameter != null)
                {
                    part.AutoboxParameter = new AutoboxParameter
                    {
                        DimensionsMm = partDto.AutoboxParameter.DimensionsMm,
                        LoadKg = partDto.AutoboxParameter.LoadKg,
                        VolumeL = partDto.AutoboxParameter.VolumeL,
                        OpeningSystem = partDto.AutoboxParameter.OpeningSystem,
                        CountryOfOrigin = partDto.AutoboxParameter.CountryOfOrigin,
                        Color = partDto.AutoboxParameter.Color
                    };
                }
                else if (partDto.ProductTypeId == 2 && partDto.RoofRackParameter != null)
                {
                    part.RoofRackParameter = new RoofRackParameter
                    {
                        LengthCm = partDto.RoofRackParameter.LengthCm,
                        Material = partDto.RoofRackParameter.Material,
                        LoadKg = partDto.RoofRackParameter.LoadKg,
                        MountingType = partDto.RoofRackParameter.MountingType,
                        CrossbarShape = partDto.RoofRackParameter.CrossbarShape,
                        CountryOfOrigin = partDto.RoofRackParameter.CountryOfOrigin,
                        Color = partDto.RoofRackParameter.Color
                    };
                }
                else if (partDto.ProductTypeId == 3 && partDto.SparePartsParameter != null)
                {
                    part.SparePartsParameter = new SparePartsParameter
                    {
                        CountryOfOrigin = partDto.SparePartsParameter.CountryOfOrigin,
                        Color = partDto.SparePartsParameter.Color
                    };
                }
                else
                {
                    throw new ArgumentException("Параметры не соответствуют указанному ProductTypeId");
                }

                if (partDto.Compatibilities != null)
                {
                    foreach (var c in partDto.Compatibilities)
                    {
                        if (!await _context.Brands.AnyAsync(b => b.BrandId == c.BrandId))
                            throw new ArgumentException($"Бренд с ID {c.BrandId} не существует");
                        if (!await _context.Models.AnyAsync(m => m.ModelId == c.ModelId))
                            throw new ArgumentException($"Модель с ID {c.ModelId} не существует");
                        if (!await _context.Generations.AnyAsync(g => g.GenerationId == c.GenerationId))
                            throw new ArgumentException($"Поколение с ID {c.GenerationId} не существует");
                        if (!await _context.BodytypesCars.AnyAsync(btc => btc.Bodytypeid == c.BodytypeId))
                            throw new ArgumentException($"Тип кузова с ID {c.BodytypeId} не существует");
                    }

                    part.PartsAutos = partDto.Compatibilities.Select(c => new PartsAuto
                    {
                        BrandId = c.BrandId,
                        ModelId = c.ModelId,
                        GenerationId = c.GenerationId,
                        Bodytypeid = c.BodytypeId
                    }).ToList();
                }

                _context.Parts.Add(part);
                await _context.SaveChangesAsync();

                return new PartDto
                {
                    PartId = part.PartId,
                    Name = part.Name,
                    Description = part.Description,
                    Price = part.Price,
                    StockQuantity = part.StockQuantity,
                    ProductTypeId = part.ProductTypeId,
                    ImageUrl = part.ImageUrl,
                    AutoboxParameter = part.AutoboxParameter != null ? new AutoboxParameterDto
                    {
                        DimensionsMm = part.AutoboxParameter.DimensionsMm,
                        LoadKg = part.AutoboxParameter.LoadKg,
                        VolumeL = part.AutoboxParameter.VolumeL,
                        OpeningSystem = part.AutoboxParameter.OpeningSystem,
                        CountryOfOrigin = part.AutoboxParameter.CountryOfOrigin,
                        Color = part.AutoboxParameter.Color
                    } : null,
                    RoofRackParameter = part.RoofRackParameter != null ? new RoofRackParameterDto
                    {
                        LengthCm = part.RoofRackParameter.LengthCm,
                        Material = part.RoofRackParameter.Material,
                        LoadKg = part.RoofRackParameter.LoadKg,
                        MountingType = part.RoofRackParameter.MountingType,
                        CrossbarShape = part.RoofRackParameter.CrossbarShape,
                        CountryOfOrigin = part.RoofRackParameter.CountryOfOrigin,
                        Color = part.RoofRackParameter.Color
                    } : null,
                    SparePartsParameter = part.SparePartsParameter != null ? new SparePartsParameterDto
                    {
                        CountryOfOrigin = part.SparePartsParameter.CountryOfOrigin,
                        Color = part.SparePartsParameter.Color
                    } : null,
                    Compatibilities = part.PartsAutos?.Select(pa => new PartAutoCompatibilityDto
                    {
                        BrandId = pa.BrandId,
                        ModelId = pa.ModelId,
                        GenerationId = pa.GenerationId,
                        BodytypeId = pa.Bodytypeid
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при создании: {ex.Message}");
                throw;
            }
        }

        public async Task<PartDto> UpdatePart(int partId, PartDto partDto)
        {
            try
            {
                var part = await _context.Parts
                    .Include(p => p.PartsAutos)
                    .Include(p => p.AutoboxParameter)
                    .Include(p => p.RoofRackParameter)
                    .Include(p => p.SparePartsParameter)
                    .FirstOrDefaultAsync(p => p.PartId == partId);

                if (part == null)
                {
                    return null;
                }

                if (partDto.ProductTypeId < 1 || partDto.ProductTypeId > 3)
                {
                    throw new ArgumentException("Недопустимый ProductTypeId. Допустимые значения: 1, 2, 3.");
                }

                part.Name = partDto.Name;
                part.Description = partDto.Description;
                part.Price = partDto.Price;
                part.StockQuantity = partDto.StockQuantity;
                part.ProductTypeId = partDto.ProductTypeId;
                part.ImageUrl = partDto.ImageUrl;

                if (part.AutoboxParameter != null && partDto.ProductTypeId != 1)
                {
                    _context.AutoboxParameters.Remove(part.AutoboxParameter);
                    part.AutoboxParameter = null;
                }
                if (part.RoofRackParameter != null && partDto.ProductTypeId != 2)
                {
                    _context.RoofRackParameters.Remove(part.RoofRackParameter);
                    part.RoofRackParameter = null;
                }
                if (part.SparePartsParameter != null && partDto.ProductTypeId != 3)
                {
                    _context.SparePartsParameters.Remove(part.SparePartsParameter);
                    part.SparePartsParameter = null;
                }

                if (partDto.ProductTypeId == 1 && partDto.AutoboxParameter != null)
                {
                    if (part.AutoboxParameter == null)
                    {
                        part.AutoboxParameter = new AutoboxParameter { PartId = partId };
                    }
                    part.AutoboxParameter.DimensionsMm = partDto.AutoboxParameter.DimensionsMm;
                    part.AutoboxParameter.LoadKg = partDto.AutoboxParameter.LoadKg;
                    part.AutoboxParameter.VolumeL = partDto.AutoboxParameter.VolumeL;
                    part.AutoboxParameter.OpeningSystem = partDto.AutoboxParameter.OpeningSystem;
                    part.AutoboxParameter.CountryOfOrigin = partDto.AutoboxParameter.CountryOfOrigin;
                    part.AutoboxParameter.Color = partDto.AutoboxParameter.Color;
                }
                else if (partDto.ProductTypeId == 2 && partDto.RoofRackParameter != null)
                {
                    if (part.RoofRackParameter == null)
                    {
                        part.RoofRackParameter = new RoofRackParameter { PartId = partId };
                    }
                    part.RoofRackParameter.LengthCm = partDto.RoofRackParameter.LengthCm;
                    part.RoofRackParameter.Material = partDto.RoofRackParameter.Material;
                    part.RoofRackParameter.LoadKg = partDto.RoofRackParameter.LoadKg;
                    part.RoofRackParameter.MountingType = partDto.RoofRackParameter.MountingType;
                    part.RoofRackParameter.CrossbarShape = partDto.RoofRackParameter.CrossbarShape;
                    part.RoofRackParameter.CountryOfOrigin = partDto.RoofRackParameter.CountryOfOrigin;
                    part.RoofRackParameter.Color = partDto.RoofRackParameter.Color;
                }
                else if (partDto.ProductTypeId == 3 && partDto.SparePartsParameter != null)
                {
                    if (part.SparePartsParameter == null)
                    {
                        part.SparePartsParameter = new SparePartsParameter { PartId = partId };
                    }
                    part.SparePartsParameter.CountryOfOrigin = partDto.SparePartsParameter.CountryOfOrigin;
                    part.SparePartsParameter.Color = partDto.SparePartsParameter.Color;
                }
                else
                {
                    throw new ArgumentException("Параметры не соответствуют указанному ProductTypeId");
                }

                if (partDto.Compatibilities != null)
                {
                    foreach (var c in partDto.Compatibilities)
                    {
                        if (!await _context.Brands.AnyAsync(b => b.BrandId == c.BrandId))
                            throw new ArgumentException($"Бренд с ID {c.BrandId} не существует");
                        if (!await _context.Models.AnyAsync(m => m.ModelId == c.ModelId))
                            throw new ArgumentException($"Модель с ID {c.ModelId} не существует");
                        if (!await _context.Generations.AnyAsync(g => g.GenerationId == c.GenerationId))
                            throw new ArgumentException($"Поколение с ID {c.GenerationId} не существует");
                        if (!await _context.BodytypesCars.AnyAsync(btc => btc.Bodytypeid == c.BodytypeId))
                            throw new ArgumentException($"Тип кузова с ID {c.BodytypeId} не существует");
                    }

                    _context.PartsAutos.RemoveRange(part.PartsAutos);
                    part.PartsAutos = partDto.Compatibilities.Select(c => new PartsAuto
                    {
                        PartId = partId,
                        BrandId = c.BrandId,
                        ModelId = c.ModelId,
                        GenerationId = c.GenerationId,
                        Bodytypeid = c.BodytypeId
                    }).ToList();
                }
                else
                {
                    _context.PartsAutos.RemoveRange(part.PartsAutos);
                    part.PartsAutos = new List<PartsAuto>();
                }

                await _context.SaveChangesAsync();

                return new PartDto
                {
                    PartId = part.PartId,
                    Name = part.Name,
                    Description = part.Description,
                    Price = part.Price,
                    StockQuantity = part.StockQuantity,
                    ProductTypeId = part.ProductTypeId,
                    ImageUrl = part.ImageUrl,
                    AutoboxParameter = part.AutoboxParameter != null ? new AutoboxParameterDto
                    {
                        DimensionsMm = part.AutoboxParameter.DimensionsMm,
                        LoadKg = part.AutoboxParameter.LoadKg,
                        VolumeL = part.AutoboxParameter.VolumeL,
                        OpeningSystem = part.AutoboxParameter.OpeningSystem,
                        CountryOfOrigin = part.AutoboxParameter.CountryOfOrigin,
                        Color = part.AutoboxParameter.Color
                    } : null,
                    RoofRackParameter = part.RoofRackParameter != null ? new RoofRackParameterDto
                    {
                        LengthCm = part.RoofRackParameter.LengthCm,
                        Material = part.RoofRackParameter.Material,
                        LoadKg = part.RoofRackParameter.LoadKg,
                        MountingType = part.RoofRackParameter.MountingType,
                        CrossbarShape = part.RoofRackParameter.CrossbarShape,
                        CountryOfOrigin = part.RoofRackParameter.CountryOfOrigin,
                        Color = part.RoofRackParameter.Color
                    } : null,
                    SparePartsParameter = part.SparePartsParameter != null ? new SparePartsParameterDto
                    {
                        CountryOfOrigin = part.SparePartsParameter.CountryOfOrigin,
                        Color = part.SparePartsParameter.Color
                    } : null,
                    Compatibilities = part.PartsAutos?.Select(pa => new PartAutoCompatibilityDto
                    {
                        BrandId = pa.BrandId,
                        ModelId = pa.ModelId,
                        GenerationId = pa.GenerationId,
                        BodytypeId = pa.Bodytypeid
                    }).ToList()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении запчасти номер {partId}: {ex.Message}");
                throw;
            }
        }
    }
}
