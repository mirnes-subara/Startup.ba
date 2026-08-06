using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Database;
using Startupba.Services.Helpers;
using Startupba.Services.Interfaces;
using Startupba.Subscriber.Models;
using MapsterMapper;
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

        private static readonly string[] StatusNames = { "Open", "Answered", "Closed" };

        public SupportTicketService(StartupbaDbContext context, IMapper mapper, INotificationService notificationService, ILogger<SupportTicketService> logger, IRabbitMqPublisher rabbitMqPublisher) : base(context, mapper)
        {
            _notificationService = notificationService;
            _logger = logger;
            _rabbitMqPublisher = rabbitMqPublisher;
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
            return new PagedResult<SupportTicketResponse>
            {
                Items = list.Select(MapToResponse).ToList(),
                TotalCount = totalCount
            };
        }

        protected override IQueryable<SupportTicket> ApplyFilter(IQueryable<SupportTicket> query, SupportTicketSearchObject search)
        {
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

        protected override async Task BeforeInsert(SupportTicket entity, SupportTicketUpsertRequest request)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == request.UserId))
            {
                throw new InvalidOperationException("User does not exist.");
            }
        }

        protected override SupportTicket MapInsertToEntity(SupportTicket entity, SupportTicketUpsertRequest request)
        {
            base.MapInsertToEntity(entity, request);
            entity.Status = 0; // Open
            entity.CreatedAt = DateTime.UtcNow;
            return entity;
        }

        public async Task<SupportTicketResponse?> AnswerAsync(int id, SupportTicketAnswerRequest request)
        {
            var entity = await BaseQuery.FirstOrDefaultAsync(st => st.Id == id);
            if (entity == null)
                return null;

            if (entity.Status == 2)
            {
                throw new InvalidOperationException("Cannot answer a closed ticket.");
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
                throw new InvalidOperationException("Ticket is already closed.");
            }

            entity.Status = 2; // Closed
            entity.ClosedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToResponse(entity);
        }
    }
}
