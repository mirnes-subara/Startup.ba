using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
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
        public async Task<ActionResult<int>> GetUnreadCount([FromQuery] int userId)
        {
            return await _chatService.GetUnreadCountAsync(userId);
        }

        [HttpPost("mark-conversation-read")]
        public async Task<IActionResult> MarkConversationAsRead([FromQuery] int senderId, [FromQuery] int receiverId)
        {
            var result = await _chatService.MarkConversationAsReadAsync(senderId, receiverId);
            if (!result)
                return NotFound();

            return Ok();
        }

        [HttpGet("conversations/{userId}")]
        public async Task<ActionResult<List<ConversationResponse>>> GetConversations(int userId)
        {
            return await _chatService.GetConversationsAsync(userId);
        }

        [HttpGet("conversation/{userId}/{otherUserId}")]
        public async Task<ActionResult<PagedResult<ChatResponse>>> GetConversationMessages(
            int userId, 
            int otherUserId, 
            [FromQuery] int page = 0, 
            [FromQuery] int pageSize = 50)
        {
            return await _chatService.GetConversationMessagesAsync(userId, otherUserId, page, pageSize);
        }
    }
} 