using System.ComponentModel.DataAnnotations;

namespace Give_AID.Models
{
    // Model representing a user question, support request, or query
    public class Queries
    {
        // Primary key
        public int Id { get; set; }

        // Foreign key linking to the user who submitted the query
        [Required]
        public int UserId { get; set; }

        // Brief subject or title of the query
        [Required(ErrorMessage = "Subject is required.")]
        [StringLength(200, ErrorMessage = "Subject cannot exceed 200 characters.")]
        public string Subject { get; set; } = string.Empty;

        // Full query text sent by the user
        [Required(ErrorMessage = "Message is required.")]
        public string Message { get; set; } = string.Empty;

        // Response text provided by an administrator
        public string? Reply { get; set; }

        // Status of the query (e.g., Open, In Progress, Resolved)
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Open";

        // Date and time when the query was submitted
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property to the user who raised the query
        public Users? User { get; set; }
    }
}

