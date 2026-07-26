using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startupba.Services.Interfaces;

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

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserAnalytics(int userId)
        {
            var analytics = await _userAnalyticsService.GetUserAnalyticsAsync(userId);
            if (analytics == null)
                return NotFound();
            return Ok(analytics);
        }
    }
}
