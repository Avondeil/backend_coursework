using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace api_details.Models;

public partial class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string? Phone { get; set; }

    public DateTime? CreatedAt { get; set; }

    public bool? StatusAdmin { get; set; }

    [JsonIgnore]
    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
