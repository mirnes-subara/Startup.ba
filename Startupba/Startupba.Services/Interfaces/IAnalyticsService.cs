using System.Threading.Tasks;
using Startupba.Model.Responses;

namespace Startupba.Services.Interfaces
{
    public interface IAnalyticsService
    {
        Task<AnalyticsResponse> GetAnalyticsAsync();
    }
}
