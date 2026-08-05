using System.ComponentModel.DataAnnotations;

namespace Startupba.Model.Requests
{
    public class BlogPostUpsertRequest
    {
        [Required]
        public int AuthorId { get; set; }

        public int? StartupId { get; set; }

        /// <summary>
        /// When set, creates an in-app repost of this blog post.
        /// Title/content/image/startup may be auto-filled from the original.
        /// </summary>
        public int? SharedFromBlogPostId { get; set; }

        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(4000)]
        public string Content { get; set; } = string.Empty;

        public byte[]? ImageData { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
