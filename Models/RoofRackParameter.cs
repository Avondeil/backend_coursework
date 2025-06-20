﻿using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace api_details.Models;

public partial class RoofRackParameter
{
    public int PartId { get; set; }

    public int? LengthCm { get; set; }

    public string? Material { get; set; }

    public int? LoadKg { get; set; }

    public string? MountingType { get; set; }

    public string? CrossbarShape { get; set; }

    public string? CountryOfOrigin { get; set; }

    public string? Color { get; set; }

    [JsonIgnore]
    public virtual Part Part { get; set; } = null!;
}
