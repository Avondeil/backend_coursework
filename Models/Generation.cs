using System;
using System.Collections.Generic;

namespace api_details.Models;

public partial class Generation
{
    public int GenerationId { get; set; }

    public int? ModelId { get; set; }

    public int? Year { get; set; }

    public virtual Model? Model { get; set; }

    public virtual ICollection<Part> Parts { get; set; } = new List<Part>();
}
