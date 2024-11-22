using System;
using System.Collections.Generic;

namespace api_details.Models;

public partial class Model
{
    public int ModelId { get; set; }

    public int? BrandId { get; set; }

    public string Name { get; set; } = null!;

    public virtual Brand? Brand { get; set; }

    public virtual ICollection<Generation> Generations { get; set; } = new List<Generation>();

    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();
}
