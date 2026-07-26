using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Startupba.Services.Interfaces
{
    public interface IStartupService : ICRUDService<StartupResponse, StartupSearchObject, StartupUpsertRequest, StartupUpsertRequest>
    {
        // Admin moderation actions
        Task<StartupResponse?> ApproveAsync(int id);
        Task<StartupResponse?> RejectAsync(int id, StartupRejectRequest request);
        Task<StartupResponse?> PauseAsync(int id);
        Task<StartupResponse?> ResumeAsync(int id);

        // Likes / favorites
        Task<bool> LikeAsync(int startupId, int userId);
        Task<bool> UnlikeAsync(int startupId, int userId);
        Task<bool> AddFavoriteAsync(int startupId, int userId);
        Task<bool> RemoveFavoriteAsync(int startupId, int userId);

        // Content-based recommendations
        Task<List<StartupResponse>> GetRecommendedStartupsAsync(int userId, int count);
    }
}
