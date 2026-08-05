using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Database;
using Startupba.Services.Interfaces;
using MapsterMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Startupba.Services.Services
{
    public class BlogPostService : BaseCRUDService<BlogPostResponse, BlogPostSearchObject, BlogPost, BlogPostUpsertRequest, BlogPostUpsertRequest>, IBlogPostService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BlogPostService(
            StartupbaDbContext context,
            IMapper mapper,
            IHttpContextAccessor httpContextAccessor) : base(context, mapper)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private IQueryable<BlogPost> BaseQuery => _context.BlogPosts
            .Include(bp => bp.Author)
            .Include(bp => bp.Startup)
            .Include(bp => bp.Comments)
            .Include(bp => bp.BlogPostLikes);

        public override async Task<PagedResult<BlogPostResponse>> GetAsync(BlogPostSearchObject search)
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
            return new PagedResult<BlogPostResponse>
            {
                Items = list.Select(MapToResponse).ToList(),
                TotalCount = totalCount
            };
        }

        protected override IQueryable<BlogPost> ApplyFilter(IQueryable<BlogPost> query, BlogPostSearchObject search)
        {
            if (!string.IsNullOrEmpty(search.Title))
            {
                query = query.Where(bp => bp.Title.Contains(search.Title));
            }

            if (!string.IsNullOrEmpty(search.FTS))
            {
                query = query.Where(bp =>
                    bp.Title.Contains(search.FTS) ||
                    bp.Content.Contains(search.FTS));
            }

            if (search.AuthorId.HasValue)
            {
                query = query.Where(bp => bp.AuthorId == search.AuthorId.Value);
            }

            if (search.StartupId.HasValue)
            {
                query = query.Where(bp => bp.StartupId == search.StartupId.Value);
            }

            if (search.IncludeInactive == true)
            {
                // Admin: show all unless an explicit IsActive filter is provided
                if (search.IsActive.HasValue)
                {
                    query = query.Where(bp => bp.IsActive == search.IsActive.Value);
                }
            }
            else
            {
                // Public default: only active posts
                query = query.Where(bp => bp.IsActive == (search.IsActive ?? true));
            }

            // Newest first
            return query.OrderByDescending(bp => bp.Id);
        }

        public override async Task<BlogPostResponse?> GetByIdAsync(int id)
        {
            var entity = await BaseQuery.FirstOrDefaultAsync(bp => bp.Id == id);
            if (entity == null)
                return null;

            return MapToResponse(entity);
        }

        protected BlogPostResponse MapToResponse(BlogPost entity)
        {
            var response = _mapper.Map<BlogPostResponse>(entity);

            if (entity.Author != null)
            {
                response.AuthorName = $"{entity.Author.FirstName} {entity.Author.LastName}";
            }

            response.StartupName = entity.Startup?.Name;
            response.LikeCount = entity.BlogPostLikes?.Count ?? 0;
            response.CommentCount = entity.Comments?.Count(c => c.IsActive) ?? 0;

            var userIdClaim = _httpContextAccessor.HttpContext?.User
                ?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                response.IsLiked = entity.BlogPostLikes?.Any(l => l.UserId == userId) ?? false;
            }

            return response;
        }

        protected override async Task BeforeInsert(BlogPost entity, BlogPostUpsertRequest request)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == request.AuthorId))
            {
                throw new InvalidOperationException("Author does not exist.");
            }

            if (request.StartupId.HasValue && !await _context.Startups.AnyAsync(s => s.Id == request.StartupId.Value))
            {
                throw new InvalidOperationException("Startup does not exist.");
            }
        }

        protected override async Task BeforeUpdate(BlogPost entity, BlogPostUpsertRequest request)
        {
            if (!await _context.Users.AnyAsync(u => u.Id == request.AuthorId))
            {
                throw new InvalidOperationException("Author does not exist.");
            }

            if (request.StartupId.HasValue && !await _context.Startups.AnyAsync(s => s.Id == request.StartupId.Value))
            {
                throw new InvalidOperationException("Startup does not exist.");
            }
        }

        protected override void MapUpdateToEntity(BlogPost entity, BlogPostUpsertRequest request)
        {
            base.MapUpdateToEntity(entity, request);
            entity.UpdatedAt = DateTime.Now;
        }

        public async Task<bool> LikeAsync(int blogPostId, int userId)
        {
            if (!await _context.BlogPosts.AnyAsync(bp => bp.Id == blogPostId))
            {
                throw new InvalidOperationException("Blog post does not exist.");
            }

            if (!await _context.Users.AnyAsync(u => u.Id == userId))
            {
                throw new InvalidOperationException("User does not exist.");
            }

            if (await _context.BlogPostLikes.AnyAsync(l => l.BlogPostId == blogPostId && l.UserId == userId))
            {
                return false; // already liked
            }

            _context.BlogPostLikes.Add(new BlogPostLike
            {
                BlogPostId = blogPostId,
                UserId = userId,
                CreatedAt = DateTime.Now
            });
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlikeAsync(int blogPostId, int userId)
        {
            var like = await _context.BlogPostLikes
                .FirstOrDefaultAsync(l => l.BlogPostId == blogPostId && l.UserId == userId);
            if (like == null)
                return false;

            _context.BlogPostLikes.Remove(like);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
