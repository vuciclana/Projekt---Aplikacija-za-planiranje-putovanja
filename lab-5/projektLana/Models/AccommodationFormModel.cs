using System;
using System.ComponentModel.DataAnnotations;

namespace projektLana
{
    public class AccommodationFormModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 200 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Type is required.")]
        public AccommodationType? Type { get; set; }

        [Required(ErrorMessage = "Address is required.")]
        [StringLength(300, ErrorMessage = "Address must be at most 300 characters.")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Cost per night is required.")]
        [Range(0, 100000, ErrorMessage = "Cost per night must be 0 or more.")]
        public decimal? CostPerNight { get; set; }

        [Required(ErrorMessage = "Check-in date is required.")]
        public DateTime? CheckInDate { get; set; }

        [Required(ErrorMessage = "Check-out date is required.")]
        public DateTime? CheckOutDate { get; set; }

        [Required(ErrorMessage = "Destination is required.")]
        public int? DestinationId { get; set; }

        public string? DestinationDisplayName { get; set; }
    }
}
