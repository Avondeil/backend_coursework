using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace api_details.Models;

public partial class Model
{
    public int ModelId { get; set; }

    public int? BrandId { get; set; }

    public string Name { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Bodytype> Bodytypes { get; set; } = new List<Bodytype>();

    [JsonIgnore]
    public virtual Brand? Brand { get; set; }

    [JsonIgnore]
    public virtual ICollection<Generation> Generations { get; set; } = new List<Generation>();

    [JsonIgnore]
    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();
}
