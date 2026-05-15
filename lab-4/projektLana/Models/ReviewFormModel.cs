using System.ComponentModel.DataAnnotations;

namespace projektLana
{
    public class ReviewFormModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Rating is required.")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int? Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Comment must be at most 1000 characters.")]
        public string? Comment { get; set; }

        [Required(ErrorMessage = "User is required.")]
        public int? UserId { get; set; }

        public string? UserDisplayName { get; set; }

        [Required(ErrorMessage = "Destination is required.")]
        public int? DestinationId { get; set; }

        public string? DestinationDisplayName { get; set; }
    }
}
