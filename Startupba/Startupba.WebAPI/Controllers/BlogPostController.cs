using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;
using Startupba.WebAPI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Startupba.WebAPI.Controllers
{
    public class BlogPostController : BaseCRUDController<BlogPostResponse, BlogPostSearchObject, BlogPostUpsertRequest, BlogPostUpsertRequest>
    {
        public BlogPostController(IBlogPostService service) : base(service)
        {
        }

        [HttpPost]
        public override async Task<BlogPostResponse> Create([FromBody] BlogPostUpsertRequest request)
        {
            request.AuthorId = this.RequireUserId();
            return await _crudService.CreateAsync(request);
        }

        /// <summary>
        /// Like a blog post. Returns false if already liked.
        /// </summary>
        [HttpPost("{id}/like")]
        public async Task<ActionResult<bool>> Like(int id)
        {
            return Ok(await ((IBlogPostService)_service).LikeAsync(id, this.RequireUserId()));
        }

        /// <summary>
        /// Remove a like from a blog post.
        /// </summary>
        [HttpDelete("{id}/like")]
        public async Task<ActionResult<bool>> Unlike(int id)
        {
            return Ok(await ((IBlogPostService)_service).UnlikeAsync(id, this.RequireUserId()));
        }
    }
}
