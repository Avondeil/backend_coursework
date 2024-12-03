using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace api_details.Models;

public partial class AutoboxParameter
{
    public int PartId { get; set; }

    public string? DimensionsMm { get; set; }

    public int? LoadKg { get; set; }

    public int? VolumeL { get; set; }

    public string? OpeningSystem { get; set; }

    public string? CountryOfOrigin { get; set; }

    public string? Color { get; set; }

    [JsonIgnore]
    public virtual Part Part { get; set; } = null!;
}
