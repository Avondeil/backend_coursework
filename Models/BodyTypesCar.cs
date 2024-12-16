using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace api_details.Models;

public partial class BodytypesCar
{
    public int Bodytypeid { get; set; }

    public int GenerationId { get; set; }

    public int BodyTypeId { get; set; }

    [JsonIgnore]
    public virtual BodyType BodyType { get; set; } = null!;

    [JsonIgnore]
    public virtual Generation Generation { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<PartsAuto> PartsAutos { get; set; } = new List<PartsAuto>();
}
