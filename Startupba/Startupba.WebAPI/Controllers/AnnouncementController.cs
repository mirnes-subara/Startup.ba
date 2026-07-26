using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Startupba.WebAPI.Controllers
{
    public class AnnouncementController : BaseCRUDController<AnnouncementResponse, AnnouncementSearchObject, AnnouncementUpsertRequest, AnnouncementUpsertRequest>
    {
        public AnnouncementController(IAnnouncementService service) : base(service)
        {
        }

        [AllowAnonymous]
        public override async Task<PagedResult<AnnouncementResponse>> Get([FromQuery] AnnouncementSearchObject? search = null)
        {
            return await base.Get(search);
        }

        [AllowAnonymous]
        public override async Task<AnnouncementResponse?> GetById(int id)
        {
            return await base.GetById(id);
        }

        [Authorize(Roles = "Administrator")]
        public override async Task<AnnouncementResponse> Create([FromBody] AnnouncementUpsertRequest request)
        {
            return await base.Create(request);
        }

        [Authorize(Roles = "Administrator")]
        public override async Task<AnnouncementResponse?> Update(int id, [FromBody] AnnouncementUpsertRequest request)
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
