using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Startupba.Services.Interfaces;

namespace Startupba.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize(Roles = "Administrator")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAnalytics()
        {
            var analytics = await _analyticsService.GetAnalyticsAsync();
            return Ok(analytics);
        }
    }
}
