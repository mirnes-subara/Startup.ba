using System.ComponentModel.DataAnnotations;

namespace Startupba.Model.Requests
{
    public class ReportUpsertRequest
    {
        [Required]
        public int ReporterId { get; set; }

        /// <summary>
        /// 0=Startup, 1=BlogPost, 2=User
        /// </summary>
        [Required]
        [Range(0, 2)]
        public int TargetType { get; set; }

        public int? StartupId { get; set; }

        public int? BlogPostId { get; set; }

        public int? ReportedUserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Reason { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string? Description { get; set; }
    }
}
