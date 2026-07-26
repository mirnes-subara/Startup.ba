using System.ComponentModel.DataAnnotations;

namespace Startupba.Model.Requests
{
    public class ReportResolveRequest
    {
        /// <summary>
        /// 1=Reviewed, 2=Dismissed, 3=ActionTaken
        /// </summary>
        [Required]
        [Range(1, 3)]
        public int Status { get; set; }

        [MaxLength(1000)]
        public string? AdminNote { get; set; }
    }
}
