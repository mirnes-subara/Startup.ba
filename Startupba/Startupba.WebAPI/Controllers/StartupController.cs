using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Startupba.WebAPI.Controllers
{
    public class StartupController : BaseCRUDController<StartupResponse, StartupSearchObject, StartupUpsertRequest, StartupUpsertRequest>
    {
        public StartupController(IStartupService service) : base(service)
        {
        }

        private IStartupService StartupService => (IStartupService)_service;

        [AllowAnonymous]
        public override async Task<PagedResult<StartupResponse>> Get([FromQuery] StartupSearchObject? search = null)
        {
            return await base.Get(search);
        }

        [AllowAnonymous]
        public override async Task<StartupResponse?> GetById(int id)
        {
            return await base.GetById(id);
        }

        /// <summary>
        /// Soft-deletes a startup. Only the founder may delete their own startup.
        /// </summary>
        [HttpDelete("{id}")]
        public override async Task<bool> Delete(int id)
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimId, out var userId))
            {
                return false;
            }

            return await StartupService.DeleteOwnedAsync(id, userId);
        }

        /// <summary>
        /// Content-based recommendations: startups from categories the user
        /// liked, favorited or donated to. User id is taken from the JWT.
        /// </summary>
        [HttpGet("recommended")]
        public async Task<ActionResult<List<StartupResponse>>> GetRecommendedStartups([FromQuery] int count = 5)
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimId, out var userId))
                return Unauthorized();

            var startups = await StartupService.GetRecommendedStartupsAsync(userId, count);
            return Ok(startups);
        }

        // ---------- Admin moderation actions ----------

        /// <summary>
        /// Admin approves a pending startup (founder gets an in-app notification and an email).
        /// </summary>
        [HttpPut("{id}/approve")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<StartupResponse>> Approve(int id)
        {
            var result = await StartupService.ApproveAsync(id);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Admin rejects a pending startup with a reason (founder gets an in-app notification and an email).
        /// </summary>
        [HttpPut("{id}/reject")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<StartupResponse>> Reject(int id, [FromBody] StartupRejectRequest request)
        {
            var result = await StartupService.RejectAsync(id, request);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Admin pauses an approved startup.
        /// </summary>
        [HttpPut("{id}/pause")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<StartupResponse>> Pause(int id)
        {
            var result = await StartupService.PauseAsync(id);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Admin resumes a paused startup.
        /// </summary>
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

        /// <summary>
        /// Like (recommend) a startup. Returns false if already liked.
        /// </summary>
        [HttpPost("{id}/like")]
        public async Task<ActionResult<bool>> Like(int id)
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimId, out var userId))
                return Unauthorized();
            return Ok(await StartupService.LikeAsync(id, userId));
        }

        /// <summary>
        /// Remove a like from a startup.
        /// </summary>
        [HttpDelete("{id}/like")]
        public async Task<ActionResult<bool>> Unlike(int id)
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimId, out var userId))
                return Unauthorized();
            return Ok(await StartupService.UnlikeAsync(id, userId));
        }

        /// <summary>
        /// Add a startup to the user's favorites. Returns false if already favorited.
        /// </summary>
        [HttpPost("{id}/favorite")]
        public async Task<ActionResult<bool>> AddFavorite(int id)
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimId, out var userId))
                return Unauthorized();
            return Ok(await StartupService.AddFavoriteAsync(id, userId));
        }

        /// <summary>
        /// Remove a startup from the user's favorites.
        /// </summary>
        [HttpDelete("{id}/favorite")]
        public async Task<ActionResult<bool>> RemoveFavorite(int id)
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimId, out var userId))
                return Unauthorized();
            return Ok(await StartupService.RemoveFavoriteAsync(id, userId));
        }
    }
}
