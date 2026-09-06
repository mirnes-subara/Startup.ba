using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;
using Startupba.WebAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Startupba.WebAPI.Controllers
{
    /// <summary>
    /// Manages user notifications for startup and platform activity.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _service;

        public NotificationController(INotificationService service)
        {
            _service = service;
        }

        /// <summary>
        /// Gets a paginated list of notifications with optional filtering.
        /// Non-admins are always scoped to the current user.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<NotificationResponse>>> Get([FromQuery] NotificationSearchObject search)
        {
            search ??= new NotificationSearchObject();
            if (!this.IsAdministrator())
                search.UserId = this.RequireUserId();

            var result = await _service.GetAsync(search);
            return Ok(result);
        }

        /// <summary>
        /// Gets the count of unread notifications for the current user.
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<ActionResult<UnreadCountResponse>> GetUnreadCount()
        {
            var count = await _service.GetUnreadCountAsync(this.RequireUserId());
            return Ok(new UnreadCountResponse { Count = count });
        }

        /// <summary>
        /// Marks all notifications as read for the current user.
        /// </summary>
        [HttpPost("mark-all-read")]
        public async Task<ActionResult> MarkAllAsRead()
        {
            var count = await _service.MarkAllAsReadAsync(this.RequireUserId());
            return Ok(new { success = true, markedCount = count });
        }

        /// <summary>
        /// Gets a notification by ID (recipient only).
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<NotificationResponse>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Marks a single notification as read (recipient only).
        /// </summary>
        [HttpPost("{id}/mark-read")]
        public async Task<ActionResult> MarkAsRead(int id)
        {
            var result = await _service.MarkAsReadAsync(id);
            if (!result) return NotFound();
            return Ok(new { success = true });
        }
    }
}
