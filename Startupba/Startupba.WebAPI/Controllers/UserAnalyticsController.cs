using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startupba.Services.Interfaces;
using System.Security.Claims;

namespace Startupba.WebAPI.Controllers
{
    /// <summary>
    /// Profile activity report for a single user (mobile app).
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class UserAnalyticsController : ControllerBase
    {
        private readonly IUserAnalyticsService _userAnalyticsService;

        public UserAnalyticsController(IUserAnalyticsService userAnalyticsService)
        {
            _userAnalyticsService = userAnalyticsService;
        }

        /// <summary>
        /// Gets analytics for the authenticated user (mobile "my analytics").
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetMyAnalytics()
        {
            var claimId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(claimId, out var userId))
                return Unauthorized();

            var analytics = await _userAnalyticsService.GetUserAnalyticsAsync(userId);
            if (analytics == null)
                return NotFound();
            return Ok(analytics);
        }

        /// <summary>
        /// Gets analytics for a specific user. Administrator only.
        /// </summary>
        [HttpGet("{userId}")]
        [Authorize(Roles = "Administrator")]
        public async Task<IActionResult> GetUserAnalytics(int userId)
        {
            var analytics = await _userAnalyticsService.GetUserAnalyticsAsync(userId);
            if (analytics == null)
                return NotFound();
            return Ok(analytics);
        }
    }
}
