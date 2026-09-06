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
using System.Linq;
using System.Threading.Tasks;

namespace Startupba.Services.Services
{
    public class DonationService : BaseCRUDService<DonationResponse, DonationSearchObject, Donation, DonationUpsertRequest, DonationUpsertRequest>, IDonationService
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<DonationService> _logger;
        private readonly IRabbitMqPublisher _rabbitMqPublisher;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DonationService(
            StartupbaDbContext context,
            IMapper mapper,
            INotificationService notificationService,
            ILogger<DonationService> logger,
            IRabbitMqPublisher rabbitMqPublisher,
            IHttpContextAccessor httpContextAccessor) : base(context, mapper)
        {
            _notificationService = notificationService;
            _logger = logger;
            _rabbitMqPublisher = rabbitMqPublisher;
            _httpContextAccessor = httpContextAccessor;
        }

        private IQueryable<Donation> BaseQuery => _context.Donations
            .Include(d => d.Startup)
            .Include(d => d.User);

        public override async Task<PagedResult<DonationResponse>> GetAsync(DonationSearchObject search)
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
            return new PagedResult<DonationResponse>
            {
                Items = list.Select(MapToResponse).ToList(),
                TotalCount = totalCount
            };
        }

        protected override IQueryable<Donation> ApplyFilter(IQueryable<Donation> query, DonationSearchObject search)
        {
            if (search.StartupId.HasValue)
            {
                query = query.Where(d => d.StartupId == search.StartupId.Value);
            }

            if (search.UserId.HasValue)
            {
                query = query.Where(d => d.UserId == search.UserId.Value);
            }

            if (search.FounderId.HasValue)
            {
                query = query.Where(d => d.Startup.FounderId == search.FounderId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search.Status))
            {
                query = query.Where(d => d.Status == search.Status);
            }

            if (search.DateFrom.HasValue)
            {
                query = query.Where(d => d.CreatedAt >= search.DateFrom.Value);
            }

            if (search.DateTo.HasValue)
            {
                query = query.Where(d => d.CreatedAt <= search.DateTo.Value);
            }

            if (search.MinAmount.HasValue)
            {
                query = query.Where(d => d.Amount >= search.MinAmount.Value);
            }

            if (search.MaxAmount.HasValue)
            {
                query = query.Where(d => d.Amount <= search.MaxAmount.Value);
            }

            if (!string.IsNullOrWhiteSpace(search.FTS))
            {
                query = query.Where(d =>
                    d.Startup.Name.Contains(search.FTS) ||
                    (d.Message != null && d.Message.Contains(search.FTS)));
            }

            return query.OrderByDescending(d => d.Id);
        }

        public override async Task<DonationResponse?> GetByIdAsync(int id)
        {
            var entity = await BaseQuery.FirstOrDefaultAsync(d => d.Id == id);
            if (entity == null)
                return null;

            var currentUserId = _httpContextAccessor.GetUserId();
            var isAdmin = _httpContextAccessor.IsAdministrator();
            if (!isAdmin
                && entity.UserId != currentUserId
                && entity.Startup?.FounderId != currentUserId)
            {
                return null;
            }

            return MapToResponse(entity);
        }

        protected DonationResponse MapToResponse(Donation entity)
        {
            var response = _mapper.Map<DonationResponse>(entity);

            response.StartupName = entity.Startup?.Name ?? string.Empty;

            if (entity.User != null)
            {
                response.UserName = $"{entity.User.FirstName} {entity.User.LastName}";
            }

            return response;
        }

        protected override async Task BeforeInsert(Donation entity, DonationUpsertRequest request)
        {
            request.UserId = _httpContextAccessor.RequireUserId();
            entity.UserId = request.UserId;

            var startup = await _context.Startups.FirstOrDefaultAsync(s => s.Id == request.StartupId);
            if (startup == null)
            {
                throw new UserException("Startup does not exist.");
            }

            if (startup.StatusId != StartupStatuses.Approved)
            {
                throw new UserException("Donations are only possible for approved startups.");
            }

            if (request.Amount <= 0)
            {
                throw new UserException("Donation amount must be greater than zero.");
            }
        }

        protected override Donation MapInsertToEntity(Donation entity, DonationUpsertRequest request)
        {
            base.MapInsertToEntity(entity, request);

            entity.UserId = request.UserId;
            entity.StartupId = request.StartupId;
            entity.Amount = request.Amount;
            entity.Status = "Pending";
            entity.CreatedAt = DateTime.UtcNow;

            return entity;
        }

        protected override Task BeforeUpdate(Donation entity, DonationUpsertRequest request)
        {
            throw new UserException("Donation amount, startup, and donor cannot be changed.");
        }

        protected override void MapUpdateToEntity(Donation entity, DonationUpsertRequest request)
        {
            // Financial fields and status are immutable after create.
        }

        protected override Task BeforeDelete(Donation entity)
        {
            throw new UserException("Donations cannot be deleted.");
        }

        /// <summary>
        /// Marks a pending donation as completed, updates the startup's raised amount,
        /// notifies the founder (in-app + email via RabbitMQ) and auto-completes the
        /// startup when the target amount is reached.
        /// </summary>
        public async Task<DonationResponse?> CompleteAsync(int id)
        {
            var booking = await ApplyCompletionAsync(id);
            await _context.SaveChangesAsync();

            if (booking.Applied)
                await NotifyCompletionAsync(id, booking.TargetReached);

            var entity = await BaseQuery.FirstOrDefaultAsync(d => d.Id == id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<DonationBookingResult> ApplyCompletionAsync(int id)
        {
            var entity = await _context.Donations
                .Include(d => d.Startup)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (entity == null)
                throw new NotFoundException($"Donation with ID {id} not found.");

            if (string.Equals(entity.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                return new DonationBookingResult { Applied = false, TargetReached = false };
            }

            entity.Status = "Completed";
            entity.CompletedAt = DateTime.UtcNow;

            var startup = entity.Startup;
            startup.AmountRaised += entity.Amount;
            startup.UpdatedAt = DateTime.UtcNow;

            var targetReached = startup.AmountRaised >= startup.TargetAmount
                && startup.StatusId == StartupStatuses.Approved;
            if (targetReached)
            {
                var history = StartupStatusMachine.Transition(
                    startup,
                    StartupStatuses.Completed,
                    _httpContextAccessor.GetUserId(),
                    "Funding target reached");
                _context.StartupStatusHistories.Add(history);
            }

            return new DonationBookingResult { Applied = true, TargetReached = targetReached };
        }

        public async Task NotifyCompletionAsync(int donationId, bool targetReached)
        {
            var entity = await _context.Donations
                .Include(d => d.User)
                .Include(d => d.Startup)
                    .ThenInclude(s => s.Founder)
                .Include(d => d.Startup)
                    .ThenInclude(s => s.Category)
                .Include(d => d.Startup)
                    .ThenInclude(s => s.City)
                        .ThenInclude(c => c.Country)
                .FirstOrDefaultAsync(d => d.Id == donationId);

            if (entity?.Startup == null)
                return;

            var startup = entity.Startup;
            var donorName = entity.User != null
                ? $"{entity.User.FirstName} {entity.User.LastName}"
                : "An investor";

            try
            {
                await _notificationService.CreateNotificationAsync(
                    startup.FounderId,
                    "Donation Received",
                    $"{donorName} donated {entity.Amount:F2} EUR to \"{startup.Name}\".",
                    NotificationTypes.DonationReceived,
                    entity.Id,
                    "Donation");

                if (targetReached)
                {
                    await _notificationService.CreateNotificationAsync(
                        startup.FounderId,
                        "Funding Target Reached",
                        $"Congratulations! \"{startup.Name}\" has reached its funding target of {startup.TargetAmount:F2} EUR.",
                        NotificationTypes.DonationReceived,
                        startup.Id,
                        "Startup");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send donation notification");
            }

            if (startup.Founder != null)
            {
                await _rabbitMqPublisher.PublishEmailAsync(new EmailNotificationDto
                {
                    NotificationType = "DonationReceived",
                    RecipientEmail = startup.Founder.Email,
                    RecipientFullName = $"{startup.Founder.FirstName} {startup.Founder.LastName}".Trim(),
                    StartupId = startup.Id,
                    StartupName = startup.Name,
                    CategoryName = startup.Category?.Name ?? string.Empty,
                    CityName = startup.City?.Name ?? string.Empty,
                    CountryName = startup.City?.Country?.Name ?? string.Empty,
                    TargetAmount = startup.TargetAmount,
                    AmountRaised = startup.AmountRaised,
                    PlatformFeePercent = startup.PlatformFeePercent,
                    DonorFullName = donorName,
                    DonationAmount = entity.Amount,
                    DonationMessage = entity.Message
                });
            }
        }

        /// <summary>
        /// Marks a completed donation as Refunded and rolls back AmountRaised.
        /// Does not reverse a Completed startup status (avoids surprising founders).
        /// </summary>
        public async Task<DonationResponse?> RefundAsync(int id)
        {
            await ApplyRefundAsync(id);
            await _context.SaveChangesAsync();

            var entity = await BaseQuery.FirstOrDefaultAsync(d => d.Id == id);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task ApplyRefundAsync(int id)
        {
            var entity = await _context.Donations
                .Include(d => d.Startup)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (entity == null)
                throw new NotFoundException($"Donation with ID {id} not found.");

            if (string.Equals(entity.Status, "Refunded", StringComparison.OrdinalIgnoreCase))
                return;

            if (!string.Equals(entity.Status, "Completed", StringComparison.OrdinalIgnoreCase))
            {
                throw new UserException("Only completed donations can be refunded.");
            }

            entity.Status = "Refunded";

            var startup = entity.Startup;
            startup.AmountRaised = Math.Max(0, startup.AmountRaised - entity.Amount);
            startup.UpdatedAt = DateTime.UtcNow;
        }
    }
}
