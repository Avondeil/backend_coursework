using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace api_details.Models;

public partial class BodyTypesCar
{
    [JsonPropertyName("body_type_car_id")]
    public int Bodytypeid { get; set; }

    public int BrandId { get; set; }

    public int ModelId { get; set; }

    public int GenerationId { get; set; }

    [JsonPropertyName("body_type_id")]
    public int BodyTypeId { get; set; }

    [JsonIgnore]
    public virtual BodyType BodyType { get; set; } = null!;

    [JsonIgnore]
    public virtual Brand Brand { get; set; } = null!;

    [JsonIgnore]
    public virtual Generation Generation { get; set; } = null!;

    [JsonIgnore]
    public virtual Model Model { get; set; } = null!;
}
