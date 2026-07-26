namespace Startupba.Model.SearchObjects
{
    public class StartupSearchObject : BaseSearchObject
    {
        public string? Name { get; set; }
        public int? FounderId { get; set; }
        public int? CategoryId { get; set; }
        public int? CityId { get; set; }
        public int? StatusId { get; set; }
        public decimal? MinTargetAmount { get; set; }
        public decimal? MaxTargetAmount { get; set; }
        public bool? IsActive { get; set; }

        /// <summary>
        /// Only return startups favorited by this user.
        /// </summary>
        public int? FavoritedByUserId { get; set; }

        /// <summary>
        /// Only return startups liked by this user.
        /// </summary>
        public int? LikedByUserId { get; set; }
    }
}
