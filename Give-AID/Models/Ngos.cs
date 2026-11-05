using System.ComponentModel.DataAnnotations;

namespace Give_AID.Models
{
    // Model representing an associated NGO running welfare programmes
    public class Ngos
    {
        // Primary key
        public int Id { get; set; }

        // Name of the NGO organization
        [Required(ErrorMessage = "NGO name is required.")]
        [StringLength(150, ErrorMessage = "Name cannot exceed 150 characters.")]
        public string Name { get; set; } = string.Empty;

        // Information about the NGO and its work
        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        // Path or filename for the NGO logo image
        public string? Logo { get; set; }

        // Contact information such as email, phone, or address
        [Required(ErrorMessage = "Contact info is required.")]
        [StringLength(200, ErrorMessage = "Contact info cannot exceed 200 characters.")]
        public string Contact { get; set; } = string.Empty;

        // Navigation property for welfare programmes conducted by this NGO
        public ICollection<Programmes> Programmes { get; set; } = new List<Programmes>();

        // Navigation property for direct donations received by this NGO
        public ICollection<Donations> Donations { get; set; } = new List<Donations>();
    }
}

