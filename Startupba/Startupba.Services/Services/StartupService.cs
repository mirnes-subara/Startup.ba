using Startupba.Model;
using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Database;
using Startupba.Services.Helpers;
using Startupba.Services.Interfaces;
using Startupba.Subscriber.Models;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Startupba.Services.Services
{
    public class StartupService : BaseCRUDService<StartupResponse, StartupSearchObject, Startup, StartupUpsertRequest, StartupUpsertRequest>, IStartupService
    {
        private readonly INotificationService _notificationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<StartupService> _logger;
        private readonly IRabbitMqPublisher _rabbitMqPublisher;

        public StartupService(
            StartupbaDbContext context,
            IMapper mapper,
            INotificationService notificationService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<StartupService> logger,
            IRabbitMqPublisher rabbitMqPublisher) : base(context, mapper)
        {
            _notificationService = notificationService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _rabbitMqPublisher = rabbitMqPublisher;
        }

        #region Query

        private IQueryable<Startup> BaseQuery => _context.Startups
            .Include(s => s.Founder)
            .Include(s => s.Category)
            .Include(s => s.City)
            .Include(s => s.Status)
            .Include(s => s.StartupImages)
            .Include(s => s.StartupLikes)
            .Include(s => s.Favorites)
            .Include(s => s.Donations);

        public override async Task<PagedResult<StartupResponse>> GetAsync(StartupSearchObject search)
        {
            var query = BaseQuery.AsQueryable();
            query = ApplyFilter(query, search);

            int? totalCount = null;
            if (search.IncludeTotalCount)
            {
                totalCount = await query.CountAsync();
            }

            query = ApplyPaging(query, search);

            var list = await query.ToListAsync();
            return new PagedResult<StartupResponse>
            {
                Items = list.Select(MapToResponse).ToList(),
                TotalCount = totalCount
            };
        }

        protected override IQueryable<Startup> ApplyFilter(IQueryable<Startup> query, StartupSearchObject search)
        {
            if (!string.IsNullOrEmpty(search.Name))
            {
                query = query.Where(s => s.Name.Contains(search.Name));
            }

            if (!string.IsNullOrEmpty(search.FTS))
            {
                query = query.Where(s =>
                    s.Name.Contains(search.FTS) ||
                    s.Description.Contains(search.FTS) ||
                    s.Category.Name.Contains(search.FTS));
            }

            if (search.FounderId.HasValue)
            {
                query = query.Where(s => s.FounderId == search.FounderId.Value);
            }

            if (search.CategoryId.HasValue)
            {
                query = query.Where(s => s.CategoryId == search.CategoryId.Value);
            }

            if (search.CityId.HasValue)
            {
                query = query.Where(s => s.CityId == search.CityId.Value);
            }

            if (search.StatusId.HasValue)
            {
                query = query.Where(s => s.StatusId == search.StatusId.Value);
            }
            else if (!search.FounderId.HasValue)
            {
                // Public browse (Home/Explore): hide non-discoverable statuses.
                // Admins listing without a status filter still see everything.
                var isAdmin = _httpContextAccessor.HttpContext?.User
                    ?.IsInRole("Administrator") == true;
                if (!isAdmin)
                {
                    query = query.Where(s =>
                        s.StatusId != StartupStatuses.Pending
                        && s.StatusId != StartupStatuses.Rejected
                        && s.StatusId != StartupStatuses.Paused
                        && s.StatusId != StartupStatuses.Draft);
                }
            }

            if (search.MinTargetAmount.HasValue)
            {
                query = query.Where(s => s.TargetAmount >= search.MinTargetAmount.Value);
            }

            if (search.MaxTargetAmount.HasValue)
            {
                query = query.Where(s => s.TargetAmount <= search.MaxTargetAmount.Value);
            }

            if (search.IsActive.HasValue)
            {
                query = query.Where(s => s.IsActive == search.IsActive.Value);
            }

            if (search.FavoritedByUserId.HasValue)
            {
                query = query.Where(s => s.Favorites.Any(f => f.UserId == search.FavoritedByUserId.Value));
            }

            if (search.LikedByUserId.HasValue)
            {
                query = query.Where(s => s.StartupLikes.Any(l => l.UserId == search.LikedByUserId.Value));
            }

            // Newest first
            return query.OrderByDescending(s => s.Id);
        }

        public override async Task<StartupResponse?> GetByIdAsync(int id)
        {
            var entity = await BaseQuery.FirstOrDefaultAsync(s => s.Id == id);
            if (entity == null)
                return null;

            return MapToResponse(entity);
        }

        protected override StartupResponse MapToResponse(Startup entity)
        {
            var response = _mapper.Map<StartupResponse>(entity);

            if (entity.Founder != null)
            {
                response.FounderName = $"{entity.Founder.FirstName} {entity.Founder.LastName}";
            }

            response.CategoryName = entity.Category?.Name ?? string.Empty;
            response.CityName = entity.City?.Name ?? string.Empty;
            response.StatusName = entity.Status?.Name ?? string.Empty;

            response.FundingPercent = entity.TargetAmount > 0
                ? Math.Round(entity.AmountRaised / entity.TargetAmount * 100, 2)
                : 0;

            response.LikeCount = entity.StartupLikes?.Count ?? 0;
            response.FavoriteCount = entity.Favorites?.Count ?? 0;
            response.DonationCount = entity.Donations?.Count(d => d.Status == "Completed") ?? 0;

            var userIdClaim = _httpContextAccessor.HttpContext?.User
                ?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                response.IsLiked = entity.StartupLikes?.Any(l => l.UserId == userId) ?? false;
                response.IsFavorited = entity.Favorites?.Any(f => f.UserId == userId) ?? false;
            }

            response.CoverImage = entity.StartupImages?
                .Where(i => i.IsActive && !i.IsLogo)
                .OrderByDescending(i => i.IsCover)
                .ThenBy(i => i.DisplayOrder)
                .FirstOrDefault()?.ImageData;

            response.LogoImage = entity.StartupImages?
                .Where(i => i.IsActive && i.IsLogo)
                .FirstOrDefault()?.ImageData;

            return response;
        }

        #endregion

        #region Create / Update

        protected override async Task BeforeInsert(Startup entity, StartupUpsertRequest request)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == request.FounderId))
            {
                throw new InvalidOperationException("Founder does not exist.");
            }

            if (!await _context.Categories.AnyAsync(c => c.Id == request.CategoryId && c.IsActive))
            {
                throw new InvalidOperationException("Category does not exist or is not active.");
            }

            if (!await _context.Cities.AnyAsync(c => c.Id == request.CityId))
            {
                throw new InvalidOperationException("City does not exist.");
            }
        }

        protected override Startup MapInsertToEntity(Startup entity, StartupUpsertRequest request)
        {
            base.MapInsertToEntity(entity, request);

            // Every new startup awaits admin approval
            entity.StatusId = StartupStatuses.Pending;
            entity.AmountRaised = 0;
            entity.CreatedAt = DateTime.UtcNow;

            // Snapshot the current platform fee so the founder knows the terms up front
            var feeSetting = _context.PlatformSettings
                .FirstOrDefault(ps => ps.Key == PlatformSettingKeys.PlatformFeePercent);
            if (feeSetting != null && decimal.TryParse(feeSetting.Value, out var fee))
            {
                entity.PlatformFeePercent = fee;
            }

            return entity;
        }

        public override async Task<StartupResponse> CreateAsync(StartupUpsertRequest request)
        {
            var result = await base.CreateAsync(request);

            await NotifyAdminsStartupSubmittedAsync(result.Id, result.Name);

            return result;
        }

        protected override async Task BeforeUpdate(Startup entity, StartupUpsertRequest request)
        {
            if (entity.StatusId == StartupStatuses.Completed)
            {
                throw new InvalidOperationException("Completed startups cannot be edited.");
            }

            if (!await _context.Users.AnyAsync(u => u.Id == request.FounderId))
            {
                throw new InvalidOperationException("Founder does not exist.");
            }

            if (!await _context.Categories.AnyAsync(c => c.Id == request.CategoryId && c.IsActive))
            {
                throw new InvalidOperationException("Category does not exist or is not active.");
            }

            if (!await _context.Cities.AnyAsync(c => c.Id == request.CityId))
            {
                throw new InvalidOperationException("City does not exist.");
            }
        }

        protected override void MapUpdateToEntity(Startup entity, StartupUpsertRequest request)
        {
            base.MapUpdateToEntity(entity, request);
            entity.UpdatedAt = DateTime.UtcNow;

            // Resubmit rejected startups for admin review after founder edits
            if (entity.StatusId == StartupStatuses.Rejected)
            {
                entity.StatusId = StartupStatuses.Pending;
                entity.RejectionReason = null;
            }
        }

        public override async Task<StartupResponse?> UpdateAsync(int id, StartupUpsertRequest request)
        {
            var previousStatusId = await _context.Startups
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => (int?)s.StatusId)
                .FirstOrDefaultAsync();

            var result = await base.UpdateAsync(id, request);
            if (result == null)
                return null;

            if (previousStatusId == StartupStatuses.Rejected)
            {
                await NotifyAdminsStartupSubmittedAsync(result.Id, result.Name);
            }

            // Reload with navigations so StatusName / CategoryName / etc. are populated
            return await GetByIdAsync(id);
        }

        #endregion

        #region Admin moderation actions

        public async Task<StartupResponse?> ApproveAsync(int id)
        {
            if (!await _context.Startups.AnyAsync(s => s.Id == id))
                return null;

            await EnsureNotDeletedAsync(id);

            var now = DateTime.UtcNow;
            decimal? fee = null;
            var feeSetting = await _context.PlatformSettings
                .FirstOrDefaultAsync(ps => ps.Key == PlatformSettingKeys.PlatformFeePercent);
            if (feeSetting != null && decimal.TryParse(feeSetting.Value, out var parsedFee))
            {
                fee = parsedFee;
            }

            int rows;
            if (fee.HasValue)
            {
                var feeValue = fee.Value;
                rows = await _context.Startups
                    .Where(s => s.Id == id && s.StatusId == StartupStatuses.Pending)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.StatusId, StartupStatuses.Approved)
                        .SetProperty(x => x.ApprovedAt, now)
                        .SetProperty(x => x.UpdatedAt, now)
                        .SetProperty(x => x.PlatformFeePercent, feeValue));
            }
            else
            {
                rows = await _context.Startups
                    .Where(s => s.Id == id && s.StatusId == StartupStatuses.Pending)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(x => x.StatusId, StartupStatuses.Approved)
                        .SetProperty(x => x.ApprovedAt, now)
                        .SetProperty(x => x.UpdatedAt, now));
            }

            if (rows == 0)
            {
                throw new InvalidOperationException("Only startups in Pending status can be approved.");
            }

            var entity = await BaseQuery.FirstAsync(s => s.Id == id);

            await NotifyFounderAsync(entity, "Startup Approved",
                $"Congratulations! Your startup \"{entity.Name}\" has been approved and is now visible to investors.",
                NotificationTypes.StartupApproved);

            await SendStartupEmailAsync(entity, "StartupApproved");

            return MapToResponse(entity);
        }

        public async Task<StartupResponse?> RejectAsync(int id, StartupRejectRequest request)
        {
            if (!await _context.Startups.AnyAsync(s => s.Id == id))
                return null;

            await EnsureNotDeletedAsync(id);

            var now = DateTime.UtcNow;
            var rows = await _context.Startups
                .Where(s => s.Id == id && s.StatusId == StartupStatuses.Pending)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.StatusId, StartupStatuses.Rejected)
                    .SetProperty(x => x.RejectionReason, request.Reason)
                    .SetProperty(x => x.UpdatedAt, now));

            if (rows == 0)
            {
                throw new InvalidOperationException("Only startups in Pending status can be rejected.");
            }

            var entity = await BaseQuery.FirstAsync(s => s.Id == id);

            await NotifyFounderAsync(entity, "Startup Rejected",
                $"Your startup \"{entity.Name}\" has been rejected. Reason: {request.Reason}",
                NotificationTypes.StartupRejected);

            await SendStartupEmailAsync(entity, "StartupRejected");

            return MapToResponse(entity);
        }

        public async Task<StartupResponse?> PauseAsync(int id)
        {
            if (!await _context.Startups.AnyAsync(s => s.Id == id))
                return null;

            await EnsureNotDeletedAsync(id);

            var now = DateTime.UtcNow;
            var rows = await _context.Startups
                .Where(s => s.Id == id && s.StatusId == StartupStatuses.Approved)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.StatusId, StartupStatuses.Paused)
                    .SetProperty(x => x.UpdatedAt, now));

            if (rows == 0)
            {
                throw new InvalidOperationException("Only startups in Approved status can be paused.");
            }

            var entity = await BaseQuery.FirstAsync(s => s.Id == id);

            await NotifyFounderAsync(entity, "Startup Paused",
                $"Your startup \"{entity.Name}\" has been paused by the administrator.",
                NotificationTypes.StartupPaused);

            return MapToResponse(entity);
        }

        public async Task<StartupResponse?> ResumeAsync(int id)
        {
            if (!await _context.Startups.AnyAsync(s => s.Id == id))
                return null;

            await EnsureNotDeletedAsync(id);

            var now = DateTime.UtcNow;
            var rows = await _context.Startups
                .Where(s => s.Id == id && s.StatusId == StartupStatuses.Paused)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.StatusId, StartupStatuses.Approved)
                    .SetProperty(x => x.UpdatedAt, now));

            if (rows == 0)
            {
                throw new InvalidOperationException("Only startups in Paused status can be resumed.");
            }

            var entity = await BaseQuery.FirstAsync(s => s.Id == id);

            await NotifyFounderAsync(entity, "Startup Resumed",
                $"Your startup \"{entity.Name}\" is active again and visible to investors.",
                NotificationTypes.StartupApproved);

            return MapToResponse(entity);
        }

        private async Task NotifyAdminsStartupSubmittedAsync(int startupId, string startupName)
        {
            try
            {
                var adminIds = await _context.UserRoles
                    .Where(ur => ur.Role.Name == "Administrator")
                    .Select(ur => ur.UserId)
                    .Distinct()
                    .ToListAsync();

                foreach (var adminId in adminIds)
                {
                    await _notificationService.CreateNotificationAsync(
                        adminId,
                        "New Startup Submitted",
                        $"\"{startupName}\" has been submitted and is awaiting your review.",
                        NotificationTypes.StartupSubmitted,
                        startupId,
                        "Startup");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify admins about startup submission");
            }
        }

        private async Task NotifyFounderAsync(Startup entity, string title, string message, int type)
        {
            try
            {
                await _notificationService.CreateNotificationAsync(
                    entity.FounderId, title, message, type, entity.Id, "Startup");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify founder about startup status change");
            }
        }

        private async Task SendStartupEmailAsync(Startup entity, string notificationType)
        {
            var founder = entity.Founder ?? await _context.Users.FindAsync(entity.FounderId);
            if (founder == null)
                return;

            var city = entity.City ?? await _context.Cities.Include(c => c.Country).FirstOrDefaultAsync(c => c.Id == entity.CityId);

            await _rabbitMqPublisher.PublishEmailAsync(new EmailNotificationDto
            {
                NotificationType = notificationType,
                RecipientEmail = founder.Email,
                RecipientFullName = $"{founder.FirstName} {founder.LastName}".Trim(),
                StartupId = entity.Id,
                StartupName = entity.Name,
                CategoryName = entity.Category?.Name ?? string.Empty,
                CityName = city?.Name ?? string.Empty,
                CountryName = city?.Country?.Name ?? string.Empty,
                TargetAmount = entity.TargetAmount,
                AmountRaised = entity.AmountRaised,
                PlatformFeePercent = entity.PlatformFeePercent,
                RejectionReason = entity.RejectionReason
            });
        }

        #endregion

        #region Likes / favorites

        public async Task<bool> LikeAsync(int startupId, int userId)
        {
            await EnsureStartupAndUserExist(startupId, userId);

            if (await _context.StartupLikes.AnyAsync(l => l.StartupId == startupId && l.UserId == userId))
            {
                return false; // already liked
            }

            _context.StartupLikes.Add(new StartupLike
            {
                StartupId = startupId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlikeAsync(int startupId, int userId)
        {
            var like = await _context.StartupLikes
                .FirstOrDefaultAsync(l => l.StartupId == startupId && l.UserId == userId);
            if (like == null)
                return false;

            _context.StartupLikes.Remove(like);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> AddFavoriteAsync(int startupId, int userId)
        {
            await EnsureStartupAndUserExist(startupId, userId);

            if (await _context.Favorites.AnyAsync(f => f.StartupId == startupId && f.UserId == userId))
            {
                return false; // already favorited
            }

            _context.Favorites.Add(new Favorite
            {
                StartupId = startupId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RemoveFavoriteAsync(int startupId, int userId)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.StartupId == startupId && f.UserId == userId);
            if (favorite == null)
                return false;

            _context.Favorites.Remove(favorite);
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task EnsureStartupAndUserExist(int startupId, int userId)
        {
            if (!await _context.Startups.AnyAsync(s => s.Id == startupId))
            {
                throw new InvalidOperationException("Startup does not exist.");
            }

            if (!await _context.Users.AnyAsync(u => u.Id == userId))
            {
                throw new InvalidOperationException("User does not exist.");
            }
        }

        private async Task EnsureNotDeletedAsync(int id)
        {
            var deleted = await _context.Startups.AnyAsync(s =>
                s.Id == id && (!s.IsActive || s.StatusId == StartupStatuses.Deleted));
            if (deleted)
            {
                throw new UserException("This startup has been deleted and cannot be moderated.");
            }
        }

        #endregion

        #region Content-based recommendations

        /// <summary>
        /// Content-based filtering by category: builds the user's category-interest
        /// profile from likes, favorites and completed donations (weighted), then
        /// recommends approved startups from those categories the user has not
        /// interacted with, ordered by score and popularity.
        /// </summary>
        public async Task<List<StartupResponse>> GetRecommendedStartupsAsync(int userId, int count)
        {
            if (count <= 0) count = 5;

            // 1. Collect the user's interactions
            var likedStartupIds = await _context.StartupLikes
                .Where(l => l.UserId == userId)
                .Select(l => l.StartupId)
                .ToListAsync();

            var favoritedStartupIds = await _context.Favorites
                .Where(f => f.UserId == userId)
                .Select(f => f.StartupId)
                .ToListAsync();

            var donatedStartupIds = await _context.Donations
                .Where(d => d.UserId == userId && d.Status == "Completed")
                .Select(d => d.StartupId)
                .ToListAsync();

            var interactedStartupIds = likedStartupIds
                .Concat(favoritedStartupIds)
                .Concat(donatedStartupIds)
                .Distinct()
                .ToHashSet();

            // 2. Build a weighted category-interest profile
            //    (donation = strongest signal, then favorite, then like)
            var categoryWeights = new Dictionary<int, double>();

            void AddWeight(int categoryId, double weight)
            {
                categoryWeights.TryGetValue(categoryId, out var current);
                categoryWeights[categoryId] = current + weight;
            }

            var interactionCategories = await _context.Startups
                .Where(s => interactedStartupIds.Contains(s.Id))
                .Select(s => new { s.Id, s.CategoryId })
                .ToListAsync();

            foreach (var item in interactionCategories)
            {
                if (likedStartupIds.Contains(item.Id)) AddWeight(item.CategoryId, 3);
                if (favoritedStartupIds.Contains(item.Id)) AddWeight(item.CategoryId, 4);
                if (donatedStartupIds.Contains(item.Id)) AddWeight(item.CategoryId, 5);
            }

            // 3. Candidates: approved, active startups the user hasn't interacted with
            //    and didn't found themselves
            var candidates = await BaseQuery
                .Where(s => s.IsActive
                    && s.StatusId == StartupStatuses.Approved
                    && s.FounderId != userId
                    && !interactedStartupIds.Contains(s.Id))
                .ToListAsync();

            const string popularReason = "Popular approved startup";
            var categoryNames = await _context.Categories
                .ToDictionaryAsync(c => c.Id, c => c.Name);

            string CategoryReason(int categoryId)
            {
                var name = categoryNames.TryGetValue(categoryId, out var n) && !string.IsNullOrWhiteSpace(n)
                    ? n
                    : "this category";
                return $"Based on your interest in {name}";
            }

            var recommended = new List<(Startup Startup, string Reason)>();

            if (categoryWeights.Any())
            {
                // Score = category interest + small popularity boost
                recommended = candidates
                    .Select(s => new
                    {
                        Startup = s,
                        Score = (categoryWeights.TryGetValue(s.CategoryId, out var w) ? w : 0)
                                + s.StartupLikes.Count * 0.1
                                + s.Donations.Count(d => d.Status == "Completed") * 0.2
                    })
                    .Where(x => x.Score > 0)
                    .OrderByDescending(x => x.Score)
                    .ThenByDescending(x => x.Startup.CreatedAt)
                    .Take(count)
                    .Select(x => (x.Startup, CategoryReason(x.Startup.CategoryId)))
                    .ToList();

                // Fill up with popular startups if not enough category matches
                if (recommended.Count < count)
                {
                    var already = recommended.Select(r => r.Startup).ToHashSet();
                    var fill = candidates
                        .Where(s => !already.Contains(s))
                        .OrderByDescending(s => s.StartupLikes.Count + s.Donations.Count(d => d.Status == "Completed"))
                        .ThenByDescending(s => s.CreatedAt)
                        .Take(count - recommended.Count)
                        .Select(s => (s, popularReason));
                    recommended.AddRange(fill);
                }
            }
            else
            {
                // Cold start: most popular approved startups
                recommended = candidates
                    .OrderByDescending(s => s.StartupLikes.Count + s.Donations.Count(d => d.Status == "Completed"))
                    .ThenByDescending(s => s.CreatedAt)
                    .Take(count)
                    .Select(s => (s, popularReason))
                    .ToList();
            }

            return recommended.Select(item =>
            {
                var response = MapToResponse(item.Startup);
                response.RecommendationReason = item.Reason;
                return response;
            }).ToList();
        }

        #endregion

        /// <summary>
        /// Soft-deletes a startup owned by the given user.
        /// </summary>
        public async Task<bool> DeleteOwnedAsync(int id, int userId)
        {
            var entity = await _context.Startups.FindAsync(id);
            if (entity == null)
                return false;

            if (entity.FounderId != userId)
            {
                throw new UserException("You can only delete your own startups.");
            }

            entity.IsActive = false;
            entity.StatusId = StartupStatuses.Deleted;
            entity.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Soft-delete via standard DELETE; founder is resolved from the auth claims.
        /// </summary>
        public override async Task<bool> DeleteAsync(int id)
        {
            var claimId = _httpContextAccessor.HttpContext?.User
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimId, out var userId))
            {
                throw new UserException("You must be signed in to delete a startup.");
            }

            return await DeleteOwnedAsync(id, userId);
        }
    }
}
