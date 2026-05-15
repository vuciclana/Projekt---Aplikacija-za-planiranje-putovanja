using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace projektLana 
{
    [Table("Transports")]
    public class Transport 
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public TransportType Type { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Cost { get; set; }

        [Required]
        public DateTime DepartureTime { get; set; }

        [Required]
        public DateTime ArrivalTime { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Foreign key
        [ForeignKey(nameof(Destination))]
        public int DestinationId { get; set; }

        // Navigation property
        public virtual Destination Destination { get; set; } = null!;
        public TimeSpan Duration => ArrivalTime - DepartureTime;
        public string RouteTime => $"{DepartureTime:dd.MM.yyyy HH:mm} - {ArrivalTime:HH:mm}";
        public bool IsLongTrip => Duration.TotalHours > 5;
    }
}
