using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace api_details.Models;

public partial class SparePartsParameter
{
    public int PartId { get; set; }

    public string? CountryOfOrigin { get; set; }

    public string? Color { get; set; }

    [JsonIgnore]
    public virtual Part Part { get; set; } = null!;
}
