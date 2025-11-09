using System;

namespace AssetManagement.API.Models
{
    public class TrxAssetApprovalDto
    {
        public string ApproverName { get; set; }
        public string Status { get; set; }
        public string? Comments { get; set; }
        public DateTime? ApprovalDate { get; set; }
    }
}