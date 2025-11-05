// Models/MaintenanceCreateDto.cs
using System;
using System.ComponentModel.DataAnnotations;

namespace AssetManagement.API.Models
{
    /// <summary>
    /// DTO for creating a new maintenance record.
    /// </summary>
    public class MaintenanceCreateDto
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