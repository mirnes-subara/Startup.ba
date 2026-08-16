using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Database;
using Startupba.Services.Helpers;
using Startupba.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Startupba.Services.Services
{
    public class NotificationService : INotificationService
    {
        private readonly StartupbaDbContext _context;

        private static readonly string[] TypeNames =
        {
            "Startup Submitted",   // 0
            "Startup Approved",    // 1
            "Startup Rejected",    // 2
            "Startup Paused",      // 3
            "Donation Received",   // 4
            "New Comment",         // 5
            "Ticket Answered",     // 6
            "Report Resolved",     // 7
            "Announcement",        // 8
            "Verification Requested" // 9
        };

        public NotificationService(StartupbaDbContext context)
        {
            _context = context;
        }

        public async Task<NotificationResponse> CreateNotificationAsync(
            int userId, string title, string message, int type,
            int? referenceId = null, string? referenceType = null)
        {
            var entity = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                ReferenceId = referenceId,
                ReferenceType = referenceType,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(entity);
            await _context.SaveChangesAsync();

            return MapToResponse(entity);
        }

        public async Task<PagedResult<NotificationResponse>> GetAsync(NotificationSearchObject search)
        {
            var query = ExcludeInactiveAnnouncementNotifications(_context.Notifications.AsQueryable());

            if (search.UserId.HasValue)
            {
                query = query.Where(n => n.UserId == search.UserId.Value);
            }

            if (search.Type.HasValue)
            {
                query = query.Where(n => n.Type == search.Type.Value);
            }

            if (search.IsRead.HasValue)
            {
                query = query.Where(n => n.IsRead == search.IsRead.Value);
            }

            if (!string.IsNullOrWhiteSpace(search.ReferenceType))
            {
                query = query.Where(n => n.ReferenceType == search.ReferenceType);
            }

            if (!string.IsNullOrWhiteSpace(search.FTS))
            {
                var fts = search.FTS.ToLower();
                query = query.Where(n =>
                    n.Title.ToLower().Contains(fts) ||
                    n.Message.ToLower().Contains(fts));
            }

            // Newest first
            query = query.OrderByDescending(n => n.CreatedAt);

            var result = new PagedResult<NotificationResponse>();

            if (search.IncludeTotalCount)
            {
                result.TotalCount = await query.CountAsync();
            }

            query = PagingHelper.ApplyPaging(query, search);

            var list = await query.ToListAsync();
            result.Items = list.Select(MapToResponse).ToList();

            return result;
        }

        public async Task<NotificationResponse?> GetByIdAsync(int id)
        {
            var entity = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id);
            if (entity == null) return null;
            return MapToResponse(entity);
        }

        public async Task<bool> MarkAsReadAsync(int id)
        {
            var entity = await _context.Notifications.FindAsync(id);
            if (entity == null) return false;

            entity.IsRead = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> MarkAllAsReadAsync(int userId)
        {
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread)
            {
                n.IsRead = true;
            }

            await _context.SaveChangesAsync();
            return unread.Count;
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await ExcludeInactiveAnnouncementNotifications(_context.Notifications.AsQueryable())
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task<int> DeleteByReferenceAsync(string referenceType, int referenceId)
        {
            var toDelete = await _context.Notifications
                .Where(n => n.ReferenceType == referenceType && n.ReferenceId == referenceId)
                .ToListAsync();

            if (toDelete.Count == 0)
                return 0;

            _context.Notifications.RemoveRange(toDelete);
            await _context.SaveChangesAsync();
            return toDelete.Count;
        }

        /// <summary>
        /// Hide notifications for announcements that were deactivated by an admin.
        /// </summary>
        private IQueryable<Notification> ExcludeInactiveAnnouncementNotifications(IQueryable<Notification> query)
        {
            var inactiveAnnouncementIds = _context.Announcements
                .Where(a => !a.IsActive)
                .Select(a => a.Id);

            return query.Where(n =>
                n.ReferenceType != "Announcement"
                || n.ReferenceId == null
                || !inactiveAnnouncementIds.Contains(n.ReferenceId.Value));
        }

        private static NotificationResponse MapToResponse(Notification entity)
        {
            return new NotificationResponse
            {
                Id = entity.Id,
                UserId = entity.UserId,
                Title = entity.Title,
                Message = entity.Message,
                Type = entity.Type,
                TypeName = entity.Type >= 0 && entity.Type < TypeNames.Length
                    ? TypeNames[entity.Type]
                    : "Unknown",
                ReferenceId = entity.ReferenceId,
                ReferenceType = entity.ReferenceType,
                IsRead = entity.IsRead,
                CreatedAt = entity.CreatedAt
            };
        }
    }
}
