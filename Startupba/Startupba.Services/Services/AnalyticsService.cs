using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Startupba.Model.Responses;
using Startupba.Services.Database;
using Startupba.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Startupba.Services.Services
{
    /// <summary>
    /// Platform-wide business report for the administrator (desktop app):
    /// donations, platform revenue, startups by category, user growth,
    /// community activity and support/moderation counters.
    /// Aggregations run in SQL; only report rows are materialized.
    /// </summary>
    public class AnalyticsService : IAnalyticsService
    {
        private readonly StartupbaDbContext _context;

        public AnalyticsService(StartupbaDbContext context)
        {
            _context = context;
        }

        public async Task<AnalyticsResponse> GetAnalyticsAsync()
        {
            var response = new AnalyticsResponse();
            var now = DateTime.UtcNow;
            var currentMonthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var nextMonthStart = currentMonthStart.AddMonths(1);
            var windowStart = currentMonthStart.AddMonths(-11);
            var months = Last12Months(currentMonthStart);

            var completedDonations = _context.Donations.AsNoTracking()
                .Where(d => d.Status == "Completed");

            var funding = await completedDonations
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    Total = g.Sum(d => d.Amount),
                    Count = g.Count(),
                    Average = g.Average(d => d.Amount)
                })
                .FirstOrDefaultAsync();

            response.TotalDonated = funding?.Total ?? 0;
            response.AverageDonation = funding == null
                ? 0
                : Math.Round(funding.Average, 2);

            response.MonthlyDonated = await completedDonations
                .Where(d => (d.CompletedAt ?? d.CreatedAt) >= currentMonthStart
                            && (d.CompletedAt ?? d.CreatedAt) < nextMonthStart)
                .SumAsync(d => d.Amount);

            // Fee applies only after the campaign has reached its funding target
            // (prijava / TermsOfUse: keep a percent of the collected amount once the target is met).
            response.PlatformRevenue = Math.Round(
                await _context.Startups.AsNoTracking()
                    .Where(s => s.TargetAmount > 0 && s.AmountRaised >= s.TargetAmount)
                    .SumAsync(s => s.AmountRaised * s.PlatformFeePercent / 100m),
                2);

            var donationTrendRows = await completedDonations
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

            response.MonthlyDonationTrend = months.Select(m =>
            {
                var row = donationTrendRows.FirstOrDefault(r => r.Year == m.Year && r.Month == m.Month);
                return new MonthlyDonationData
                {
                    Month = m.Label,
                    Amount = row?.Amount ?? 0,
                    DonationCount = row?.DonationCount ?? 0
                };
            }).ToList();

            response.DonationsByCategory = await completedDonations
                .GroupBy(d => d.Startup.Category != null ? d.Startup.Category.Name : "Unknown")
                .Select(g => new DonationsByCategory
                {
                    CategoryName = g.Key,
                    Amount = g.Sum(d => d.Amount),
                    DonationCount = g.Count()
                })
                .OrderByDescending(x => x.Amount)
                .ToListAsync();

            var startups = _context.Startups.AsNoTracking();

            response.TotalStartups = await startups.CountAsync();
            response.PendingStartups = await startups.CountAsync(s => s.StatusId == StartupStatuses.Pending);
            response.ApprovedStartups = await startups.CountAsync(s => s.StatusId == StartupStatuses.Approved);
            response.RejectedStartups = await startups.CountAsync(s => s.StatusId == StartupStatuses.Rejected);
            response.PausedStartups = await startups.CountAsync(s => s.StatusId == StartupStatuses.Paused);
            response.CompletedStartups = await startups.CountAsync(s => s.StatusId == StartupStatuses.Completed);

            response.StartupsByCategory = await startups
                .GroupBy(s => s.Category != null ? s.Category.Name : "Unknown")
                .Select(g => new StartupCountByCategory
                {
                    CategoryName = g.Key,
                    Count = g.Count(),
                    ApprovedCount = g.Count(s => s.StatusId == StartupStatuses.Approved)
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            response.StartupsByCity = await startups
                .GroupBy(s => s.City != null ? s.City.Name : "Unknown")
                .Select(g => new StartupCountByCity
                {
                    CityName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();

            var topRows = await startups
                .OrderByDescending(s => s.AmountRaised)
                .Take(10)
                .Select(s => new
                {
                    s.Id,
                    s.Name,
                    CategoryName = s.Category != null ? s.Category.Name : "Unknown",
                    s.TargetAmount,
                    s.AmountRaised
                })
                .ToListAsync();

            response.TopStartupsByFunding = topRows.Select(s => new TopStartupData
            {
                StartupId = s.Id,
                StartupName = s.Name,
                CategoryName = s.CategoryName,
                TargetAmount = s.TargetAmount,
                AmountRaised = s.AmountRaised,
                FundingPercent = s.TargetAmount > 0
                    ? Math.Round(s.AmountRaised / s.TargetAmount * 100, 2)
                    : 0
            }).ToList();

            var startupMonthRows = await startups
                .Where(s => s.CreatedAt >= windowStart)
                .GroupBy(s => new { s.CreatedAt.Year, s.CreatedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToListAsync();
            var startupsBeforeWindow = await startups.CountAsync(s => s.CreatedAt < windowStart);

            var runningStartups = startupsBeforeWindow;
            response.MonthlyStartupGrowth = months.Select(m =>
            {
                var born = startupMonthRows.FirstOrDefault(r => r.Year == m.Year && r.Month == m.Month)?.Count ?? 0;
                runningStartups += born;
                return new MonthlyStartupGrowth
                {
                    Month = m.Label,
                    NewStartups = born,
                    TotalStartups = runningStartups
                };
            }).ToList();

            var users = _context.Users.AsNoTracking();
            response.TotalUsers = await users.CountAsync();
            response.ActiveUsers = await users.CountAsync(u => u.IsActive);
            response.VerifiedUsers = await users.CountAsync(u => u.IsVerified);
            response.TotalAdmins = await users.CountAsync(u =>
                u.UserRoles.Any(ur => ur.Role != null && ur.Role.Name == "Administrator"));

            var userMonthRows = await users
                .Where(u => u.CreatedAt >= windowStart)
                .GroupBy(u => new { u.CreatedAt.Year, u.CreatedAt.Month })
                .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
                .ToListAsync();
            var usersBeforeWindow = await users.CountAsync(u => u.CreatedAt < windowStart);

            var runningUsers = usersBeforeWindow;
            response.MonthlyUserGrowth = months.Select(m =>
            {
                var born = userMonthRows.FirstOrDefault(r => r.Year == m.Year && r.Month == m.Month)?.Count ?? 0;
                runningUsers += born;
                return new MonthlyUserGrowth
                {
                    Month = m.Label,
                    NewUsers = born,
                    TotalUsers = runningUsers
                };
            }).ToList();

            response.TotalBlogPosts = await _context.BlogPosts.CountAsync();
            response.TotalComments = await _context.Comments.CountAsync();
            response.TotalStartupLikes = await _context.StartupLikes.CountAsync();
            response.TotalFavorites = await _context.Favorites.CountAsync();

            response.OpenSupportTickets = await _context.SupportTickets.CountAsync(st => st.Status == 0);
            response.AnsweredSupportTickets = await _context.SupportTickets.CountAsync(st => st.Status == 1);
            response.ClosedSupportTickets = await _context.SupportTickets.CountAsync(st => st.Status == 2);
            response.PendingReports = await _context.Reports.CountAsync(r => r.Status == 0);
            response.ResolvedReports = await _context.Reports.CountAsync(r => r.Status != 0);

            return response;
        }

        private static List<(int Year, int Month, string Label)> Last12Months(DateTime currentMonthStart)
        {
            return Enumerable.Range(0, 12)
                .Select(i =>
                {
                    var start = currentMonthStart.AddMonths(i - 11);
                    return (start.Year, start.Month, start.ToString("yyyy-MM"));
                })
                .ToList();
        }
    }
}
