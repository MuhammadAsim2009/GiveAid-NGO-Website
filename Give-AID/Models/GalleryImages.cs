using System.ComponentModel.DataAnnotations;

namespace Give_AID.Models
{
    // Model representing an image displayed in the public media gallery
    public class GalleryImages
    {
        // Primary key
        public int Id { get; set; }

        // Optional foreign key linking the image to a specific welfare programme
        public int? ProgrammeId { get; set; }

        // Caption or title describing the photo
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
        public string Title { get; set; } = string.Empty;

        // Path or URL to the uploaded image file
        [Required(ErrorMessage = "Image path is required.")]
        public string ImagePath { get; set; } = string.Empty;

        // Navigation property to the related programme
        public Programmes? Programme { get; set; }
    }
}

