using Startupba.Services.Database;
using System.Collections.Generic;
using System.Threading.Tasks;
using Startupba.Model.Responses;
using Startupba.Model.Requests;
using Startupba.Model.SearchObjects;
using Startupba.Services.Services;

namespace Startupba.Services.Interfaces
{
    public interface IUserService : IService<UserResponse, UserSearchObject>
    {
        Task<UserResponse?> AuthenticateAsync(UserLoginRequest request);
        Task<UserResponse> CreateAsync(UserUpsertRequest request);
        Task<UserResponse?> UpdateAsync(int id, UserUpsertRequest request);
        Task<bool> DeleteAsync(int id);
        Task<UserResponse?> VerifyAsync(int id);
    }
}