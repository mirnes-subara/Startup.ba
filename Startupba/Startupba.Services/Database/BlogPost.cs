using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Startupba.Services.Database
{
    /// <summary>
    /// A community blog post. Can optionally be linked to a startup
    /// (used when a user shares a startup with the community).
    /// </summary>
    public class BlogPost
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AuthorId { get; set; } // FK to User

        /// <summary>
        /// Optional link to a startup this post is about / shares.
        /// </summary>
        public int? StartupId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(4000)]
        public string Content { get; set; } = string.Empty;

        public byte[]? ImageData { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        [ForeignKey("AuthorId")]
        public User Author { get; set; } = null!;

        [ForeignKey("StartupId")]
        public Startup? Startup { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<BlogPostLike> BlogPostLikes { get; set; } = new List<BlogPostLike>();
    }
}
