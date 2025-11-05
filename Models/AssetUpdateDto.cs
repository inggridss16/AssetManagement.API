// Models/AssetUpdateDto.cs
using System.ComponentModel.DataAnnotations;

namespace AssetManagement.API.Models
{
    /// <summary>
    /// Data Transfer Object for updating an existing asset.
    /// </summary>
    public class AssetUpdateDto
    {
        [Required]
        [StringLength(100)]
        public string AssetName { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public long ResponsiblePersonId { get; set; }

        [Required]
        [StringLength(100)]
        public string Category { get; set; }

        [Required]
        [StringLength(100)]
        public string Subcategory { get; set; }

        [Required]
        [StringLength(100)]
        public string Status { get; set; }
    }
}