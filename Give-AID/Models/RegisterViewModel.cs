using System.ComponentModel.DataAnnotations;

namespace Give_AID.Models
{
    // View model used for new user registration capturing personal and professional details
    public class RegisterViewModel
    {
        // Full name
        [Required(ErrorMessage = "Full name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        [Display(Name = "Full Name")]
        public string Name { get; set; } = string.Empty;

        // Email address
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        // Password
        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long.")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        // Confirm Password
        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        // Personal Contact Phone
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string? Phone { get; set; }

        // Residential / Mailing Address
        [StringLength(250)]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        // Professional Occupation
        [StringLength(100)]
        [Display(Name = "Occupation / Profession")]
        public string? Occupation { get; set; }

        // Organization / Company Name
        [StringLength(150)]
        [Display(Name = "Organization / Company")]
        public string? Organization { get; set; }
    }
}

