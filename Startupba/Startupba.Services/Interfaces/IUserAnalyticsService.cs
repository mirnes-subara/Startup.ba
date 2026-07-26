using System.Threading.Tasks;
using Startupba.Model.Responses;

namespace Startupba.Services.Interfaces
{
    public interface IUserAnalyticsService
    {
        Task<UserAnalyticsResponse?> GetUserAnalyticsAsync(int userId);
    }
}
