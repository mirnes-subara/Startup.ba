using System;

namespace Startupba.Model.Responses
{
    public class BlogPostResponse
    {
        public int Id { get; set; }

        public int AuthorId { get; set; }
        public string AuthorName { get; set; } = string.Empty;

        public int? StartupId { get; set; }
        public string? StartupName { get; set; }

        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public byte[]? ImageData { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Engagement metrics
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public bool IsLiked { get; set; }
    }
}
