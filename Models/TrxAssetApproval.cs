using System;
using System.Collections.Generic;

namespace AssetManagement.API.Models;

public partial class TrxAssetApproval
{
    public int Id { get; set; }

    public string AssetId { get; set; } = null!;

    public long ApproverId { get; set; }

    public string Status { get; set; } = null!;

    public string? Comments { get; set; }

    public DateTime? ApprovalDate { get; set; }

    public DateTime CreatedDate { get; set; }

    public long CreatedBy { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public long? UpdatedBy { get; set; }
}
