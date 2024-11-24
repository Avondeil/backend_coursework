using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace api_details.Models;

public partial class Generation
{
    public int GenerationId { get; set; }

    public int ModelId { get; set; }

    public string Year { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<BodyTypesCar> BodyTypesCars { get; set; } = new List<BodyTypesCar>();

    [JsonIgnore]
    public virtual Model Model { get; set; } = null!;
}
