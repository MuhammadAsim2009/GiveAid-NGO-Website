using System.ComponentModel.DataAnnotations;

namespace Give_AID.Models
{
    // Model representing a corporate or organizational partner
    public class Partners
    {
        // Primary key
        public int Id { get; set; }

        // Name of the partner company or sponsor
        [Required(ErrorMessage = "Partner name is required.")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
        public string Name { get; set; } = string.Empty;

        // File path to the partner logo image
        [Required(ErrorMessage = "Logo path is required.")]
        public string LogoPath { get; set; } = string.Empty;

        // Website URL of the partner organization
        [Url(ErrorMessage = "Please enter a valid website URL.")]
        public string? Website { get; set; }
    }
}

