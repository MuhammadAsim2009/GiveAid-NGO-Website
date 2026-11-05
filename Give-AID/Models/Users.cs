using System.ComponentModel.DataAnnotations;

namespace Give_AID.Models
{
    // Model representing registered system users (Members and Admins)
    public class Users
    {
        // Primary key
        public int Id { get; set; }

        // Full name of the user
        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        // Email address used for authentication and notifications
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string Email { get; set; } = string.Empty;

        // Securely hashed password (never stored in plain text)
        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = string.Empty;

        // User role: "Admin" or "User"
        [Required]
        [StringLength(20)]
        public string Role { get; set; } = "User";

        // Personal contact phone number
        [Phone(ErrorMessage = "Please enter a valid phone number.")]
        [StringLength(20)]
        public string? Phone { get; set; }

        // Residential or mailing address
        [StringLength(250)]
        public string? Address { get; set; }

        // Professional occupation of the user
        [StringLength(100)]
        public string? Occupation { get; set; }

        // Employer or organization name
        [StringLength(150)]
        public string? Organization { get; set; }

        // Navigation collection of donations made by this user
        public ICollection<Donations> Donations { get; set; } = new List<Donations>();

        // Navigation collection of programme interests registered by this user
        public ICollection<ProgrammeInterests> ProgrammeInterests { get; set; } = new List<ProgrammeInterests>();

        // Navigation collection of queries submitted by this user
        public ICollection<Queries> Queries { get; set; } = new List<Queries>();

        // Navigation collection of invitations sent by this user
        public ICollection<Invites> Invites { get; set; } = new List<Invites>();
    }
}
