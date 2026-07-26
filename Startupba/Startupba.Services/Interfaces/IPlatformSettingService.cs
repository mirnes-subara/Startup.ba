using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using System.Threading.Tasks;

namespace Startupba.Services.Interfaces
{
    public interface IPlatformSettingService : ICRUDService<PlatformSettingResponse, PlatformSettingSearchObject, PlatformSettingUpsertRequest, PlatformSettingUpsertRequest>
    {
        Task<PlatformSettingResponse?> GetByKeyAsync(string key);
    }
}
