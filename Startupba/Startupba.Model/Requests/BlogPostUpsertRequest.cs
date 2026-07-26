using System.ComponentModel.DataAnnotations;

namespace Startupba.Model.Requests
{
    public class BlogPostUpsertRequest
    {
        [Required]
        public int AuthorId { get; set; }

        public int? StartupId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(4000)]
        public string Content { get; set; } = string.Empty;

        public byte[]? ImageData { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
