using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace projektLana
{
    [Table("Destinations")]
    public class Destination  
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public String City { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public String Country { get; set; } = null!;

        [StringLength(500)]
        public String Description { get; set; } = null!;

        public bool IsDeleted { get; set; } = false;

        // Foreign key for Trip
        [ForeignKey(nameof(Trip))]
        public int TripId { get; set; }

        // Navigation properties
        public virtual Trip Trip { get; set; } = null!;
        public virtual ICollection<Activity> Activities { get; set; } = new List<Activity>();
        public virtual ICollection<Accommodation> Accommodations { get; set; } = new List<Accommodation>();
        public virtual ICollection<Transport> Transports { get; set; } = new List<Transport>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public decimal TotalDestinationCost => 
            Activities.Sum(a => a.Cost) + 
            Transports.Sum(t => t.Cost) + 
            Accommodations.Sum(a => a.TotalCost);
            
        public int ActivityCount => Activities.Count;
        public bool HasAccommodation => Accommodations.Any();
        public string Location => $"{City}, {Country}";
    }
}
