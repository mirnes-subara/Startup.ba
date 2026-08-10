using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

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
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResult<NotificationResponse>>> Get([FromQuery] NotificationSearchObject search)
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimId, out var userId))
                return Unauthorized();

            search ??= new NotificationSearchObject();
            if (!User.IsInRole("Administrator"))
            {
                search.UserId = userId;
            }

            var result = await _service.GetAsync(search);
            return Ok(result);
        }

        /// <summary>
        /// Gets the count of unread notifications for the current user.
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<ActionResult<UnreadCountResponse>> GetUnreadCount()
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimId, out var userId))
                return Unauthorized();

            var count = await _service.GetUnreadCountAsync(userId);
            return Ok(new UnreadCountResponse { Count = count });
        }

        /// <summary>
        /// Marks all notifications as read for the current user.
        /// </summary>
        [HttpPost("mark-all-read")]
        public async Task<ActionResult> MarkAllAsRead()
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimId, out var userId))
                return Unauthorized();

            var count = await _service.MarkAllAsReadAsync(userId);
            return Ok(new { success = true, markedCount = count });
        }

        /// <summary>
        /// Gets a notification by ID.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<NotificationResponse>> GetById(int id)
        {
            var result = await _service.GetByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Marks a single notification as read.
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
