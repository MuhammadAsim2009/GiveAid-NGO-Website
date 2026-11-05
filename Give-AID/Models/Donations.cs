using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Give_AID.Models
{
    // Model representing a financial donation made by a user
    public class Donations
    {
        // Primary key
        public int Id { get; set; }

        // Foreign key linking to the donor (Users)
        [Required]
        public int UserId { get; set; }

        // Foreign key linking to the selected charitable cause
        [Required]
        public int CauseId { get; set; }

        // Optional foreign key if donation is directed to a specific NGO
        public int? NgoId { get; set; }

        // Donated amount in currency units
        [Required(ErrorMessage = "Donation amount is required.")]
        [Range(1, 1000000, ErrorMessage = "Donation must be between 1 and 1,000,000.")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        // Last 4 digits of the payment card (full card numbers are never stored)
        [Required]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "CardLast4 must be 4 digits.")]
        public string CardLast4 { get; set; } = string.Empty;

        // Status of the payment (e.g., Completed, Pending, Failed)
        [Required]
        [StringLength(50)]
        public string Status { get; set; } = "Completed";

        // Date and time when the donation was made
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property to the donor user
        public Users? User { get; set; }

        // Navigation property to the cause
        public Causes? Cause { get; set; }

        // Navigation property to the optional NGO
        public Ngos? Ngo { get; set; }
    }
}

