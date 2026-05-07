using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace projektLana
{
    [Table("Users")]
    public class User
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public String FirstName { get; set; } = null!;

        [Required]
        [StringLength(100)]
        public String LastName { get; set; } = null!;

        [Required]
        [EmailAddress]
        [StringLength(255)]
        public String Email { get; set; } = null!;

        // Navigation properties
        public virtual ICollection<Trip> Trips { get; set; } = new List<Trip>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
