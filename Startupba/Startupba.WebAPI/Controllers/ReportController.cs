using Startupba.Model;
using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;
using Startupba.WebAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Startupba.WebAPI.Controllers
{
    public class ReportController : BaseCRUDController<ReportResponse, ReportSearchObject, ReportUpsertRequest, ReportUpsertRequest>
    {
        public ReportController(IReportService service) : base(service)
        {
        }

        [HttpPost]
        public override async Task<ReportResponse> Create([FromBody] ReportUpsertRequest request)
        {
            request.ReporterId = this.RequireUserId();
            return await _crudService.CreateAsync(request);
        }

        [HttpDelete("{id}")]
        public override Task<bool> Delete(int id)
        {
            throw new UserException("Reports cannot be deleted.");
        }

        /// <summary>
        /// Admin resolves a pending report (1=Reviewed, 2=Dismissed, 3=ActionTaken).
        /// Status and AdminNote are only set here, not via generic update.
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
