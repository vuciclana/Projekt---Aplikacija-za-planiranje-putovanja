using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace projektLana
{
    [Table("Reviews")]
    public class Review
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; }

        [StringLength(1000)]
        public String? Comment { get; set; }

        public bool IsDeleted { get; set; } = false;

        // Foreign keys
        [ForeignKey(nameof(User))]
        public int UserId { get; set; }

        [ForeignKey(nameof(Destination))]
        public int DestinationId { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!; 
        public virtual Destination Destination { get; set; } = null!;
        public string Stars => new string('★', Rating) + new string('☆', 5 - Rating);
        public bool IsPositive => Rating >= 4;
        public string ReviewerName => User != null ? $"{User.FirstName} {User.LastName}" : "Anonymous";
    }
}
