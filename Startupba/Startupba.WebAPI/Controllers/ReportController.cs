using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Startupba.WebAPI.Controllers
{
    public class ReportController : BaseCRUDController<ReportResponse, ReportSearchObject, ReportUpsertRequest, ReportUpsertRequest>
    {
        public ReportController(IReportService service) : base(service)
        {
        }

        /// <summary>
        /// Admin resolves a pending report (1=Reviewed, 2=Dismissed, 3=ActionTaken).
        /// The reporter gets an in-app notification about the outcome.
        /// </summary>
        [HttpPut("{id}/resolve")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<ReportResponse>> Resolve(int id, [FromBody] ReportResolveRequest request)
        {
            var result = await ((IReportService)_service).ResolveAsync(id, request);
            if (result == null)
                return NotFound();
            return Ok(result);
        }
    }
}
