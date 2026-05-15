using System;
using System.ComponentModel.DataAnnotations;

namespace projektLana
{
    public class ActivityFormModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Name must be between 3 and 200 characters.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Activity type is required.")]
        public ActivityType? TypeOfActivity { get; set; }

        [Required(ErrorMessage = "Date is required.")]
        public DateTime? Date { get; set; }

        [Required(ErrorMessage = "Cost is required.")]
        [Range(0, 100000, ErrorMessage = "Cost must be 0 or more.")]
        public decimal? Cost { get; set; }

        [Required(ErrorMessage = "Destination is required.")]
        public int? DestinationId { get; set; }

        public string? DestinationDisplayName { get; set; }
    }
}
