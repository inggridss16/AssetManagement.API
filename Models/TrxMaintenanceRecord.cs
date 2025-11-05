using System;
using System.Collections.Generic;

namespace AssetManagement.API.Models;

public partial class TrxMaintenanceRecord
{
    public long Id { get; set; }

    public string LinkedAssetId { get; set; } = null!;

    public decimal MaintenanceCost { get; set; }

    public string MaintenanceType { get; set; } = null!;

    public string Comments { get; set; } = null!;

    public string Vendor { get; set; } = null!;

    public DateTime MaintenanceDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public long? UpdatedBy { get; set; }
}
