using Startupba.Model;
using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Database;
using Startupba.Services.Helpers;
using Startupba.Services.Interfaces;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Startupba.Services.Services
{
    public class ChatService : BaseCRUDService<ChatResponse, ChatSearchObject, Chat, ChatUpsertRequest, ChatUpsertRequest>, IChatService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChatService(StartupbaDbContext context, IMapper mapper, IHttpContextAccessor httpContextAccessor)
            : base(context, mapper)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private int CurrentUserId => _httpContextAccessor.RequireUserId();

        private static bool IsParticipant(Chat chat, int userId)
            => chat.SenderId == userId || chat.ReceiverId == userId;

        protected override IQueryable<Chat> ApplyFilter(IQueryable<Chat> query, ChatSearchObject search)
        {
            query = query.Include(c => c.Sender)
                        .Include(c => c.Receiver);

            var currentUserId = CurrentUserId;
            query = query.Where(c => c.SenderId == currentUserId || c.ReceiverId == currentUserId);

            if (search.SenderId.HasValue)
            {
                query = query.Where(c => c.SenderId == search.SenderId.Value);
            }

            if (search.ReceiverId.HasValue)
            {
                query = query.Where(c => c.ReceiverId == search.ReceiverId.Value);
            }

            if (search.IsRead.HasValue)
            {
                query = query.Where(c => c.IsRead == search.IsRead.Value);
            }

            if (search.OnlyUnread == true)
            {
                query = query.Where(c => !c.IsRead);
            }

            if (!string.IsNullOrEmpty(search.FTS))
            {
                query = query.Where(c => c.Message.Contains(search.FTS));
            }

            return query.OrderByDescending(c => c.CreatedAt);
        }

        protected override async Task BeforeInsert(Chat entity, ChatUpsertRequest request)
        {
            entity.SenderId = _httpContextAccessor.RequireUserId();
            request.SenderId = entity.SenderId;
            entity.CreatedAt = DateTime.UtcNow;
            entity.IsRead = false;

            if (entity.ReceiverId == entity.SenderId)
            {
                throw new UserException("You cannot send a message to yourself.");
            }

            if (!await _context.Users.AnyAsync(u => u.Id == entity.ReceiverId))
            {
                throw new NotFoundException("Receiver does not exist.");
            }
        }

        public override Task<ChatResponse?> UpdateAsync(int id, ChatUpsertRequest request)
        {
            throw new UserException("Chat messages cannot be edited.");
        }

        public override Task<bool> DeleteAsync(int id)
        {
            throw new UserException("Chat messages cannot be deleted.");
        }

        public async Task<bool> MarkAsReadAsync(int chatId)
        {
            var chat = await _context.Chats.FindAsync(chatId);
            if (chat == null || chat.IsRead)
                return false;

            if (chat.ReceiverId != CurrentUserId)
                return false;

            chat.IsRead = true;
            chat.ReadAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            if (userId != CurrentUserId)
                throw new UserException("You can only read your own unread count.");

            return await _context.Chats
                .Where(c => c.ReceiverId == userId && !c.IsRead)
                .CountAsync();
        }

        public async Task<bool> MarkConversationAsReadAsync(int senderId, int receiverId)
        {
            if (receiverId != CurrentUserId)
                throw new UserException("You can only mark your own conversations as read.");

            var unreadMessages = await _context.Chats
                .Where(c => c.SenderId == senderId && c.ReceiverId == receiverId && !c.IsRead)
                .ToListAsync();

            if (!unreadMessages.Any())
                return false;

            foreach (var message in unreadMessages)
            {
                message.IsRead = true;
                message.ReadAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<PagedResult<ChatResponse>> GetOptimizedAsync(ChatSearchObject search)
        {
            search ??= new ChatSearchObject();
            var currentUserId = CurrentUserId;
            var query = _context.Chats
                .Where(c => c.SenderId == currentUserId || c.ReceiverId == currentUserId);

            if (search.SenderId.HasValue)
            {
                query = query.Where(c => c.SenderId == search.SenderId.Value);
            }

            if (search.ReceiverId.HasValue)
            {
                query = query.Where(c => c.ReceiverId == search.ReceiverId.Value);
            }

            if (search.IsRead.HasValue)
            {
                query = query.Where(c => c.IsRead == search.IsRead.Value);
            }

            if (search.OnlyUnread == true)
            {
                query = query.Where(c => !c.IsRead);
            }

            if (!string.IsNullOrEmpty(search.FTS))
            {
                query = query.Where(c => c.Message.Contains(search.FTS));
            }

            var totalCount = await query.CountAsync();

            var paged = PagingHelper.ApplyPaging(
                query.OrderByDescending(c => c.CreatedAt),
                search);

            var items = await paged
                .Select(c => new ChatResponse
                {
                    Id = c.Id,
                    SenderId = c.SenderId,
                    SenderName = $"{c.Sender.FirstName} {c.Sender.LastName}",
                    SenderPicture = null,
                    ReceiverId = c.ReceiverId,
                    ReceiverName = $"{c.Receiver.FirstName} {c.Receiver.LastName}",
                    ReceiverPicture = null,
                    Message = c.Message,
                    CreatedAt = c.CreatedAt,
                    IsRead = c.IsRead,
                    ReadAt = c.ReadAt
                })
                .ToListAsync();

            return new PagedResult<ChatResponse>
            {
                Items = items,
                TotalCount = totalCount
            };
        }

        protected override ChatResponse MapToResponse(Chat entity)
        {
            var response = _mapper.Map<ChatResponse>(entity);
            response.SenderName = $"{entity.Sender.FirstName} {entity.Sender.LastName}";
            response.SenderPicture = entity.Sender?.Picture;
            response.ReceiverName = $"{entity.Receiver.FirstName} {entity.Receiver.LastName}";
            response.ReceiverPicture = entity.Receiver?.Picture;
            return response;
        }

        public override async Task<ChatResponse?> GetByIdAsync(int id)
        {
            var entity = await _context.Chats
                .Include(c => c.Sender)
                .Include(c => c.Receiver)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (entity == null)
                return null;

            if (!IsParticipant(entity, CurrentUserId))
                return null;

            return MapToResponse(entity);
        }

        public override async Task<ChatResponse> CreateAsync(ChatUpsertRequest request)
        {
            var entity = new Chat();
            MapInsertToEntity(entity, request);

            await BeforeInsert(entity, request);

            _context.Add(entity);
            await _context.SaveChangesAsync();

            return await GetByIdAsync(entity.Id) ?? throw new UserException("Failed to create chat message");
        }

        public async Task<List<ConversationResponse>> GetConversationsAsync(int userId)
        {
            if (userId != CurrentUserId)
                throw new UserException("You can only list your own conversations.");

            var conversations = await _context.Chats
                .Where(c => c.SenderId == userId || c.ReceiverId == userId)
                .GroupBy(c => c.SenderId == userId ? c.ReceiverId : c.SenderId)
                .Select(g => new
                {
                    OtherUserId = g.Key,
                    LastMessage = g.OrderByDescending(c => c.CreatedAt).First(),
                    UnreadCount = g.Count(c => c.ReceiverId == userId && !c.IsRead)
                })
                .ToListAsync();

            var userIds = conversations.Select(c => c.OtherUserId).ToList();
            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id);

            var result = conversations.Select(c => new ConversationResponse
            {
                UserId = c.OtherUserId,
                UserName = users.ContainsKey(c.OtherUserId)
                    ? $"{users[c.OtherUserId].FirstName} {users[c.OtherUserId].LastName}"
                    : "Unknown User",
                UserPicture = users.ContainsKey(c.OtherUserId) ? users[c.OtherUserId].Picture : null,
                LastMessage = c.LastMessage.Message.Length > 50
                    ? c.LastMessage.Message.Substring(0, 50) + "..."
                    : c.LastMessage.Message,
                LastMessageAt = c.LastMessage.CreatedAt,
                UnreadCount = c.UnreadCount,
                IsLastMessageFromMe = c.LastMessage.SenderId == userId
            })
            .OrderByDescending(c => c.LastMessageAt)
            .ToList();

            return result;
        }

        public async Task<PagedResult<ChatResponse>> GetConversationMessagesAsync(int userId, int otherUserId, int page = 0, int pageSize = 50)
        {
            if (userId != CurrentUserId)
                throw new UserException("You can only read your own conversations.");

            var (safePage, size) = PagingHelper.Clamp(page, pageSize, defaultSize: 50);

            var query = _context.Chats
                .Include(c => c.Sender)
                .Include(c => c.Receiver)
                .Where(c =>
                    (c.SenderId == userId && c.ReceiverId == otherUserId) ||
                    (c.SenderId == otherUserId && c.ReceiverId == userId));

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip(safePage * size)
                .Take(size)
                .ToListAsync();

            return new PagedResult<ChatResponse>
            {
                Items = items.Select(MapToResponse).ToList(),
                TotalCount = totalCount
            };
        }
    }
}
