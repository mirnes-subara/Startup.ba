using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;

namespace Startupba.Services.Interfaces
{
    public interface IStartupImageService : ICRUDService<StartupImageResponse, StartupImageSearchObject, StartupImageUpsertRequest, StartupImageUpsertRequest>
    {
    }
}
