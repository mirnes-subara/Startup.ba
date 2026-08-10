using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;

namespace Startupba.WebAPI.Controllers
{
    public class RoleController : BaseCRUDController<RoleResponse, RoleSearchObject, RoleUpsertRequest, RoleUpsertRequest>
    {
        public RoleController(IRoleService service) : base(service)
        {
        }
    }
}
