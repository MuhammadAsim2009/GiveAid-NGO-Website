using System.ComponentModel.DataAnnotations;

namespace Give_AID.Models
{
    // Model representing CMS managed content for static pages (About Us sub-pages, Help Centre, Contact)
    public class PageContents
    {
        // Primary key
        public int Id { get; set; }

        // Unique identifier slug for routing (e.g., 'what-we-do', 'our-mission', 'help-centre')
        [Required(ErrorMessage = "Slug is required.")]
        [StringLength(100, ErrorMessage = "Slug cannot exceed 100 characters.")]
        public string Slug { get; set; } = string.Empty;

        // Display title of the page section
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        // Full content or body markup for the page
        [Required(ErrorMessage = "Content body is required.")]
        public string Body { get; set; } = string.Empty;

        // Last updated timestamp
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}

