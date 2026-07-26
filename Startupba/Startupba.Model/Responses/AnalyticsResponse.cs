using System;
using System.Collections.Generic;

namespace Startupba.Model.Responses
{
    /// <summary>
    /// Platform-wide business report for the administrator (desktop app).
    /// </summary>
    public class AnalyticsResponse
    {
        // Funding metrics
        public decimal TotalDonated { get; set; }
        public decimal MonthlyDonated { get; set; }
        public decimal AverageDonation { get; set; }

        /// <summary>
        /// Platform revenue = sum of (completed donations x startup fee percent).
        /// </summary>
        public decimal PlatformRevenue { get; set; }

        public List<MonthlyDonationData> MonthlyDonationTrend { get; set; } = new List<MonthlyDonationData>();
        public List<DonationsByCategory> DonationsByCategory { get; set; } = new List<DonationsByCategory>();

        // Startup metrics
        public int TotalStartups { get; set; }
        public int PendingStartups { get; set; }
        public int ApprovedStartups { get; set; }
        public int RejectedStartups { get; set; }
        public int PausedStartups { get; set; }
        public int CompletedStartups { get; set; }
        public List<StartupCountByCategory> StartupsByCategory { get; set; } = new List<StartupCountByCategory>();
        public List<StartupCountByCity> StartupsByCity { get; set; } = new List<StartupCountByCity>();
        public List<TopStartupData> TopStartupsByFunding { get; set; } = new List<TopStartupData>();
        public List<MonthlyStartupGrowth> MonthlyStartupGrowth { get; set; } = new List<MonthlyStartupGrowth>();

        // User metrics
        public int TotalUsers { get; set; }
        public int ActiveUsers { get; set; }
        public int VerifiedUsers { get; set; }
        public int TotalAdmins { get; set; }
        public List<MonthlyUserGrowth> MonthlyUserGrowth { get; set; } = new List<MonthlyUserGrowth>();

        // Community metrics
        public int TotalBlogPosts { get; set; }
        public int TotalComments { get; set; }
        public int TotalStartupLikes { get; set; }
        public int TotalFavorites { get; set; }

        // Support & moderation metrics
        public int OpenSupportTickets { get; set; }
        public int AnsweredSupportTickets { get; set; }
        public int ClosedSupportTickets { get; set; }
        public int PendingReports { get; set; }
        public int ResolvedReports { get; set; }
    }

    public class MonthlyDonationData
    {
        public string Month { get; set; } = string.Empty; // Format: "YYYY-MM"
        public decimal Amount { get; set; }
        public int DonationCount { get; set; }
    }

    public class DonationsByCategory
    {
        public string CategoryName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public int DonationCount { get; set; }
    }

    public class StartupCountByCategory
    {
        public string CategoryName { get; set; } = string.Empty;
        public int Count { get; set; }
        public int ApprovedCount { get; set; }
    }

    public class StartupCountByCity
    {
        public string CityName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class TopStartupData
    {
        public int StartupId { get; set; }
        public string StartupName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public decimal TargetAmount { get; set; }
        public decimal AmountRaised { get; set; }
        public decimal FundingPercent { get; set; }
    }

    public class MonthlyStartupGrowth
    {
        public string Month { get; set; } = string.Empty;
        public int NewStartups { get; set; }
        public int TotalStartups { get; set; }
    }

    public class MonthlyUserGrowth
    {
        public string Month { get; set; } = string.Empty;
        public int NewUsers { get; set; }
        public int TotalUsers { get; set; }
    }
}
