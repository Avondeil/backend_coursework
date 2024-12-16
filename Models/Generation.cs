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
    public virtual ICollection<BodytypesCar> BodytypesCars { get; set; } = new List<BodytypesCar>();

    [JsonIgnore]
    public virtual Model Model { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<PartsAuto> PartsAutos { get; set; } = new List<PartsAuto>();
}
