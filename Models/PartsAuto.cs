using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace api_details.Models;

public partial class PartsAuto
{
    public int PartId { get; set; }

    public int BrandId { get; set; }

    public int ModelId { get; set; }

    public int GenerationId { get; set; }

    public int Bodytypeid { get; set; }

    [JsonIgnore]
    public virtual BodytypesCar Bodytype { get; set; } = null!;
    [JsonIgnore]
    public virtual Brand Brand { get; set; } = null!;
    [JsonIgnore]
    public virtual Generation Generation { get; set; } = null!;
    [JsonIgnore]
    public virtual Model Model { get; set; } = null!;
    [JsonIgnore]
    public virtual Part Part { get; set; } = null!;
}
