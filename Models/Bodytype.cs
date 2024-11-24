using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace api_details.Models;

public partial class Bodytype
{
    public int Bodytypeid { get; set; }

    public string Name { get; set; } = null!;

    public int BrandId { get; set; }

    public int ModelId { get; set; }

    public int GenerationId { get; set; }

    [JsonIgnore]
    public virtual Brand Brand { get; set; } = null!;

    [JsonIgnore]
    public virtual Generation Generation { get; set; } = null!;

    [JsonIgnore]
    public virtual Model Model { get; set; } = null!;
}
