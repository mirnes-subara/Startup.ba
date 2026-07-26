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
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
                return null;

            var response = new UserAnalyticsResponse
            {
                UserId = user.Id,
                UserName = $"{user.FirstName} {user.LastName}",
                IsVerified = user.IsVerified,
                MemberSince = user.CreatedAt
            };

            // As a founder
            var startups = await _context.Startups
                .Include(s => s.Category)
                .Include(s => s.Status)
                .Include(s => s.StartupLikes)
                .Include(s => s.Favorites)
                .Include(s => s.Donations)
                .Where(s => s.FounderId == userId)
                .ToListAsync();

            response.StartupsCreated = startups.Count;
            response.StartupsApproved = startups.Count(s => s.StatusId == StartupStatuses.Approved);
            response.StartupsCompleted = startups.Count(s => s.StatusId == StartupStatuses.Completed);
            response.TotalRaised = startups.Sum(s => s.AmountRaised);
            response.DonationsReceived = startups.Sum(s => s.Donations.Count(d => d.Status == "Completed"));
            response.LikesReceived = startups.Sum(s => s.StartupLikes.Count);
            response.FavoritesReceived = startups.Sum(s => s.Favorites.Count);

            response.Startups = startups
                .OrderByDescending(s => s.AmountRaised)
                .Select(s => new UserStartupSummary
                {
                    StartupId = s.Id,
                    StartupName = s.Name,
                    CategoryName = s.Category?.Name ?? "Unknown",
                    StatusName = s.Status?.Name ?? "Unknown",
                    TargetAmount = s.TargetAmount,
                    AmountRaised = s.AmountRaised,
                    FundingPercent = s.TargetAmount > 0
                        ? Math.Round(s.AmountRaised / s.TargetAmount * 100, 2)
                        : 0,
                    LikeCount = s.StartupLikes.Count,
                    FavoriteCount = s.Favorites.Count
                })
                .ToList();

            // As an investor / community member
            var donationsMade = await _context.Donations
                .Where(d => d.UserId == userId && d.Status == "Completed")
                .ToListAsync();

            response.DonationsMade = donationsMade.Count;
            response.TotalDonated = donationsMade.Sum(d => d.Amount);
            response.StartupsLiked = await _context.StartupLikes.CountAsync(l => l.UserId == userId);
            response.StartupsFavorited = await _context.Favorites.CountAsync(f => f.UserId == userId);
            response.BlogPostsWritten = await _context.BlogPosts.CountAsync(bp => bp.AuthorId == userId);
            response.CommentsWritten = await _context.Comments.CountAsync(c => c.UserId == userId);

            // Monthly donation activity (last 12 months)
            var last12Months = Enumerable.Range(0, 12)
                .Select(i => DateTime.UtcNow.AddMonths(-i))
                .Reverse()
                .ToList();

            response.MonthlyDonationsMade = last12Months.Select(month => new MonthlyDonationData
            {
                Month = month.ToString("yyyy-MM"),
                Amount = donationsMade
                    .Where(d => d.CreatedAt.Year == month.Year && d.CreatedAt.Month == month.Month)
                    .Sum(d => d.Amount),
                DonationCount = donationsMade
                    .Count(d => d.CreatedAt.Year == month.Year && d.CreatedAt.Month == month.Month)
            }).ToList();

            return response;
        }
    }
}
