using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Startupba.WebAPI.Controllers
{
    public class StartupImageController : BaseCRUDController<StartupImageResponse, StartupImageSearchObject, StartupImageUpsertRequest, StartupImageUpsertRequest>
    {
        public StartupImageController(IStartupImageService service) : base(service)
        {
        }

        [AllowAnonymous]
        public override async Task<PagedResult<StartupImageResponse>> Get([FromQuery] StartupImageSearchObject? search = null)
        {
            return await base.Get(search);
        }

        [AllowAnonymous]
        public override async Task<StartupImageResponse?> GetById(int id)
        {
            return await base.GetById(id);
        }
    }
}
