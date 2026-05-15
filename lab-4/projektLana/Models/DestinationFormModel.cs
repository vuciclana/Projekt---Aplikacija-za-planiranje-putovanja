using System.ComponentModel.DataAnnotations;

namespace projektLana
{
    public class DestinationFormModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "City is required.")]
        [StringLength(100, ErrorMessage = "City must be at most 100 characters.")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required.")]
        [StringLength(100, ErrorMessage = "Country must be at most 100 characters.")]
        public string Country { get; set; } = string.Empty;

        [StringLength(500, ErrorMessage = "Description must be at most 500 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Trip is required.")]
        public int? TripId { get; set; }

        public string? TripDisplayName { get; set; }
    }
}
