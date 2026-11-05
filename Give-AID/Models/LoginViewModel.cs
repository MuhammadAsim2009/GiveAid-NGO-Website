using System.ComponentModel.DataAnnotations;

namespace Give_AID.Models
{
    // View model used for user and admin login
    public class LoginViewModel
    {
        // User email address
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        // User password
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        // Optional return URL after successful authentication
        public string? ReturnUrl { get; set; }
    }
}

