using System;

namespace Startupba.Model.Responses
{
    public class StartupResponse
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public int FounderId { get; set; }
        public string FounderName { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public int CityId { get; set; }
        public string CityName { get; set; } = string.Empty;

        public decimal TargetAmount { get; set; }
        public decimal AmountRaised { get; set; }

        /// <summary>
        /// Funding progress as a percentage of the target (0-100+).
        /// </summary>
        public decimal FundingPercent { get; set; }

        public decimal PlatformFeePercent { get; set; }

        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;

        public string? RejectionReason { get; set; }

        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Engagement metrics
        public int LikeCount { get; set; }
        public int FavoriteCount { get; set; }
        public int DonationCount { get; set; }

        /// <summary>
        /// Whether the authenticated user has liked this startup.
        /// </summary>
        public bool IsLiked { get; set; }

        /// <summary>
        /// Whether the authenticated user has favorited this startup.
        /// </summary>
        public bool IsFavorited { get; set; }

        // Cover image (first cover, if any)
        public byte[]? CoverImage { get; set; }

        // Logo image (first logo, if any)
        public byte[]? LogoImage { get; set; }
    }
}
