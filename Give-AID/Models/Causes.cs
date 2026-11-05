using System.ComponentModel.DataAnnotations;

namespace Give_AID.Models
{
    // Model representing a charitable cause that users can donate to
    public class Causes
    {
        // Primary key
        public int Id { get; set; }

        // Name of the cause (e.g., Education, Healthcare, Child Welfare)
        [Required(ErrorMessage = "Cause name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        // Detailed description of the cause
        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        // Status indicating if the cause is currently accepting donations
        public bool IsActive { get; set; } = true;

        // Navigation property for all donations received for this cause
        public ICollection<Donations> Donations { get; set; } = new List<Donations>();
    }
}

