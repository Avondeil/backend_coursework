namespace api_details.DataTransfer
{
    public class FilterDTO
    {
        public int? BrandId { get; set; }
        public int? ModelId { get; set; }
        public int? GenerationId { get; set; }
        public int? BodyTypeId { get; set; }
        public string Size { get; set; }
        public string Weight { get; set; }
        public string Color { get; set; }
        public string Country { get; set; }
        public int? ProductTypeId { get; set; } // Если нужно фильтровать по типу продукта (автобоксы, багажники, запчасти)
    }

}
