using Startupba.Model.Responses;

namespace Startupba.Services.Interfaces
{
    public interface IJwtTokenService
    {
        string CreateAccessToken(UserResponse user, out DateTime expiresAt);
        Task<string> CreateRefreshTokenAsync(int userId);
        Task<LoginResponse?> IssueLoginResponseAsync(UserResponse user);
        Task<LoginResponse?> RotateRefreshTokenAsync(string refreshToken);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);
        Task RevokeAllForUserAsync(int userId);
    }
}
