using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Database;
using Startupba.Services.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Startupba.Services.Services
{
    public class ReportService : BaseCRUDService<ReportResponse, ReportSearchObject, Report, ReportUpsertRequest, ReportUpsertRequest>, IReportService
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<ReportService> _logger;

        private static readonly string[] TargetTypeNames = { "Startup", "BlogPost", "User" };
        private static readonly string[] StatusNames = { "Pending", "Reviewed", "Dismissed", "ActionTaken" };

        public ReportService(StartupbaDbContext context, IMapper mapper, INotificationService notificationService, ILogger<ReportService> logger) : base(context, mapper)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        private IQueryable<Report> BaseQuery => _context.Reports
            .Include(r => r.Reporter)
            .Include(r => r.Startup)
            .Include(r => r.BlogPost)
            .Include(r => r.ReportedUser);

        public override async Task<PagedResult<ReportResponse>> GetAsync(ReportSearchObject search)
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
            return new PagedResult<ReportResponse>
            {
                Items = list.Select(MapToResponse).ToList(),
                TotalCount = totalCount
            };
        }

        protected override IQueryable<Report> ApplyFilter(IQueryable<Report> query, ReportSearchObject search)
        {
            if (search.ReporterId.HasValue)
            {
                query = query.Where(r => r.ReporterId == search.ReporterId.Value);
            }

            if (search.TargetType.HasValue)
            {
                query = query.Where(r => r.TargetType == search.TargetType.Value);
            }

            if (search.Status.HasValue)
            {
                query = query.Where(r => r.Status == search.Status.Value);
            }

            if (search.StartupId.HasValue)
            {
                query = query.Where(r => r.StartupId == search.StartupId.Value);
            }

            if (search.BlogPostId.HasValue)
            {
                query = query.Where(r => r.BlogPostId == search.BlogPostId.Value);
            }

            if (search.ReportedUserId.HasValue)
            {
                query = query.Where(r => r.ReportedUserId == search.ReportedUserId.Value);
            }

            if (!string.IsNullOrEmpty(search.FTS))
            {
                query = query.Where(r =>
                    r.Reason.Contains(search.FTS) ||
                    (r.Description != null && r.Description.Contains(search.FTS)));
            }

            // Pending first, then newest
            return query.OrderBy(r => r.Status).ThenByDescending(r => r.Id);
        }

        public override async Task<ReportResponse?> GetByIdAsync(int id)
        {
            var entity = await BaseQuery.FirstOrDefaultAsync(r => r.Id == id);
            if (entity == null)
                return null;

            return MapToResponse(entity);
        }

        protected ReportResponse MapToResponse(Report entity)
        {
            var response = _mapper.Map<ReportResponse>(entity);

            if (entity.Reporter != null)
            {
                response.ReporterName = $"{entity.Reporter.FirstName} {entity.Reporter.LastName}";
            }

            response.TargetTypeName = entity.TargetType >= 0 && entity.TargetType < TargetTypeNames.Length
                ? TargetTypeNames[entity.TargetType]
                : "Unknown";

            response.StatusName = entity.Status >= 0 && entity.Status < StatusNames.Length
                ? StatusNames[entity.Status]
                : "Unknown";

            response.StartupName = entity.Startup?.Name;
            response.BlogPostTitle = entity.BlogPost?.Title;

            if (entity.ReportedUser != null)
            {
                response.ReportedUserName = $"{entity.ReportedUser.FirstName} {entity.ReportedUser.LastName}";
            }

            return response;
        }

        protected override async Task BeforeInsert(Report entity, ReportUpsertRequest request)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == request.ReporterId))
            {
                throw new InvalidOperationException("Reporter does not exist.");
            }

            // Exactly one target must be set, matching the target type
            switch (request.TargetType)
            {
                case 0: // Startup
                    if (!request.StartupId.HasValue)
                        throw new InvalidOperationException("StartupId is required when reporting a startup.");
                    if (!await _context.Startups.AnyAsync(s => s.Id == request.StartupId.Value))
                        throw new InvalidOperationException("Reported startup does not exist.");
                    break;
                case 1: // BlogPost
                    if (!request.BlogPostId.HasValue)
                        throw new InvalidOperationException("BlogPostId is required when reporting a blog post.");
                    if (!await _context.BlogPosts.AnyAsync(bp => bp.Id == request.BlogPostId.Value))
                        throw new InvalidOperationException("Reported blog post does not exist.");
                    break;
                case 2: // User
                    if (!request.ReportedUserId.HasValue)
                        throw new InvalidOperationException("ReportedUserId is required when reporting a user.");
                    if (!await _context.Users.AnyAsync(u => u.Id == request.ReportedUserId.Value))
                        throw new InvalidOperationException("Reported user does not exist.");
                    break;
                default:
                    throw new InvalidOperationException("Invalid target type. Use 0=Startup, 1=BlogPost, 2=User.");
            }
        }

        protected override Report MapInsertToEntity(Report entity, ReportUpsertRequest request)
        {
            base.MapInsertToEntity(entity, request);
            entity.Status = 0; // Pending
            entity.CreatedAt = DateTime.UtcNow;
            return entity;
        }

        public async Task<ReportResponse?> ResolveAsync(int id, ReportResolveRequest request)
        {
            var entity = await BaseQuery.FirstOrDefaultAsync(r => r.Id == id);
            if (entity == null)
                return null;

            if (entity.Status != 0)
            {
                throw new InvalidOperationException("Only pending reports can be resolved.");
            }

            entity.Status = request.Status;
            entity.AdminNote = request.AdminNote;
            entity.ResolvedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Notify the reporter about the outcome
            try
            {
                var statusName = request.Status >= 0 && request.Status < StatusNames.Length
                    ? StatusNames[request.Status]
                    : "Resolved";

                await _notificationService.CreateNotificationAsync(
                    entity.ReporterId,
                    "Report Resolved",
                    $"Your report \"{entity.Reason}\" has been reviewed. Outcome: {statusName}.",
                    NotificationTypes.ReportResolved,
                    entity.Id,
                    "Report");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send report notification");
            }

            return MapToResponse(entity);
        }
    }
}
