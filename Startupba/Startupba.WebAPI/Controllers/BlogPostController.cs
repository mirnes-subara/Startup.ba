using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Startupba.WebAPI.Controllers
{
    public class BlogPostController : BaseCRUDController<BlogPostResponse, BlogPostSearchObject, BlogPostUpsertRequest, BlogPostUpsertRequest>
    {
        public BlogPostController(IBlogPostService service) : base(service)
        {
        }

        [AllowAnonymous]
        public override async Task<PagedResult<BlogPostResponse>> Get([FromQuery] BlogPostSearchObject? search = null)
        {
            return await base.Get(search);
        }

        [AllowAnonymous]
        public override async Task<BlogPostResponse?> GetById(int id)
        {
            return await base.GetById(id);
        }

        /// <summary>
        /// Like a blog post. Returns false if already liked.
        /// </summary>
        [HttpPost("{id}/like")]
        public async Task<ActionResult<bool>> Like(int id, [FromQuery] int userId)
        {
            return Ok(await ((IBlogPostService)_service).LikeAsync(id, userId));
        }

        /// <summary>
        /// Remove a like from a blog post.
        /// </summary>
        [HttpDelete("{id}/like")]
        public async Task<ActionResult<bool>> Unlike(int id, [FromQuery] int userId)
        {
            return Ok(await ((IBlogPostService)_service).UnlikeAsync(id, userId));
        }
    }
}
