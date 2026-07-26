using System;

namespace Startupba.Model.Responses
{
    public class CommentResponse
    {
        public int Id { get; set; }

        public int BlogPostId { get; set; }
        public string BlogPostTitle { get; set; } = string.Empty;

        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
