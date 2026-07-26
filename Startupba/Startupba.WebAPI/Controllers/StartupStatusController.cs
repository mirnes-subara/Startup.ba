using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Startupba.WebAPI.Controllers
{
    public class StartupStatusController : BaseCRUDController<StartupStatusResponse, StartupStatusSearchObject, StartupStatusUpsertRequest, StartupStatusUpsertRequest>
    {
        public StartupStatusController(IStartupStatusService service) : base(service)
        {
        }

        [AllowAnonymous]
        public override async Task<PagedResult<StartupStatusResponse>> Get([FromQuery] StartupStatusSearchObject? search = null)
        {
            return await base.Get(search);
        }

        [AllowAnonymous]
        public override async Task<StartupStatusResponse?> GetById(int id)
        {
            return await base.GetById(id);
        }

        [Authorize(Roles = "Administrator")]
        public override async Task<StartupStatusResponse> Create([FromBody] StartupStatusUpsertRequest request)
        {
            return await base.Create(request);
        }

        [Authorize(Roles = "Administrator")]
        public override async Task<StartupStatusResponse?> Update(int id, [FromBody] StartupStatusUpsertRequest request)
        {
            return await base.Update(id, request);
        }

        [Authorize(Roles = "Administrator")]
        public override async Task<bool> Delete(int id)
        {
            return await base.Delete(id);
        }
    }
}
