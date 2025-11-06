using System.ComponentModel.DataAnnotations;

namespace AssetManagement.API.Models
{
    /// <summary>
    /// Data Transfer Object for user registration.
    /// </summary>
    public class RegisterDto
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "The password must be at least 6 characters long.")]
        public string Password { get; set; }

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; }

        public long SupervisorId { get; set; }

        public bool IsAssetManager { get; set; }
    }
}