using System;
using System.Collections.Generic;

namespace api_details.Models;

public partial class UserHistory
{
    public int HistoryId { get; set; }

    public int? UserId { get; set; }

    public int? PartId { get; set; }

    public DateTime? PurchaseDate { get; set; }

    public virtual Part? Part { get; set; }

    public virtual User? User { get; set; }
}
