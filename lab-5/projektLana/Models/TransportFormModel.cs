using System;
using System.ComponentModel.DataAnnotations;

namespace projektLana
{
    public class TransportFormModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Transport type is required.")]
        public TransportType? Type { get; set; }

        [Required(ErrorMessage = "Cost is required.")]
        [Range(0, 100000, ErrorMessage = "Cost must be 0 or more.")]
        public decimal? Cost { get; set; }

        [Required(ErrorMessage = "Departure time is required.")]
        public DateTime? DepartureTime { get; set; }

        [Required(ErrorMessage = "Arrival time is required.")]
        public DateTime? ArrivalTime { get; set; }

        [Required(ErrorMessage = "Destination is required.")]
        public int? DestinationId { get; set; }

        public string? DestinationDisplayName { get; set; }
    }
}
