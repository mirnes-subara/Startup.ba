using System.ComponentModel.DataAnnotations;

namespace Startupba.Model.Requests
{
    public class CommentUpsertRequest
    {
        [Required]
        public int BlogPostId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Content { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
    }
}
