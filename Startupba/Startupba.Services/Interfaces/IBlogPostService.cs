using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using System.Threading.Tasks;

namespace Startupba.Services.Interfaces
{
    public interface IBlogPostService : ICRUDService<BlogPostResponse, BlogPostSearchObject, BlogPostUpsertRequest, BlogPostUpsertRequest>
    {
        Task<bool> LikeAsync(int blogPostId, int userId);
        Task<bool> UnlikeAsync(int blogPostId, int userId);
    }
}
