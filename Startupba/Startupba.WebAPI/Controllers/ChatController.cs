using Startupba.Model;
using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;
using Startupba.WebAPI.Helpers;
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

        [HttpPost]
        public override async Task<ChatResponse> Create([FromBody] ChatUpsertRequest request)
        {
            request.SenderId = this.RequireUserId();
            return await _crudService.CreateAsync(request);
        }

        [HttpPut("{id}")]
        public override Task<ChatResponse?> Update(int id, [FromBody] ChatUpsertRequest request)
        {
            throw new UserException("Chat messages cannot be edited.");
        }

        [HttpDelete("{id}")]
        public override Task<bool> Delete(int id)
        {
            throw new UserException("Chat messages cannot be deleted.");
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
            return await _chatService.GetUnreadCountAsync(this.RequireUserId());
        }

        [HttpPost("mark-conversation-read")]
        public async Task<IActionResult> MarkConversationAsRead([FromQuery] int senderId)
        {
            var result = await _chatService.MarkConversationAsReadAsync(senderId, this.RequireUserId());
            if (!result)
                return NotFound();

            return Ok();
        }

        [HttpGet("conversations")]
        public async Task<ActionResult<List<ConversationResponse>>> GetConversations()
        {
            return await _chatService.GetConversationsAsync(this.RequireUserId());
        }

        [HttpGet("conversation/{otherUserId}")]
        public async Task<ActionResult<PagedResult<ChatResponse>>> GetConversationMessages(
            int otherUserId,
            [FromQuery] int page = 0,
            [FromQuery] int pageSize = 50)
        {
            return await _chatService.GetConversationMessagesAsync(this.RequireUserId(), otherUserId, page, pageSize);
        }
    }
}
