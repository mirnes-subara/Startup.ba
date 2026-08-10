using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Startupba.WebAPI.Controllers
{
    public class BlogPostController : BaseCRUDController<BlogPostResponse, BlogPostSearchObject, BlogPostUpsertRequest, BlogPostUpsertRequest>
    {
        public BlogPostController(IBlogPostService service) : base(service)
        {
        }

        /// <summary>
        /// Like a blog post. Returns false if already liked.
        /// </summary>
        [HttpPost("{id}/like")]
        public async Task<ActionResult<bool>> Like(int id)
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimId, out var userId))
                return Unauthorized();
            return Ok(await ((IBlogPostService)_service).LikeAsync(id, userId));
        }

        /// <summary>
        /// Remove a like from a blog post.
        /// </summary>
        [HttpDelete("{id}/like")]
        public async Task<ActionResult<bool>> Unlike(int id)
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimId, out var userId))
                return Unauthorized();
            return Ok(await ((IBlogPostService)_service).UnlikeAsync(id, userId));
        }
    }
}
