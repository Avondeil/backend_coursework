using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace api_details.Models;

public partial class Brand
{
    public int BrandId { get; set; }

    public string Name { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<Model> Models { get; set; } = new List<Model>();

    [JsonIgnore]
    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();
}
