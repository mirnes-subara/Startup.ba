using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;

namespace Startupba.WebAPI.Controllers
{
    public class StartupImageController : BaseCRUDController<StartupImageResponse, StartupImageSearchObject, StartupImageUpsertRequest, StartupImageUpsertRequest>
    {
        public StartupImageController(IStartupImageService service) : base(service)
        {
        }
    }
}
