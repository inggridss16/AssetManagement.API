using System;
using System.Collections.Generic;

namespace AssetManagement.API.Models;

public partial class TrxAsset
{
    public string Id { get; set; } = null!;

    public string AssetName { get; set; } = null!;

    public string Description { get; set; } = null!;

    public long RequesterId { get; set; }

    public long ResponsiblePersonId { get; set; }

    public string Category { get; set; } = null!;

    public string Subcategory { get; set; } = null!;

    public string Status { get; set; } = null!;

    public decimal AssetValue { get; set; }

    public DateTime CreatedDate { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public long? UpdatedBy { get; set; }
}
