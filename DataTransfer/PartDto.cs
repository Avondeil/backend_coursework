using System.Text.Json.Serialization;

namespace api_details.DataTransfer
{
    public class PartDto
    {
        public int? PartId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int? StockQuantity { get; set; }
        public int? ProductTypeId { get; set; }
        public string? ImageUrl { get; set; }
        public List<PartAutoCompatibilityDto> Compatibilities { get; set; } = new List<PartAutoCompatibilityDto>();
        public AutoboxParameterDto? AutoboxParameter { get; set; }
        public RoofRackParameterDto? RoofRackParameter { get; set; }
        public SparePartsParameterDto? SparePartsParameter { get; set; }
    }

}
