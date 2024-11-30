using System;
using System.Collections.Generic;

namespace api_details.Models;

public partial class ProductType
{
    public int ProductTypeId { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();
}
