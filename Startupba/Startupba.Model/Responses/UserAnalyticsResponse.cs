using System;
using System.Collections.Generic;

namespace Startupba.Model.Responses
{
    /// <summary>
    /// Profile activity report for a single user (mobile app "profile analytics").
    /// </summary>
    public class UserAnalyticsResponse
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
        public DateTime MemberSince { get; set; }

        // As a founder
        public int StartupsCreated { get; set; }
        public int StartupsApproved { get; set; }
        public int StartupsCompleted { get; set; }
        public decimal TotalRaised { get; set; }
        public int DonationsReceived { get; set; }
        public int LikesReceived { get; set; }
        public int FavoritesReceived { get; set; }
        public List<UserStartupSummary> Startups { get; set; } = new List<UserStartupSummary>();

        // As an investor / community member
        public int DonationsMade { get; set; }
        public decimal TotalDonated { get; set; }
        public int StartupsLiked { get; set; }
        public int StartupsFavorited { get; set; }
        public int BlogPostsWritten { get; set; }
        public int CommentsWritten { get; set; }

        public List<MonthlyDonationData> MonthlyDonationsMade { get; set; } = new List<MonthlyDonationData>();
    }

    public class UserStartupSummary
    {
        public int StartupId { get; set; }
        public string StartupName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public string StatusName { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal AmountRaised { get; set; }
        public decimal FundingPercent { get; set; }
        public int LikeCount { get; set; }
        public int FavoriteCount { get; set; }
    }
}
