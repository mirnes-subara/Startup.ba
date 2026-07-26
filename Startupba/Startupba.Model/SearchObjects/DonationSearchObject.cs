using System;

namespace Startupba.Model.SearchObjects
{
    public class DonationSearchObject : BaseSearchObject
    {
        public int? StartupId { get; set; }
        public int? UserId { get; set; }

        /// <summary>
        /// Only donations to startups founded by this user.
        /// </summary>
        public int? FounderId { get; set; }

        public string? Status { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
    }
}
