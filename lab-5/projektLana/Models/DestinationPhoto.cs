using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace projektLana
{
    [Table("DestinationPhotos")]
    public class DestinationPhoto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [StringLength(255)]
        public string OriginalFileName { get; set; } = string.Empty;

        [StringLength(255)]
        public string StoredFileName { get; set; } = string.Empty;

        [StringLength(100)]
        public string ContentType { get; set; } = string.Empty;

        [StringLength(500)]
        public string FilePath { get; set; } = string.Empty;

        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        public int? DestinationId { get; set; }

        public Guid? UploadSessionId { get; set; }

        public virtual Destination? Destination { get; set; }
    }
}
