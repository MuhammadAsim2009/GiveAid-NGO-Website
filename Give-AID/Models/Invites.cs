using System.ComponentModel.DataAnnotations;

namespace Give_AID.Models
{
    // Model representing an email invitation sent by a member to a friend
    public class Invites
    {
        // Primary key
        public int Id { get; set; }

        // Foreign key linking to the inviting user
        [Required]
        public int UserId { get; set; }

        // Recipient email address of the invited friend
        [Required(ErrorMessage = "Friend's email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        [StringLength(150, ErrorMessage = "Email cannot exceed 150 characters.")]
        public string FriendEmail { get; set; } = string.Empty;

        // Date and time when the invitation was dispatched
        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // Navigation property to the inviting user
        public Users? User { get; set; }
    }
}

