using System;
using System.ComponentModel.DataAnnotations;

namespace projektLana
{
    public class TripFormModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Trip name is required.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Trip name must be between 3 and 200 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start date is required.")]
        public DateTime? StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        public DateTime? EndDate { get; set; }

        [Required(ErrorMessage = "Traveler is required.")]
        public int? UserId { get; set; }

        public string? UserDisplayName { get; set; }
    }
}