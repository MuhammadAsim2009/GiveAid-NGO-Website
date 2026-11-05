using System.ComponentModel.DataAnnotations;

namespace Give_AID.Models
{
    // Model representing a user's expressed interest or participation in a programme
    public class ProgrammeInterests
    {
        // Primary key
        public int Id { get; set; }

        // Foreign key linking to the interested user
        [Required]
        public int UserId { get; set; }

        // Foreign key linking to the selected programme
        [Required]
        public int ProgrammeId { get; set; }

        // Optional note or comment from the user explaining how they want to participate
        [StringLength(1000, ErrorMessage = "Message cannot exceed 1000 characters.")]
        public string? Message { get; set; }

        // Date and time when the interest was registered
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property to the user
        public Users? User { get; set; }

        // Navigation property to the programme
        public Programmes? Programme { get; set; }
    }
}

