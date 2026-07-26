using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Startupba.WebAPI.Controllers
{
    public class PlatformSettingController : BaseCRUDController<PlatformSettingResponse, PlatformSettingSearchObject, PlatformSettingUpsertRequest, PlatformSettingUpsertRequest>
    {
        public PlatformSettingController(IPlatformSettingService service) : base(service)
        {
        }

        [AllowAnonymous]
        public override async Task<PagedResult<PlatformSettingResponse>> Get([FromQuery] PlatformSettingSearchObject? search = null)
        {
            return await base.Get(search);
        }

        /// <summary>
        /// Gets a setting by its key (e.g. "PlatformFeePercent", "TermsOfUse").
        /// </summary>
        [HttpGet("by-key/{key}")]
        [AllowAnonymous]
        public async Task<ActionResult<PlatformSettingResponse>> GetByKey(string key)
        {
            var result = await ((IPlatformSettingService)_service).GetByKeyAsync(key);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [Authorize(Roles = "Administrator")]
        public override async Task<PlatformSettingResponse> Create([FromBody] PlatformSettingUpsertRequest request)
        {
            return await base.Create(request);
        }

        [Authorize(Roles = "Administrator")]
        public override async Task<PlatformSettingResponse?> Update(int id, [FromBody] PlatformSettingUpsertRequest request)
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
