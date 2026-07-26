namespace Startupba.Model.SearchObjects
{
    public class ReportSearchObject : BaseSearchObject
    {
        public int? ReporterId { get; set; }

        /// <summary>
        /// 0=Startup, 1=BlogPost, 2=User
        /// </summary>
        public int? TargetType { get; set; }

        /// <summary>
        /// 0=Pending, 1=Reviewed, 2=Dismissed, 3=ActionTaken
        /// </summary>
        public int? Status { get; set; }

        public int? StartupId { get; set; }
        public int? BlogPostId { get; set; }
        public int? ReportedUserId { get; set; }
    }
}
