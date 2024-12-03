using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace api_details.Models;

public partial class ProductType
{
    public int ProductTypeId { get; set; }

    public string Name { get; set; } = null!;
    [JsonIgnore]
    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();
}
