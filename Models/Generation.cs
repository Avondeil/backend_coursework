using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace api_details.Models;

public partial class Generation
{
    public int GenerationId { get; set; }

    public int? ModelId { get; set; }

    public int? Year { get; set; }

    [JsonIgnore]
    public virtual ICollection<Bodytype> Bodytypes { get; set; } = new List<Bodytype>();

    [JsonIgnore]
    public virtual Model? Model { get; set; }

    [JsonIgnore]
    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();
}
