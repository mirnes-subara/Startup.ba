using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Startupba.WebAPI.Controllers
{
    public class RoleController : BaseCRUDController<RoleResponse, RoleSearchObject, RoleUpsertRequest, RoleUpsertRequest>
    {
        public RoleController(IRoleService service) : base(service)
        {
        }

        /// <summary>
        /// Authenticated users may list roles (e.g. registration / admin forms).
        /// Writes are administrator-only.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Administrator")]
        public override async Task<RoleResponse> Create([FromBody] RoleUpsertRequest request)
        {
            return await base.Create(request);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrator")]
        public override async Task<RoleResponse?> Update(int id, [FromBody] RoleUpsertRequest request)
        {
            return await base.Update(id, request);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrator")]
        public override async Task<bool> Delete(int id)
        {
            return await base.Delete(id);
        }
    }
}
