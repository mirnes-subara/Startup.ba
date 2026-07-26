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

            var startups = await _context.Startups
                .Include(s => s.Category)
                .Include(s => s.City)
                .ToListAsync();

            var donations = await _context.Donations
                .Include(d => d.Startup)
                    .ThenInclude(s => s.Category)
                .ToListAsync();

            var users = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .ToListAsync();

            var completedDonations = donations.Where(d => d.Status == "Completed").ToList();

            // Funding metrics
            response.TotalDonated = completedDonations.Sum(d => d.Amount);

            var currentMonth = DateTime.UtcNow.Month;
            var currentYear = DateTime.UtcNow.Year;
            response.MonthlyDonated = completedDonations
                .Where(d => d.CreatedAt.Month == currentMonth && d.CreatedAt.Year == currentYear)
                .Sum(d => d.Amount);

            response.AverageDonation = completedDonations.Any()
                ? Math.Round((decimal)completedDonations.Average(d => (double)d.Amount), 2)
                : 0;

            // Platform revenue = completed donations x the fee snapshot of their startup
            response.PlatformRevenue = Math.Round(completedDonations
                .Sum(d => d.Amount * (d.Startup?.PlatformFeePercent ?? 0) / 100m), 2);

            // Monthly donation trend (last 12 months)
            var last12Months = Enumerable.Range(0, 12)
                .Select(i => DateTime.UtcNow.AddMonths(-i))
                .Reverse()
                .ToList();

            response.MonthlyDonationTrend = last12Months.Select(month => new MonthlyDonationData
            {
                Month = month.ToString("yyyy-MM"),
                Amount = completedDonations
                    .Where(d => d.CreatedAt.Year == month.Year && d.CreatedAt.Month == month.Month)
                    .Sum(d => d.Amount),
                DonationCount = completedDonations
                    .Count(d => d.CreatedAt.Year == month.Year && d.CreatedAt.Month == month.Month)
            }).ToList();

            // Donations by category
            response.DonationsByCategory = completedDonations
                .GroupBy(d => d.Startup?.Category?.Name ?? "Unknown")
                .Select(g => new DonationsByCategory
                {
                    CategoryName = g.Key,
                    Amount = g.Sum(d => d.Amount),
                    DonationCount = g.Count()
                })
                .OrderByDescending(x => x.Amount)
                .ToList();

            // Startup metrics
            response.TotalStartups = startups.Count;
            response.PendingStartups = startups.Count(s => s.StatusId == StartupStatuses.Pending);
            response.ApprovedStartups = startups.Count(s => s.StatusId == StartupStatuses.Approved);
            response.RejectedStartups = startups.Count(s => s.StatusId == StartupStatuses.Rejected);
            response.PausedStartups = startups.Count(s => s.StatusId == StartupStatuses.Paused);
            response.CompletedStartups = startups.Count(s => s.StatusId == StartupStatuses.Completed);

            response.StartupsByCategory = startups
                .GroupBy(s => s.Category?.Name ?? "Unknown")
                .Select(g => new StartupCountByCategory
                {
                    CategoryName = g.Key,
                    Count = g.Count(),
                    ApprovedCount = g.Count(s => s.StatusId == StartupStatuses.Approved)
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            response.StartupsByCity = startups
                .GroupBy(s => s.City?.Name ?? "Unknown")
                .Select(g => new StartupCountByCity
                {
                    CityName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            response.TopStartupsByFunding = startups
                .OrderByDescending(s => s.AmountRaised)
                .Take(10)
                .Select(s => new TopStartupData
                {
                    StartupId = s.Id,
                    StartupName = s.Name,
                    CategoryName = s.Category?.Name ?? "Unknown",
                    TargetAmount = s.TargetAmount,
                    AmountRaised = s.AmountRaised,
                    FundingPercent = s.TargetAmount > 0
                        ? Math.Round(s.AmountRaised / s.TargetAmount * 100, 2)
                        : 0
                })
                .ToList();

            response.MonthlyStartupGrowth = last12Months.Select(month => new MonthlyStartupGrowth
            {
                Month = month.ToString("yyyy-MM"),
                NewStartups = startups.Count(s => s.CreatedAt.Year == month.Year && s.CreatedAt.Month == month.Month),
                TotalStartups = startups.Count(s => s.CreatedAt <= month.AddMonths(1).AddDays(-1))
            }).ToList();

            // User metrics
            response.TotalUsers = users.Count;
            response.ActiveUsers = users.Count(u => u.IsActive);
            response.VerifiedUsers = users.Count(u => u.IsVerified);
            response.TotalAdmins = users.Count(u => u.UserRoles.Any(ur => ur.Role?.Name == "Administrator"));

            response.MonthlyUserGrowth = last12Months.Select(month => new MonthlyUserGrowth
            {
                Month = month.ToString("yyyy-MM"),
                NewUsers = users.Count(u => u.CreatedAt.Year == month.Year && u.CreatedAt.Month == month.Month),
                TotalUsers = users.Count(u => u.CreatedAt <= month.AddMonths(1).AddDays(-1))
            }).ToList();

            // Community metrics
            response.TotalBlogPosts = await _context.BlogPosts.CountAsync();
            response.TotalComments = await _context.Comments.CountAsync();
            response.TotalStartupLikes = await _context.StartupLikes.CountAsync();
            response.TotalFavorites = await _context.Favorites.CountAsync();

            // Support & moderation metrics
            response.OpenSupportTickets = await _context.SupportTickets.CountAsync(st => st.Status == 0);
            response.AnsweredSupportTickets = await _context.SupportTickets.CountAsync(st => st.Status == 1);
            response.ClosedSupportTickets = await _context.SupportTickets.CountAsync(st => st.Status == 2);
            response.PendingReports = await _context.Reports.CountAsync(r => r.Status == 0);
            response.ResolvedReports = await _context.Reports.CountAsync(r => r.Status != 0);

            return response;
        }
    }
}
