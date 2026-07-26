using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Database;
using Startupba.Services.Interfaces;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Startupba.Services.Services
{
    public class AnnouncementService : BaseCRUDService<AnnouncementResponse, AnnouncementSearchObject, Announcement, AnnouncementUpsertRequest, AnnouncementUpsertRequest>, IAnnouncementService
    {
        private readonly INotificationService _notificationService;

        public AnnouncementService(StartupbaDbContext context, IMapper mapper, INotificationService notificationService) : base(context, mapper)
        {
            _notificationService = notificationService;
        }

        private IQueryable<Announcement> BaseQuery => _context.Announcements
            .Include(a => a.CreatedBy);

        public override async Task<PagedResult<AnnouncementResponse>> GetAsync(AnnouncementSearchObject search)
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
            return new PagedResult<AnnouncementResponse>
            {
                Items = list.Select(MapToResponse).ToList(),
                TotalCount = totalCount
            };
        }

        protected override IQueryable<Announcement> ApplyFilter(IQueryable<Announcement> query, AnnouncementSearchObject search)
        {
            if (!string.IsNullOrEmpty(search.Title))
            {
                query = query.Where(a => a.Title.Contains(search.Title));
            }

            if (!string.IsNullOrEmpty(search.FTS))
            {
                query = query.Where(a =>
                    a.Title.Contains(search.FTS) ||
                    a.Content.Contains(search.FTS));
            }

            if (search.IsActive.HasValue)
            {
                query = query.Where(a => a.IsActive == search.IsActive.Value);
            }

            // Newest first
            return query.OrderByDescending(a => a.Id);
        }

        public override async Task<AnnouncementResponse?> GetByIdAsync(int id)
        {
            var entity = await BaseQuery.FirstOrDefaultAsync(a => a.Id == id);
            if (entity == null)
                return null;

            return MapToResponse(entity);
        }

        protected AnnouncementResponse MapToResponse(Announcement entity)
        {
            var response = _mapper.Map<AnnouncementResponse>(entity);

            if (entity.CreatedBy != null)
            {
                response.CreatedByName = $"{entity.CreatedBy.FirstName} {entity.CreatedBy.LastName}";
            }

            return response;
        }

        protected override async Task BeforeInsert(Announcement entity, AnnouncementUpsertRequest request)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == request.CreatedByUserId))
            {
                throw new InvalidOperationException("Creator user does not exist.");
            }
        }

        public override async Task<AnnouncementResponse> CreateAsync(AnnouncementUpsertRequest request)
        {
            var result = await base.CreateAsync(request);

            // Broadcast an in-app notification to all active users (except the admin who created it)
            if (request.IsActive)
            {
                try
                {
                    var userIds = await _context.Users
                        .Where(u => u.IsActive && u.Id != request.CreatedByUserId)
                        .Select(u => u.Id)
                        .ToListAsync();

                    foreach (var userId in userIds)
                    {
                        await _notificationService.CreateNotificationAsync(
                            userId,
                            "New Announcement",
                            result.Title,
                            NotificationTypes.Announcement,
                            result.Id,
                            "Announcement");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Notification error: {ex.Message}");
                }
            }

            return result;
        }

        protected override void MapUpdateToEntity(Announcement entity, AnnouncementUpsertRequest request)
        {
            base.MapUpdateToEntity(entity, request);
            entity.UpdatedAt = DateTime.Now;
        }
    }
}
