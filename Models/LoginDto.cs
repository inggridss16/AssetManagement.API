using System.ComponentModel.DataAnnotations;

namespace AssetManagement.API.Models
{
    /// <summary>
    /// Data Transfer Object for user login.
    /// </summary>
    public class LoginDto
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}