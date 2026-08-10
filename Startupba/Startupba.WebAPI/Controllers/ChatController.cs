using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Startupba.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChatController : BaseCRUDController<ChatResponse, ChatSearchObject, ChatUpsertRequest, ChatUpsertRequest>
    {
        private readonly IChatService _chatService;

        public ChatController(IChatService service) : base(service)
        {
            _chatService = service;
        }

        [HttpGet("optimized")]
        public async Task<ActionResult<PagedResult<ChatResponse>>> GetOptimized([FromQuery] ChatSearchObject? search = null)
        {
            return await _chatService.GetOptimizedAsync(search ?? new ChatSearchObject());
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var result = await _chatService.MarkAsReadAsync(id);
            if (!result)
                return NotFound();

            return Ok();
        }

        [HttpGet("unread-count")]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimId, out var userId))
                return Unauthorized();
            return await _chatService.GetUnreadCountAsync(userId);
        }

        [HttpPost("mark-conversation-read")]
        public async Task<IActionResult> MarkConversationAsRead([FromQuery] int senderId)
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimId, out var receiverId))
                return Unauthorized();

            var result = await _chatService.MarkConversationAsReadAsync(senderId, receiverId);
            if (!result)
                return NotFound();

            return Ok();
        }

        [HttpGet("conversations")]
        public async Task<ActionResult<List<ConversationResponse>>> GetConversations()
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimId, out var userId))
                return Unauthorized();
            return await _chatService.GetConversationsAsync(userId);
        }

        [HttpGet("conversation/{otherUserId}")]
        public async Task<ActionResult<PagedResult<ChatResponse>>> GetConversationMessages(
            int otherUserId,
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 50)
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimId, out var userId))
                return Unauthorized();
            return await _chatService.GetConversationMessagesAsync(userId, otherUserId, page, pageSize);
        }
    }
}
