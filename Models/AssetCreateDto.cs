// AssetCreateDto.cs
using System.ComponentModel.DataAnnotations;

namespace AssetManagement.API.Models
{
    /// <summary>
    /// Data Transfer Object for creating a new asset.
    /// </summary>
    public class AssetCreateDto
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
    }
}