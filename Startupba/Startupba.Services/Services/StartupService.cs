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

        /// <summary>
        /// Filterable set without heavy collections or image blobs.
        /// </summary>
        private IQueryable<Startup> ListQuery => _context.Startups.AsNoTracking();

        /// <summary>
        /// Detail: lookups + image bytes for edit/details.
        /// </summary>
        private IQueryable<Startup> DetailQuery => _context.Startups
            .AsNoTracking()
            .Include(s => s.Founder)
            .Include(s => s.Category)
            .Include(s => s.City)
            .Include(s => s.Status)
            .Include(s => s.StartupImages);

        public override async Task<PagedResult<StartupResponse>> GetAsync(StartupSearchObject search)
        {
            var query = ListQuery.AsQueryable();
            query = ApplyFilter(query, search);

            int? totalCount = null;
            if (search.IncludeTotalCount)
            {
                totalCount = await query.CountAsync();
            }

            query = ApplyPaging(query, search);
            var userId = _httpContextAccessor.GetUserId();

            var rows = await ProjectListRows(query, userId).ToListAsync();
            return new PagedResult<StartupResponse>
            {
                Items = rows.Select(MapListRow).ToList(),
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

            var isAdmin = _httpContextAccessor.IsAdministrator();
            var currentUserId = _httpContextAccessor.GetUserId();

            if (isAdmin)
            {
                if (search.StatusId.HasValue)
                    query = query.Where(s => s.StatusId == search.StatusId.Value);
                if (search.FounderId.HasValue)
                    query = query.Where(s => s.FounderId == search.FounderId.Value);
            }
            else if (search.FounderId.HasValue && currentUserId.HasValue
                     && search.FounderId.Value == currentUserId.Value)
            {
                // Founder listing their own startups — any status they filter for
                query = query.Where(s => s.FounderId == currentUserId.Value);
                if (search.StatusId.HasValue)
                    query = query.Where(s => s.StatusId == search.StatusId.Value);
            }
            else
            {
                // Public browse: only discoverable statuses. Ignore foreign FounderId
                // and StatusId values that would leak non-public startups.
                query = query.Where(s =>
                    s.StatusId != StartupStatuses.Pending
                    && s.StatusId != StartupStatuses.Rejected
                    && s.StatusId != StartupStatuses.Paused
                    && s.StatusId != StartupStatuses.Draft
                    && s.StatusId != StartupStatuses.Deleted);

                if (search.StatusId.HasValue
                    && search.StatusId.Value != StartupStatuses.Pending
                    && search.StatusId.Value != StartupStatuses.Rejected
                    && search.StatusId.Value != StartupStatuses.Paused
                    && search.StatusId.Value != StartupStatuses.Draft
                    && search.StatusId.Value != StartupStatuses.Deleted)
                {
                    query = query.Where(s => s.StatusId == search.StatusId.Value);
                }

                if (search.FounderId.HasValue)
                    query = query.Where(s => s.FounderId == search.FounderId.Value);
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
            var entity = await DetailQuery.FirstOrDefaultAsync(s => s.Id == id);
            if (entity == null)
                return null;

            var isAdmin = _httpContextAccessor.IsAdministrator();
            var currentUserId = _httpContextAccessor.GetUserId();
            if (!isAdmin && entity.FounderId != currentUserId)
            {
                var isPublic =
                    entity.IsActive
                    && entity.StatusId != StartupStatuses.Pending
                    && entity.StatusId != StartupStatuses.Rejected
                    && entity.StatusId != StartupStatuses.Paused
                    && entity.StatusId != StartupStatuses.Draft
                    && entity.StatusId != StartupStatuses.Deleted;
                if (!isPublic)
                    return null;
            }

            if (currentUserId.HasValue && entity.FounderId != currentUserId.Value)
            {
                try
                {
                    await RecordViewAsync(id, currentUserId.Value);
                }
                catch
                {
                    // Viewing must not fail the detail response.
                }
            }

            return await MapToDetailResponseAsync(entity);
        }

        protected override StartupResponse MapToResponse(Startup entity)
        {
            var cover = entity.StartupImages?
                .Where(i => i.IsActive && !i.IsLogo)
                .OrderByDescending(i => i.IsCover)
                .ThenBy(i => i.DisplayOrder)
                .FirstOrDefault();
            var logo = entity.StartupImages?
                .Where(i => i.IsActive && i.IsLogo)
                .FirstOrDefault();

            var userId = _httpContextAccessor.GetUserId();
            var response = MapCore(
                entity,
                entity.StartupLikes?.Count ?? 0,
                entity.Favorites?.Count ?? 0,
                entity.Donations?.Count(d => d.Status == "Completed") ?? 0,
                userId.HasValue && (entity.StartupLikes?.Any(l => l.UserId == userId.Value) ?? false),
                userId.HasValue && (entity.Favorites?.Any(f => f.UserId == userId.Value) ?? false),
                cover?.Id,
                logo?.Id);
            response.CoverImage = cover?.ImageData;
            response.LogoImage = logo?.ImageData;
            return response;
        }

        private async Task<StartupResponse> MapToDetailResponseAsync(Startup entity)
        {
            var userId = _httpContextAccessor.GetUserId();
            var likeCount = await _context.StartupLikes.CountAsync(l => l.StartupId == entity.Id);
            var favoriteCount = await _context.Favorites.CountAsync(f => f.StartupId == entity.Id);
            var donationCount = await _context.Donations.CountAsync(d =>
                d.StartupId == entity.Id && d.Status == "Completed");
            var isLiked = userId.HasValue
                && await _context.StartupLikes.AnyAsync(l => l.StartupId == entity.Id && l.UserId == userId.Value);
            var isFavorited = userId.HasValue
                && await _context.Favorites.AnyAsync(f => f.StartupId == entity.Id && f.UserId == userId.Value);

            var cover = entity.StartupImages?
                .Where(i => i.IsActive && !i.IsLogo)
                .OrderByDescending(i => i.IsCover)
                .ThenBy(i => i.DisplayOrder)
                .FirstOrDefault();
            var logo = entity.StartupImages?
                .Where(i => i.IsActive && i.IsLogo)
                .FirstOrDefault();

            var response = MapCore(entity, likeCount, favoriteCount, donationCount, isLiked, isFavorited, cover?.Id, logo?.Id);
            response.CoverImage = cover?.ImageData;
            response.LogoImage = logo?.ImageData;
            return response;
        }

        private IQueryable<StartupListRow> ProjectListRows(IQueryable<Startup> query, int? userId)
        {
            return query.Select(s => new StartupListRow
            {
                Id = s.Id,
                Name = s.Name,
                Description = s.Description,
                FounderId = s.FounderId,
                FounderName = s.Founder != null ? s.Founder.FirstName + " " + s.Founder.LastName : "",
                CategoryId = s.CategoryId,
                CategoryName = s.Category != null ? s.Category.Name : "",
                CityId = s.CityId,
                CityName = s.City != null ? s.City.Name : "",
                TargetAmount = s.TargetAmount,
                AmountRaised = s.AmountRaised,
                PlatformFeePercent = s.PlatformFeePercent,
                StatusId = s.StatusId,
                StatusName = s.Status != null ? s.Status.Name : "",
                RejectionReason = s.RejectionReason,
                ApprovedByUserId = s.ApprovedByUserId,
                RejectedByUserId = s.RejectedByUserId,
                PausedAt = s.PausedAt,
                PausedByUserId = s.PausedByUserId,
                IsActive = s.IsActive,
                CreatedAt = s.CreatedAt,
                UpdatedAt = s.UpdatedAt,
                ApprovedAt = s.ApprovedAt,
                CompletedAt = s.CompletedAt,
                LikeCount = s.StartupLikes.Count,
                FavoriteCount = s.Favorites.Count,
                DonationCount = s.Donations.Count(d => d.Status == "Completed"),
                IsLiked = userId.HasValue && s.StartupLikes.Any(l => l.UserId == userId.Value),
                IsFavorited = userId.HasValue && s.Favorites.Any(f => f.UserId == userId.Value),
                CoverImageId = s.StartupImages
                    .Where(i => i.IsActive && !i.IsLogo)
                    .OrderByDescending(i => i.IsCover)
                    .ThenBy(i => i.DisplayOrder)
                    .Select(i => (int?)i.Id)
                    .FirstOrDefault(),
                LogoImageId = s.StartupImages
                    .Where(i => i.IsActive && i.IsLogo)
                    .Select(i => (int?)i.Id)
                    .FirstOrDefault()
            });
        }

        private StartupResponse MapListRow(StartupListRow row)
        {
            return MapCore(
                row.Id, row.Name, row.Description, row.FounderId, row.FounderName,
                row.CategoryId, row.CategoryName, row.CityId, row.CityName,
                row.TargetAmount, row.AmountRaised, row.PlatformFeePercent,
                row.StatusId, row.StatusName, row.RejectionReason,
                row.ApprovedByUserId, row.RejectedByUserId, row.PausedAt, row.PausedByUserId,
                row.IsActive, row.CreatedAt, row.UpdatedAt, row.ApprovedAt, row.CompletedAt,
                row.LikeCount, row.FavoriteCount, row.DonationCount, row.IsLiked, row.IsFavorited,
                row.CoverImageId, row.LogoImageId);
        }

        private StartupResponse MapCore(Startup entity, int likeCount, int favoriteCount, int donationCount,
            bool isLiked, bool isFavorited, int? coverImageId, int? logoImageId)
        {
            var founderName = entity.Founder != null
                ? $"{entity.Founder.FirstName} {entity.Founder.LastName}"
                : string.Empty;
            return MapCore(
                entity.Id, entity.Name, entity.Description, entity.FounderId, founderName,
                entity.CategoryId, entity.Category?.Name ?? "", entity.CityId, entity.City?.Name ?? "",
                entity.TargetAmount, entity.AmountRaised, entity.PlatformFeePercent,
                entity.StatusId, entity.Status?.Name ?? "", entity.RejectionReason,
                entity.ApprovedByUserId, entity.RejectedByUserId, entity.PausedAt, entity.PausedByUserId,
                entity.IsActive, entity.CreatedAt, entity.UpdatedAt, entity.ApprovedAt, entity.CompletedAt,
                likeCount, favoriteCount, donationCount, isLiked, isFavorited,
                coverImageId, logoImageId);
        }

        private StartupResponse MapCore(
            int id, string name, string description, int founderId, string founderName,
            int categoryId, string categoryName, int cityId, string cityName,
            decimal targetAmount, decimal amountRaised, decimal platformFeePercent,
            int statusId, string statusName, string? rejectionReason,
            int? approvedByUserId, int? rejectedByUserId, DateTime? pausedAt, int? pausedByUserId,
            bool isActive, DateTime createdAt, DateTime? updatedAt, DateTime? approvedAt, DateTime? completedAt,
            int likeCount, int favoriteCount, int donationCount, bool isLiked, bool isFavorited,
            int? coverImageId, int? logoImageId)
        {
            return new StartupResponse
            {
                Id = id,
                Name = name,
                Description = description,
                FounderId = founderId,
                FounderName = founderName,
                CategoryId = categoryId,
                CategoryName = categoryName,
                CityId = cityId,
                CityName = cityName,
                TargetAmount = targetAmount,
                AmountRaised = amountRaised,
                FundingPercent = targetAmount > 0 ? Math.Round(amountRaised / targetAmount * 100, 2) : 0,
                PlatformFeePercent = platformFeePercent,
                StatusId = statusId,
                StatusName = statusName,
                RejectionReason = rejectionReason,
                ApprovedByUserId = approvedByUserId,
                RejectedByUserId = rejectedByUserId,
                PausedAt = pausedAt,
                PausedByUserId = pausedByUserId,
                IsActive = isActive,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                ApprovedAt = approvedAt,
                CompletedAt = completedAt,
                LikeCount = likeCount,
                FavoriteCount = favoriteCount,
                DonationCount = donationCount,
                IsLiked = isLiked,
                IsFavorited = isFavorited,
                CoverImageUrl = ImageFileUrl(coverImageId),
                LogoImageUrl = ImageFileUrl(logoImageId)
            };
        }

        private string? ImageFileUrl(int? imageId)
        {
            if (!imageId.HasValue)
                return null;
            var req = _httpContextAccessor.HttpContext?.Request;
            if (req == null)
                return $"/StartupImage/{imageId.Value}/file";
            return $"{req.Scheme}://{req.Host}{req.PathBase}/StartupImage/{imageId.Value}/file";
        }

        private sealed class StartupListRow
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Description { get; set; } = "";
            public int FounderId { get; set; }
            public string FounderName { get; set; } = "";
            public int CategoryId { get; set; }
            public string CategoryName { get; set; } = "";
            public int CityId { get; set; }
            public string CityName { get; set; } = "";
            public decimal TargetAmount { get; set; }
            public decimal AmountRaised { get; set; }
            public decimal PlatformFeePercent { get; set; }
            public int StatusId { get; set; }
            public string StatusName { get; set; } = "";
            public string? RejectionReason { get; set; }
            public int? ApprovedByUserId { get; set; }
            public int? RejectedByUserId { get; set; }
            public DateTime? PausedAt { get; set; }
            public int? PausedByUserId { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public DateTime? ApprovedAt { get; set; }
            public DateTime? CompletedAt { get; set; }
            public int LikeCount { get; set; }
            public int FavoriteCount { get; set; }
            public int DonationCount { get; set; }
            public bool IsLiked { get; set; }
            public bool IsFavorited { get; set; }
            public int? CoverImageId { get; set; }
            public int? LogoImageId { get; set; }
        }

        #endregion

        #region Create / Update

        protected override async Task BeforeInsert(Startup entity, StartupUpsertRequest request)
        {
            request.FounderId = _httpContextAccessor.RequireUserId();
            entity.FounderId = request.FounderId;

            if (!await _context.Categories.AnyAsync(c => c.Id == request.CategoryId && c.IsActive))
            {
                throw new NotFoundException("Category does not exist or is not active.");
            }

            if (!await _context.Cities.AnyAsync(c => c.Id == request.CityId))
            {
                throw new NotFoundException("City does not exist.");
            }
        }

        protected override Startup MapInsertToEntity(Startup entity, StartupUpsertRequest request)
        {
            base.MapInsertToEntity(entity, request);

            entity.FounderId = request.FounderId;
            entity.StatusId = StartupStatuses.Pending;
            entity.AmountRaised = 0;
            entity.CreatedAt = DateTime.UtcNow;
            return entity;
        }

        protected override async Task MapInsertToEntityAsync(Startup entity, StartupUpsertRequest request)
        {
            MapInsertToEntity(entity, request);

            var feeSetting = await _context.PlatformSettings
                .FirstOrDefaultAsync(ps => ps.Key == PlatformSettingKeys.PlatformFeePercent);
            if (feeSetting != null && decimal.TryParse(feeSetting.Value, out var fee))
            {
                entity.PlatformFeePercent = fee;
            }
        }

        public override async Task<StartupResponse> CreateAsync(StartupUpsertRequest request)
        {
            request.FounderId = _httpContextAccessor.RequireUserId();
            var result = await base.CreateAsync(request);

            _context.StartupStatusHistories.Add(new StartupStatusHistory
            {
                StartupId = result.Id,
                FromStatusId = StartupStatuses.Draft,
                ToStatusId = StartupStatuses.Pending,
                ActorUserId = result.FounderId,
                Reason = "Submitted for review",
                CreatedAt = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();

            await NotifyAdminsStartupSubmittedAsync(result.Id, result.Name);

            return await GetByIdAsync(result.Id) ?? result;
        }

        protected override async Task BeforeUpdate(Startup entity, StartupUpsertRequest request)
        {
            _httpContextAccessor.EnsureOwnerOrAdmin(entity.FounderId, "You can only edit your own startups.");

            // Prevent reassignment of ownership via the body
            request.FounderId = entity.FounderId;

            if (entity.StatusId == StartupStatuses.Completed)
            {
                throw new UserException("Completed startups cannot be edited.");
            }

            if (!await _context.Categories.AnyAsync(c => c.Id == request.CategoryId && c.IsActive))
            {
                throw new NotFoundException("Category does not exist or is not active.");
            }

            if (!await _context.Cities.AnyAsync(c => c.Id == request.CityId))
            {
                throw new NotFoundException("City does not exist.");
            }
        }

        protected override void MapUpdateToEntity(Startup entity, StartupUpsertRequest request)
        {
            base.MapUpdateToEntity(entity, request);
            entity.FounderId = request.FounderId;
            entity.UpdatedAt = DateTime.UtcNow;

            if (entity.StatusId == StartupStatuses.Rejected)
            {
                RecordTransition(entity, StartupStatuses.Pending, "Resubmitted after edits");
            }
        }

        protected override Task BeforeDelete(Startup entity)
        {
            _httpContextAccessor.EnsureOwnerOrAdmin(entity.FounderId, "You can only delete your own startups.");
            return Task.CompletedTask;
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
            var entity = await _context.Startups.FirstOrDefaultAsync(s => s.Id == id);
            if (entity == null)
                return null;

            await EnsureNotDeletedEntityAsync(entity);

            RecordTransition(entity, StartupStatuses.Approved);

            var feeSetting = await _context.PlatformSettings
                .FirstOrDefaultAsync(ps => ps.Key == PlatformSettingKeys.PlatformFeePercent);
            if (feeSetting != null && decimal.TryParse(feeSetting.Value, out var parsedFee))
            {
                entity.PlatformFeePercent = parsedFee;
            }

            await _context.SaveChangesAsync();

            await NotifyFounderAsync(entity, "Startup Approved",
                $"Congratulations! Your startup \"{entity.Name}\" has been approved and is now visible to investors.",
                NotificationTypes.StartupApproved);

            await SendStartupEmailAsync(entity, "StartupApproved");

            return await GetByIdAsync(id);
        }

        public async Task<StartupResponse?> RejectAsync(int id, StartupRejectRequest request)
        {
            var entity = await _context.Startups.FirstOrDefaultAsync(s => s.Id == id);
            if (entity == null)
                return null;

            await EnsureNotDeletedEntityAsync(entity);

            if (string.IsNullOrWhiteSpace(request.Reason))
                throw new UserException("A rejection reason is required.");

            RecordTransition(entity, StartupStatuses.Rejected, request.Reason);
            await _context.SaveChangesAsync();

            await NotifyFounderAsync(entity, "Startup Rejected",
                $"Your startup \"{entity.Name}\" has been rejected. Reason: {request.Reason}",
                NotificationTypes.StartupRejected);

            await SendStartupEmailAsync(entity, "StartupRejected");

            return await GetByIdAsync(id);
        }

        public async Task<StartupResponse?> PauseAsync(int id)
        {
            var entity = await _context.Startups.FirstOrDefaultAsync(s => s.Id == id);
            if (entity == null)
                return null;

            await EnsureNotDeletedEntityAsync(entity);

            RecordTransition(entity, StartupStatuses.Paused);
            await _context.SaveChangesAsync();

            await NotifyFounderAsync(entity, "Startup Paused",
                $"Your startup \"{entity.Name}\" has been paused by the administrator.",
                NotificationTypes.StartupPaused);

            return await GetByIdAsync(id);
        }

        public async Task<StartupResponse?> ResumeAsync(int id)
        {
            var entity = await _context.Startups.FirstOrDefaultAsync(s => s.Id == id);
            if (entity == null)
                return null;

            await EnsureNotDeletedEntityAsync(entity);

            RecordTransition(entity, StartupStatuses.Approved, "Resumed");
            await _context.SaveChangesAsync();

            await NotifyFounderAsync(entity, "Startup Resumed",
                $"Your startup \"{entity.Name}\" is active again and visible to investors.",
                NotificationTypes.StartupApproved);

            return await GetByIdAsync(id);
        }

        private void RecordTransition(Startup entity, int toStatusId, string? reason = null)
        {
            var history = StartupStatusMachine.Transition(
                entity,
                toStatusId,
                _httpContextAccessor.GetUserId(),
                reason);
            _context.StartupStatusHistories.Add(history);
        }

        private static Task EnsureNotDeletedEntityAsync(Startup entity)
        {
            if (!entity.IsActive || entity.StatusId == StartupStatuses.Deleted)
                throw new UserException("This startup has been deleted and cannot be moderated.");
            return Task.CompletedTask;
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

        private static readonly TimeSpan ViewThrottle = TimeSpan.FromMinutes(15);

        public async Task<bool> RecordViewAsync(int startupId, int userId)
        {
            var startup = await _context.Startups
                .Select(s => new { s.Id, s.FounderId, s.IsActive, s.StatusId })
                .FirstOrDefaultAsync(s => s.Id == startupId);

            if (startup == null)
                return false;

            if (startup.FounderId == userId)
                return false;

            if (!startup.IsActive || startup.StatusId == StartupStatuses.Deleted)
                return false;

            var existing = await _context.StartupViews
                .FirstOrDefaultAsync(v => v.StartupId == startupId && v.UserId == userId);

            var now = DateTime.UtcNow;
            if (existing != null)
            {
                if (now - existing.LastViewedAt < ViewThrottle)
                    return false;

                existing.LastViewedAt = now;
                await _context.SaveChangesAsync();
                return true;
            }

            _context.StartupViews.Add(new StartupView
            {
                StartupId = startupId,
                UserId = userId,
                CreatedAt = now,
                LastViewedAt = now
            });
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task EnsureStartupAndUserExist(int startupId, int userId)
        {
            if (!await _context.Startups.AnyAsync(s => s.Id == startupId))
            {
                throw new NotFoundException("Startup does not exist.");
            }

            if (!await _context.Users.AnyAsync(u => u.Id == userId))
            {
                throw new NotFoundException("User does not exist.");
            }
        }

        #endregion

        #region Content-based recommendations

        /// <summary>
        /// Content-based filtering by category: builds the user's category-interest
        /// profile from likes, favorites, completed donations and (weaker) detail views.
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

            var viewedStartupIds = await _context.StartupViews
                .Where(v => v.UserId == userId)
                .Select(v => v.StartupId)
                .ToListAsync();

            var interactedStartupIds = likedStartupIds
                .Concat(favoritedStartupIds)
                .Concat(donatedStartupIds)
                .Distinct()
                .ToHashSet();

            // 2. Build a weighted category-interest profile
            //    (donation = strongest, then favorite, like, then view)
            var categoryWeights = new Dictionary<int, double>();

            void AddWeight(int categoryId, double weight)
            {
                categoryWeights.TryGetValue(categoryId, out var current);
                categoryWeights[categoryId] = current + weight;
            }

            var signalStartupIds = likedStartupIds
                .Concat(favoritedStartupIds)
                .Concat(donatedStartupIds)
                .Concat(viewedStartupIds)
                .Distinct()
                .ToList();

            var interactionCategories = await _context.Startups
                .Where(s => signalStartupIds.Contains(s.Id))
                .Select(s => new { s.Id, s.CategoryId })
                .ToListAsync();

            foreach (var item in interactionCategories)
            {
                if (likedStartupIds.Contains(item.Id)) AddWeight(item.CategoryId, 3);
                if (favoritedStartupIds.Contains(item.Id)) AddWeight(item.CategoryId, 4);
                if (donatedStartupIds.Contains(item.Id)) AddWeight(item.CategoryId, 5);
                if (viewedStartupIds.Contains(item.Id)) AddWeight(item.CategoryId, 1);
            }

            // 3. Candidates: approved, active startups the user hasn't interacted with
            //    and didn't found themselves
            var candidateQuery = _context.Startups.AsNoTracking()
                .Where(s => s.IsActive
                    && s.StatusId == StartupStatuses.Approved
                    && s.FounderId != userId
                    && !interactedStartupIds.Contains(s.Id));

            var candidates = await ProjectListRows(candidateQuery, userId).ToListAsync();

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

            var recommended = new List<(StartupListRow Row, string Reason)>();

            if (categoryWeights.Any())
            {
                recommended = candidates
                    .Select(s => new
                    {
                        Row = s,
                        Score = (categoryWeights.TryGetValue(s.CategoryId, out var w) ? w : 0)
                                + s.LikeCount * 0.1
                                + s.DonationCount * 0.2
                    })
                    .Where(x => x.Score > 0)
                    .OrderByDescending(x => x.Score)
                    .ThenByDescending(x => x.Row.CreatedAt)
                    .Take(count)
                    .Select(x => (x.Row, CategoryReason(x.Row.CategoryId)))
                    .ToList();

                if (recommended.Count < count)
                {
                    var already = recommended.Select(r => r.Row.Id).ToHashSet();
                    var fill = candidates
                        .Where(s => !already.Contains(s.Id))
                        .OrderByDescending(s => s.LikeCount + s.DonationCount)
                        .ThenByDescending(s => s.CreatedAt)
                        .Take(count - recommended.Count)
                        .Select(s => (s, popularReason));
                    recommended.AddRange(fill);
                }
            }
            else
            {
                recommended = candidates
                    .OrderByDescending(s => s.LikeCount + s.DonationCount)
                    .ThenByDescending(s => s.CreatedAt)
                    .Take(count)
                    .Select(s => (s, popularReason))
                    .ToList();
            }

            return recommended.Select(item =>
            {
                var response = MapListRow(item.Row);
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

            if (entity.StatusId == StartupStatuses.Deleted)
                return true;

            RecordTransition(entity, StartupStatuses.Deleted, "Deleted by founder");
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Soft-delete via standard DELETE; founder is resolved from the auth claims.
        /// </summary>
        public override async Task<bool> DeleteAsync(int id)
        {
            return await DeleteOwnedAsync(id, _httpContextAccessor.RequireUserId());
        }
    }
}
