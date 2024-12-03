using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace api_details.Models;

public partial class Part
{
    public int PartId { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int? StockQuantity { get; set; }


    public int? ProductTypeId { get; set; }

    public string? ImageUrl { get; set; }

    [JsonIgnore]
    public virtual AutoboxParameter? AutoboxParameter { get; set; }

    [JsonIgnore]
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    [JsonIgnore]
    public virtual ProductType? ProductType { get; set; }

    [JsonIgnore]
    public virtual RoofRackParameter? RoofRackParameter { get; set; }

    [JsonIgnore]
    public virtual SparePartsParameter? SparePartsParameter { get; set; }

    [JsonIgnore]
    public virtual ICollection<UserHistory> UserHistories { get; set; } = new List<UserHistory>();
}
