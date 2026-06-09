using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace projektLana
{  
    [Table("Trips")]
    public class Trip
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Trip name is required.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Trip name must be between 3 and 200 characters.")]
        public String Name { get; set; } = null!;

        [Required(ErrorMessage = "Start date is required.")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "End date is required.")]
        public DateTime EndDate { get; set; }

        // Foreign key
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual ICollection<Destination> Destinations { get; set; } = new List<Destination>();

        // Soft delete flag
        public bool IsDeleted { get; set; } = false;

        public decimal TotalCost
        {
            get
            {
                decimal total = 0;

                foreach (var destination in Destinations)
                {
                    foreach (var activity in destination.Activities)
                    {
                        total += activity.Cost;
                    }

                    foreach (var accommodation in destination.Accommodations)
                    {
                        int nights = (accommodation.CheckOutDate - accommodation.CheckInDate).Days;
                        total += accommodation.CostPerNight * nights;
                    }

                    foreach (var transport in destination.Transports)
                    {
                        total += transport.Cost;
                    }
                }

                return total;
            }
        }
    }
}
