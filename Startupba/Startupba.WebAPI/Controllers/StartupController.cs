using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;
using Startupba.WebAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Startupba.WebAPI.Controllers
{
    public class StartupController : BaseCRUDController<StartupResponse, StartupSearchObject, StartupUpsertRequest, StartupUpsertRequest>
    {
        public StartupController(IStartupService service) : base(service)
        {
        }

        private IStartupService StartupService => (IStartupService)_service;

        [HttpPost]
        public override async Task<StartupResponse> Create([FromBody] StartupUpsertRequest request)
        {
            request.FounderId = this.RequireUserId();
            return await _crudService.CreateAsync(request);
        }

        [HttpPut("{id}")]
        public override async Task<StartupResponse?> Update(int id, [FromBody] StartupUpsertRequest request)
        {
            // FounderId is enforced from the existing entity in the service
            return await _crudService.UpdateAsync(id, request);
        }

        /// <summary>
        /// Soft-deletes a startup. Only the founder may delete their own startup.
        /// </summary>
        [HttpDelete("{id}")]
        public override async Task<bool> Delete(int id)
        {
            return await StartupService.DeleteOwnedAsync(id, this.RequireUserId());
        }

        /// <summary>
        /// Content-based recommendations: startups from categories the user
        /// liked, favorited or donated to. User id is taken from the JWT.
        /// </summary>
        [HttpGet("recommended")]
        public async Task<ActionResult<List<StartupResponse>>> GetRecommendedStartups([FromQuery] int count = 5)
        {
            var startups = await StartupService.GetRecommendedStartupsAsync(this.RequireUserId(), count);
            return Ok(startups);
        }

        // ---------- Admin moderation actions ----------

        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<StartupResponse>> Approve(int id)
        {
            var result = await StartupService.ApproveAsync(id);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<StartupResponse>> Reject(int id, [FromBody] StartupRejectRequest request)
        {
            var result = await StartupService.RejectAsync(id, request);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPut("{id}/pause")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<StartupResponse>> Pause(int id)
        {
            var result = await StartupService.PauseAsync(id);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPut("{id}/resume")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<StartupResponse>> Resume(int id)
        {
            var result = await StartupService.ResumeAsync(id);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        // ---------- Likes / favorites ----------

        [HttpPost("{id}/like")]
        public async Task<ActionResult<bool>> Like(int id)
        {
            return Ok(await StartupService.LikeAsync(id, this.RequireUserId()));
        }

        [HttpDelete("{id}/like")]
        public async Task<ActionResult<bool>> Unlike(int id)
        {
            return Ok(await StartupService.UnlikeAsync(id, this.RequireUserId()));
        }

        [HttpPost("{id}/favorite")]
        public async Task<ActionResult<bool>> AddFavorite(int id)
        {
            return Ok(await StartupService.AddFavoriteAsync(id, this.RequireUserId()));
        }

        [HttpDelete("{id}/favorite")]
        public async Task<ActionResult<bool>> RemoveFavorite(int id)
        {
            return Ok(await StartupService.RemoveFavoriteAsync(id, this.RequireUserId()));
        }

        /// <summary>
        /// Records a detail-page view for the current user (deduped, 15-minute throttle).
        /// Also recorded automatically on GET /Startup/{id}.
        /// </summary>
        [HttpPost("{id}/view")]
        public async Task<ActionResult<bool>> RecordView(int id)
        {
            return Ok(await StartupService.RecordViewAsync(id, this.RequireUserId()));
        }
    }
}
