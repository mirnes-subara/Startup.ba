using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using System.Threading.Tasks;

namespace Startupba.Services.Interfaces
{
    public interface IStartupImageService : ICRUDService<StartupImageResponse, StartupImageSearchObject, StartupImageUpsertRequest, StartupImageUpsertRequest>
    {
        Task<(byte[] Data, string ContentType)?> GetFileAsync(int id);
    }
}
