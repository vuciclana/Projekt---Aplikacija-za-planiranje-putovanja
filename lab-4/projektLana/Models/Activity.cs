using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace projektLana 
{
    [Table("Activities")]
    public class Activity
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public String Name { get; set; } = null!;

        [Required]
        public ActivityType TypeOfActivity { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [Required]
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Cost { get; set; }

        // Foreign key
        [ForeignKey(nameof(Destination))]
        public int DestinationId { get; set; }

        // Navigation property
        public virtual Destination Destination { get; set; } = null!;
        public bool IsFree => Cost == 0;
        public string PriceLabel => IsFree ? "Free" : $"{Cost:0.00} €";
        public bool IsPremium => Cost >= 50;
    }
}
