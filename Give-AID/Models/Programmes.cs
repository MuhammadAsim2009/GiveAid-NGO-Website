using System.ComponentModel.DataAnnotations;

namespace Give_AID.Models
{
    // Model representing a welfare programme or event organized by an NGO
    public class Programmes
    {
        // Primary key
        public int Id { get; set; }

        // Title of the welfare programme
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        // Category such as Education, Health Care, Specially Challenged Children
        [Required(ErrorMessage = "Category is required.")]
        [StringLength(100, ErrorMessage = "Category cannot exceed 100 characters.")]
        public string Category { get; set; } = string.Empty;

        // Detailed overview of the programme objectives and activities
        [Required(ErrorMessage = "Description is required.")]
        public string Description { get; set; } = string.Empty;

        // Date when the event or programme takes place
        [Required(ErrorMessage = "Event date is required.")]
        public DateTime EventDate { get; set; }

        // Foreign key linking to the organizing NGO
        [Required]
        public int NgoId { get; set; }

        // Navigation property to the hosting NGO
        public Ngos? Ngo { get; set; }

        // Navigation property for user expressions of interest
        public ICollection<ProgrammeInterests> ProgrammeInterests { get; set; } = new List<ProgrammeInterests>();

        // Navigation property for photos associated with this programme
        public ICollection<GalleryImages> GalleryImages { get; set; } = new List<GalleryImages>();
    }
}

