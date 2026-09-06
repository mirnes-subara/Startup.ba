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
    public class SupportTicketService : BaseCRUDService<SupportTicketResponse, SupportTicketSearchObject, SupportTicket, SupportTicketUpsertRequest, SupportTicketUpsertRequest>, ISupportTicketService
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<SupportTicketService> _logger;
        private readonly IRabbitMqPublisher _rabbitMqPublisher;
        private readonly IHttpContextAccessor _httpContextAccessor;

        private static readonly string[] StatusNames = { "Open", "Answered", "Closed" };

        public SupportTicketService(
            StartupbaDbContext context,
            IMapper mapper,
            INotificationService notificationService,
            ILogger<SupportTicketService> logger,
            IRabbitMqPublisher rabbitMqPublisher,
            IHttpContextAccessor httpContextAccessor) : base(context, mapper)
        {
            _notificationService = notificationService;
            _logger = logger;
            _rabbitMqPublisher = rabbitMqPublisher;
            _httpContextAccessor = httpContextAccessor;
        }

        private IQueryable<SupportTicket> BaseQuery => _context.SupportTickets
            .Include(st => st.User);

        public override async Task<PagedResult<SupportTicketResponse>> GetAsync(SupportTicketSearchObject search)
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
            return new PagedResult<SupportTicketResponse>
            {
                Items = list.Select(MapToResponse).ToList(),
                TotalCount = totalCount
            };
        }

        protected override IQueryable<SupportTicket> ApplyFilter(IQueryable<SupportTicket> query, SupportTicketSearchObject search)
        {
            if (!_httpContextAccessor.IsAdministrator())
            {
                var currentUserId = _httpContextAccessor.RequireUserId();
                search.UserId = currentUserId;
            }

            if (search.UserId.HasValue)
            {
                query = query.Where(st => st.UserId == search.UserId.Value);
            }

            if (search.Status.HasValue)
            {
                query = query.Where(st => st.Status == search.Status.Value);
            }

            if (!string.IsNullOrEmpty(search.FTS))
            {
                query = query.Where(st =>
                    st.Subject.Contains(search.FTS) ||
                    st.Message.Contains(search.FTS));
            }

            // Open tickets first, then newest
            return query.OrderBy(st => st.Status).ThenByDescending(st => st.Id);
        }

        public override async Task<SupportTicketResponse?> GetByIdAsync(int id)
        {
            var entity = await BaseQuery.FirstOrDefaultAsync(st => st.Id == id);
            if (entity == null)
                return null;

            if (!_httpContextAccessor.IsAdministrator()
                && entity.UserId != _httpContextAccessor.GetUserId())
            {
                return null;
            }

            return MapToResponse(entity);
        }

        protected SupportTicketResponse MapToResponse(SupportTicket entity)
        {
            var response = _mapper.Map<SupportTicketResponse>(entity);

            if (entity.User != null)
            {
                response.UserName = $"{entity.User.FirstName} {entity.User.LastName}";
            }

            response.StatusName = entity.Status >= 0 && entity.Status < StatusNames.Length
                ? StatusNames[entity.Status]
                : "Unknown";

            return response;
        }

        protected override Task BeforeInsert(SupportTicket entity, SupportTicketUpsertRequest request)
        {
            request.UserId = _httpContextAccessor.RequireUserId();
            entity.UserId = request.UserId;
            return Task.CompletedTask;
        }

        protected override SupportTicket MapInsertToEntity(SupportTicket entity, SupportTicketUpsertRequest request)
        {
            base.MapInsertToEntity(entity, request);
            entity.UserId = request.UserId;
            entity.Status = 0; // Open
            entity.CreatedAt = DateTime.UtcNow;
            return entity;
        }

        protected override Task BeforeUpdate(SupportTicket entity, SupportTicketUpsertRequest request)
        {
            _httpContextAccessor.EnsureOwnerOrAdmin(entity.UserId, "You can only edit your own support tickets.");
            request.UserId = entity.UserId;
            return Task.CompletedTask;
        }

        protected override void MapUpdateToEntity(SupportTicket entity, SupportTicketUpsertRequest request)
        {
            base.MapUpdateToEntity(entity, request);
            entity.UserId = request.UserId;
        }

        protected override Task BeforeDelete(SupportTicket entity)
        {
            _httpContextAccessor.EnsureOwnerOrAdmin(entity.UserId, "You can only delete your own support tickets.");
            return Task.CompletedTask;
        }

        public async Task<SupportTicketResponse?> AnswerAsync(int id, SupportTicketAnswerRequest request)
        {
            var entity = await BaseQuery.FirstOrDefaultAsync(st => st.Id == id);
            if (entity == null)
                return null;

            if (entity.Status == 2)
            {
                throw new UserException("Cannot answer a closed ticket.");
            }

            entity.AdminResponse = request.AdminResponse;
            entity.Status = 1; // Answered
            entity.AnsweredAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // In-app notification to the ticket creator
            try
            {
                await _notificationService.CreateNotificationAsync(
                    entity.UserId,
                    "Support Ticket Answered",
                    $"Support has answered your ticket \"{entity.Subject}\".",
                    NotificationTypes.TicketAnswered,
                    entity.Id,
                    "SupportTicket");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send support ticket notification");
            }

            // Email notification via RabbitMQ
            if (entity.User != null)
            {
                await _rabbitMqPublisher.PublishEmailAsync(new EmailNotificationDto
                {
                    NotificationType = "TicketAnswered",
                    RecipientEmail = entity.User.Email,
                    RecipientFullName = $"{entity.User.FirstName} {entity.User.LastName}".Trim(),
                    TicketSubject = entity.Subject,
                    AdminResponse = entity.AdminResponse
                });
            }

            return MapToResponse(entity);
        }

        public async Task<SupportTicketResponse?> CloseAsync(int id)
        {
            var entity = await BaseQuery.FirstOrDefaultAsync(st => st.Id == id);
            if (entity == null)
                return null;

            if (entity.Status == 2)
            {
                throw new UserException("Ticket is already closed.");
            }

            entity.Status = 2; // Closed
            entity.ClosedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToResponse(entity);
        }
    }
}
