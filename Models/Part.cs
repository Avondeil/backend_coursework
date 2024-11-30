using System;
using System.Collections.Generic;

namespace api_details.Models;

public partial class Part
{
    public int PartId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int? StockQuantity { get; set; }

    public int? BrandId { get; set; }

    public int? ModelId { get; set; }

    public int? GenerationId { get; set; }

    public int? ProductTypeId { get; set; }

    public string? ImageUrl { get; set; }

    public virtual Brand? Brand { get; set; }
    public virtual Model? Model { get; set; }

    public virtual AutoboxParameter? AutoboxParameter { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ProductType? ProductType { get; set; }

    public virtual RoofRackParameter? RoofRackParameter { get; set; }

    public virtual SparePartsParameter? SparePartsParameter { get; set; }

    public virtual ICollection<UserHistory> UserHistories { get; set; } = new List<UserHistory>();
}
