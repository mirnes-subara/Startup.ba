using System;
using System.Linq;
using System.Threading.Tasks;
using Startupba.Model.Responses;
using Startupba.Services.Database;
using Startupba.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Startupba.Services.Services
{
    /// <summary>
    /// Profile activity report for a single user: their startups and raised
    /// amounts (as a founder) plus donations, likes, favorites and community
    /// activity (as an investor / community member).
    /// Aggregations run in SQL; only report rows are materialized.
    /// </summary>
    public class UserAnalyticsService : IUserAnalyticsService
    {
        private readonly StartupbaDbContext _context;

        public UserAnalyticsService(StartupbaDbContext context)
        {
            _context = context;
        }

        public async Task<UserAnalyticsResponse?> GetUserAnalyticsAsync(int userId)
        {
            var user = await _context.Users.AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    u.Id,
                    u.FirstName,
                    u.LastName,
                    u.IsVerified,
                    u.CreatedAt
                })
                .FirstOrDefaultAsync();
            if (user == null)
                return null;

            var response = new UserAnalyticsResponse
            {
                UserId = user.Id,
                UserName = $"{user.FirstName} {user.LastName}",
                IsVerified = user.IsVerified,
                MemberSince = user.CreatedAt
            };

            var founderStartups = _context.Startups.AsNoTracking()
                .Where(s => s.FounderId == userId);

            var founderStats = await founderStartups
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Created = g.Count(),
                    Approved = g.Count(s => s.StatusId == StartupStatuses.Approved),
                    Completed = g.Count(s => s.StatusId == StartupStatuses.Completed),
                    TotalRaised = g.Sum(s => s.AmountRaised)
                })
                .FirstOrDefaultAsync();

            response.StartupsCreated = founderStats?.Created ?? 0;
            response.StartupsApproved = founderStats?.Approved ?? 0;
            response.StartupsCompleted = founderStats?.Completed ?? 0;
            response.TotalRaised = founderStats?.TotalRaised ?? 0;
            response.DonationsReceived = await _context.Donations.CountAsync(d =>
                d.Status == "Completed" && d.Startup.FounderId == userId);
            response.LikesReceived = await _context.StartupLikes.CountAsync(l => l.Startup.FounderId == userId);
            response.FavoritesReceived = await _context.Favorites.CountAsync(f => f.Startup.FounderId == userId);

            var startupRows = await founderStartups
                .OrderByDescending(s => s.AmountRaised)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    CategoryName = s.Category != null ? s.Category.Name : "Unknown",
                    StatusName = s.Status != null ? s.Status.Name : "Unknown",
                    s.TargetAmount,
                    s.AmountRaised,
                    LikeCount = s.StartupLikes.Count,
                    FavoriteCount = s.Favorites.Count
                })
                .ToListAsync();

            response.Startups = startupRows.Select(s => new UserStartupSummary
            {
                StartupId = s.Id,
                StartupName = s.Name,
                CategoryName = s.CategoryName,
                StatusName = s.StatusName,
                TargetAmount = s.TargetAmount,
                AmountRaised = s.AmountRaised,
                FundingPercent = s.TargetAmount > 0
                    ? Math.Round(s.AmountRaised / s.TargetAmount * 100, 2)
                    : 0,
                LikeCount = s.LikeCount,
                FavoriteCount = s.FavoriteCount
            }).ToList();

            var donationsMade = _context.Donations.AsNoTracking()
                .Where(d => d.UserId == userId && d.Status == "Completed");

            var donationStats = await donationsMade
                .GroupBy(_ => 1)
                .Select(g => new { Count = g.Count(), Total = g.Sum(d => d.Amount) })
                .FirstOrDefaultAsync();

            response.DonationsMade = donationStats?.Count ?? 0;
            response.TotalDonated = donationStats?.Total ?? 0;
            response.StartupsLiked = await _context.StartupLikes.CountAsync(l => l.UserId == userId);
            response.StartupsFavorited = await _context.Favorites.CountAsync(f => f.UserId == userId);
            response.BlogPostsWritten = await _context.BlogPosts.CountAsync(bp => bp.AuthorId == userId);
            response.CommentsWritten = await _context.Comments.CountAsync(c => c.UserId == userId);

            var now = DateTime.UtcNow;
            var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var windowStart = currentMonthStart.AddMonths(-11);

            var monthRows = await donationsMade
                .Where(d => (d.CompletedAt ?? d.CreatedAt) >= windowStart)
                .GroupBy(d => new
                {
                    Year = (d.CompletedAt ?? d.CreatedAt).Year,
                    Month = (d.CompletedAt ?? d.CreatedAt).Month
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Amount = g.Sum(d => d.Amount),
                    DonationCount = g.Count()
                })
                .ToListAsync();

            response.MonthlyDonationsMade = Enumerable.Range(0, 12)
                .Select(i =>
                {
                    var start = currentMonthStart.AddMonths(i - 11);
                    var row = monthRows.FirstOrDefault(r => r.Year == start.Year && r.Month == start.Month);
                    return new MonthlyDonationData
                    {
                        Month = start.ToString("yyyy-MM"),
                        Amount = row?.Amount ?? 0,
                        DonationCount = row?.DonationCount ?? 0
                    };
                })
                .ToList();

            return response;
        }
    }
}
