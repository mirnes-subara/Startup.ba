using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Startupba.Services.Database
{
    /// <summary>
    /// A user report of a rule/standards violation.
    /// The target can be a startup, a blog post or another user
    /// (exactly one of StartupId / BlogPostId / ReportedUserId is set,
    /// matching TargetType).
    /// </summary>
    public class Report
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ReporterId { get; set; } // FK to User who filed the report

        /// <summary>
        /// 0=Startup, 1=BlogPost, 2=User
        /// </summary>
        [Required]
        public int TargetType { get; set; }

        public int? StartupId { get; set; }

        public int? BlogPostId { get; set; }

        public int? ReportedUserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }

        /// <summary>
        /// 0=Pending, 1=Reviewed, 2=Dismissed, 3=ActionTaken
        /// </summary>
        [Required]
        public int Status { get; set; } = 0;

        [MaxLength(1000)]
        public string? AdminNote { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? ResolvedAt { get; set; }

        // Navigation properties
        [ForeignKey("ReporterId")]
        public User Reporter { get; set; } = null!;

        [ForeignKey("StartupId")]
        public Startup? Startup { get; set; }

        [ForeignKey("BlogPostId")]
        public BlogPost? BlogPost { get; set; }

        [ForeignKey("ReportedUserId")]
        public User? ReportedUser { get; set; }
    }
}
