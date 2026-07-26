using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Database;
using Startupba.Services.Helpers;
using Startupba.Services.Interfaces;
using Startupba.Subscriber.Models;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Startupba.Services.Services
{
    public class DonationService : BaseCRUDService<DonationResponse, DonationSearchObject, Donation, DonationUpsertRequest, DonationUpsertRequest>, IDonationService
    {
        private readonly INotificationService _notificationService;

        public DonationService(StartupbaDbContext context, IMapper mapper, INotificationService notificationService) : base(context, mapper)
        {
            _notificationService = notificationService;
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

            if (!search.RetrieveAll)
            {
                if (search.Page.HasValue)
                {
                    query = query.Skip(search.Page.Value * search.PageSize.Value);
                }
                if (search.PageSize.HasValue)
                {
                    query = query.Take(search.PageSize.Value);
                }
            }

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
            var startup = await _context.Startups.FirstOrDefaultAsync(s => s.Id == request.StartupId);
            if (startup == null)
            {
                throw new InvalidOperationException("Startup does not exist.");
            }

            if (startup.StatusId != StartupStatuses.Approved)
            {
                throw new InvalidOperationException("Donations are only possible for approved startups.");
            }

            if (!await _context.Users.AnyAsync(u => u.Id == request.UserId))
            {
                throw new InvalidOperationException("User does not exist.");
            }

            if (request.Amount <= 0)
            {
                throw new InvalidOperationException("Donation amount must be greater than zero.");
            }
        }

        protected override Donation MapInsertToEntity(Donation entity, DonationUpsertRequest request)
        {
            base.MapInsertToEntity(entity, request);

            // Donations start as Pending and are completed once the payment is confirmed
            entity.Status = "Pending";
            entity.CreatedAt = DateTime.Now;

            return entity;
        }

        /// <summary>
        /// Marks a pending donation as completed, updates the startup's raised amount,
        /// notifies the founder (in-app + email via RabbitMQ) and auto-completes the
        /// startup when the target amount is reached.
        /// </summary>
        public async Task<DonationResponse?> CompleteAsync(int id)
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
                .FirstOrDefaultAsync(d => d.Id == id);

            if (entity == null)
                return null;

            if (entity.Status == "Completed")
            {
                throw new InvalidOperationException("Donation is already completed.");
            }

            entity.Status = "Completed";
            entity.CompletedAt = DateTime.Now;

            // Update the startup's raised amount
            var startup = entity.Startup;
            startup.AmountRaised += entity.Amount;
            startup.UpdatedAt = DateTime.Now;

            // Auto-complete the startup when the funding target is reached
            bool targetReached = startup.AmountRaised >= startup.TargetAmount
                && startup.StatusId == StartupStatuses.Approved;
            if (targetReached)
            {
                startup.StatusId = StartupStatuses.Completed;
                startup.CompletedAt = DateTime.Now;
            }

            await _context.SaveChangesAsync();

            var donorName = entity.User != null
                ? $"{entity.User.FirstName} {entity.User.LastName}"
                : "An investor";

            // In-app notification to the founder
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
                Console.WriteLine($"Notification error: {ex.Message}");
            }

            // Email notification to the founder via RabbitMQ
            if (startup.Founder != null)
            {
                await RabbitMqPublisher.PublishEmailAsync(new EmailNotificationDto
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

            return MapToResponse(entity);
        }
    }
}
