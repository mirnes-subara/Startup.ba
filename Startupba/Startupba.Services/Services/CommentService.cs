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
    public class CommentService : BaseCRUDService<CommentResponse, CommentSearchObject, Comment, CommentUpsertRequest, CommentUpsertRequest>, ICommentService
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<CommentService> _logger;

        public CommentService(StartupbaDbContext context, IMapper mapper, INotificationService notificationService, ILogger<CommentService> logger) : base(context, mapper)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        private IQueryable<Comment> BaseQuery => _context.Comments
            .Include(c => c.User)
            .Include(c => c.BlogPost);

        public override async Task<PagedResult<CommentResponse>> GetAsync(CommentSearchObject search)
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
            return new PagedResult<CommentResponse>
            {
                Items = list.Select(MapToResponse).ToList(),
                TotalCount = totalCount
            };
        }

        protected override IQueryable<Comment> ApplyFilter(IQueryable<Comment> query, CommentSearchObject search)
        {
            if (search.BlogPostId.HasValue)
            {
                query = query.Where(c => c.BlogPostId == search.BlogPostId.Value);
            }

            if (search.UserId.HasValue)
            {
                query = query.Where(c => c.UserId == search.UserId.Value);
            }

            if (search.IsActive.HasValue)
            {
                query = query.Where(c => c.IsActive == search.IsActive.Value);
            }

            if (!string.IsNullOrEmpty(search.FTS))
            {
                query = query.Where(c => c.Content.Contains(search.FTS));
            }

            // Oldest first (natural conversation order)
            return query.OrderBy(c => c.Id);
        }

        public override async Task<CommentResponse?> GetByIdAsync(int id)
        {
            var entity = await BaseQuery.FirstOrDefaultAsync(c => c.Id == id);
            if (entity == null)
                return null;

            return MapToResponse(entity);
        }

        protected CommentResponse MapToResponse(Comment entity)
        {
            var response = _mapper.Map<CommentResponse>(entity);

            if (entity.User != null)
            {
                response.UserName = $"{entity.User.FirstName} {entity.User.LastName}";
            }

            response.BlogPostTitle = entity.BlogPost?.Title ?? string.Empty;

            return response;
        }

        protected override async Task BeforeInsert(Comment entity, CommentUpsertRequest request)
        {
            if (!await _context.BlogPosts.AnyAsync(bp => bp.Id == request.BlogPostId))
            {
                throw new InvalidOperationException("Blog post does not exist.");
            }

            if (!await _context.Users.AnyAsync(u => u.Id == request.UserId))
            {
                throw new InvalidOperationException("User does not exist.");
            }
        }

        public override async Task<CommentResponse> CreateAsync(CommentUpsertRequest request)
        {
            var result = await base.CreateAsync(request);

            // Notify the blog post author about the new comment (unless they commented themselves)
            try
            {
                var post = await _context.BlogPosts
                    .Include(bp => bp.Author)
                    .FirstOrDefaultAsync(bp => bp.Id == request.BlogPostId);

                if (post != null && post.AuthorId != request.UserId)
                {
                    var commenter = await _context.Users.FindAsync(request.UserId);
                    var commenterName = commenter != null
                        ? $"{commenter.FirstName} {commenter.LastName}"
                        : "Someone";

                    await _notificationService.CreateNotificationAsync(
                        post.AuthorId,
                        "New Comment",
                        $"{commenterName} commented on your post \"{post.Title}\".",
                        NotificationTypes.NewComment,
                        post.Id,
                        "BlogPost");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send comment notification");
            }

            return result;
        }
    }
}
