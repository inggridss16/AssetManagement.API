// Models/MaintenanceUpdateDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace AssetManagement.API.Models
{
    /// <summary>
    /// DTO for updating an existing maintenance record.
    /// </summary>
    public class MaintenanceUpdateDto
    {
        [Required]
        [Range(0, double.MaxValue)]
        public decimal MaintenanceCost { get; set; }

        [Required]
        [StringLength(100)]
        public string MaintenanceType { get; set; }

        [StringLength(1000)]
        public string Comments { get; set; }

        [StringLength(100)]
        public string Vendor { get; set; }

        [Required]
        public DateTime MaintenanceDate { get; set; }
    }
}