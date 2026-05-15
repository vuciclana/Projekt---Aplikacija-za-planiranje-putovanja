using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace projektLana
{
    [Table("Accommodations")]
    public class Accommodation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public String Name { get; set; } = null!;

        [Required]
        public AccommodationType Type { get; set; }

        [Required]
        [StringLength(300)]
        public String Address { get; set; } = null!;

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal CostPerNight { get; set; }

        [Required]
        public DateTime CheckInDate { get; set; }

        [Required]
        public DateTime CheckOutDate { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Foreign key
        [ForeignKey(nameof(Destination))]
        public int DestinationId { get; set; }

        // Navigation property
        public virtual Destination Destination { get; set; } = null!;
        public int NumberOfNights => (CheckOutDate - CheckInDate).Days > 0 ? (CheckOutDate - CheckInDate).Days : 1;
        public decimal TotalCost => CostPerNight * NumberOfNights;
        public string StayPeriod => $"{CheckInDate:dd.MM.yyyy} - {CheckOutDate:dd.MM.yyyy}";
    }
}
