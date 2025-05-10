using System.ComponentModel.DataAnnotations;

namespace api_details.DataTransfer
{
    public class PartAutoCompatibilityDto
    {
        [Required(ErrorMessage = "BrandId обязателен")]
        public int BrandId { get; set; }
        public string? BrandName { get; set; }

        [Required(ErrorMessage = "ModelId обязателен")]
        public int ModelId { get; set; }
        public string? ModelName { get; set; }

        [Required(ErrorMessage = "GenerationId обязателен")]
        public int GenerationId { get; set; }
        public string? GenerationYear { get; set; }

        [Required(ErrorMessage = "BodytypeId обязателен")]
        public int BodytypeId { get; set; }
        public string? BodytypeName { get; set; }
    }
}
