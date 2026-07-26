using System;

namespace Startupba.Model.Responses
{
    public class ReportResponse
    {
        public int Id { get; set; }

        public int ReporterId { get; set; }
        public string ReporterName { get; set; } = string.Empty;

        /// <summary>
        /// 0=Startup, 1=BlogPost, 2=User
        /// </summary>
        public int TargetType { get; set; }
        public string TargetTypeName { get; set; } = string.Empty;

        public int? StartupId { get; set; }
        public string? StartupName { get; set; }

        public int? BlogPostId { get; set; }
        public string? BlogPostTitle { get; set; }

        public int? ReportedUserId { get; set; }
        public string? ReportedUserName { get; set; }

        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }

        /// <summary>
        /// 0=Pending, 1=Reviewed, 2=Dismissed, 3=ActionTaken
        /// </summary>
        public int Status { get; set; }
        public string StatusName { get; set; } = string.Empty;

        public string? AdminNote { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
    }
}
