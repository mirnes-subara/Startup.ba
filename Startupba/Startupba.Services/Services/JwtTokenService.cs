using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Startupba.Model.Responses;
using Startupba.Services.Database;
using Startupba.Services.Interfaces;

namespace Startupba.Services.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly StartupbaDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IUserService _userService;

        public JwtTokenService(
            StartupbaDbContext context,
            IConfiguration configuration,
            IUserService userService)
        {
            _context = context;
            _configuration = configuration;
            _userService = userService;
        }

        public string CreateAccessToken(UserResponse user, out DateTime expiresAt)
        {
            var key = GetSigningKey();
            var issuer = GetConfig("JWT__ISSUER", "JWT:ISSUER", "Startupba");
            var audience = GetConfig("JWT__AUDIENCE", "JWT:AUDIENCE", "Startupba");
            var minutes = int.TryParse(
                GetConfig("JWT__ACCESS_TOKEN_MINUTES", "JWT:ACCESS_TOKEN_MINUTES", "60"),
                out var m) ? m : 60;

            expiresAt = DateTime.UtcNow.AddMinutes(minutes);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Username),
                new(ClaimTypes.GivenName, user.FirstName ?? string.Empty),
                new(ClaimTypes.Surname, user.LastName ?? string.Empty),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
            };

            if (user.Roles != null)
            {
                foreach (var role in user.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Name));
                }
            }

            var credentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> CreateRefreshTokenAsync(int userId)
        {
            var days = int.TryParse(
                GetConfig("JWT__REFRESH_TOKEN_DAYS", "JWT:REFRESH_TOKEN_DAYS", "7"),
                out var d) ? d : 7;

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            _context.RefreshTokens.Add(new RefreshToken
            {
                UserId = userId,
                Token = token,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(days),
            });
            await _context.SaveChangesAsync();
            return token;
        }

        public async Task<LoginResponse?> IssueLoginResponseAsync(UserResponse user)
        {
            var accessToken = CreateAccessToken(user, out var expiresAt);
            var refreshToken = await CreateRefreshTokenAsync(user.Id);
            return new LoginResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                User = user,
            };
        }

        public async Task<LoginResponse?> RotateRefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return null;

            var existing = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken);

            if (existing == null || existing.RevokedAt != null || existing.ExpiresAt <= DateTime.UtcNow)
                return null;

            existing.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var user = await _userService.GetByIdAsync(existing.UserId);
            if (user == null || !user.IsActive)
                return null;

            return await IssueLoginResponseAsync(user);
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return false;

            var existing = await _context.RefreshTokens
                .FirstOrDefaultAsync(t => t.Token == refreshToken);

            if (existing == null || existing.RevokedAt != null)
                return false;

            existing.RevokedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task RevokeAllForUserAsync(int userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(t => t.UserId == userId && t.RevokedAt == null)
                .ToListAsync();

            if (tokens.Count == 0)
                return;

            var now = DateTime.UtcNow;
            foreach (var token in tokens)
            {
                token.RevokedAt = now;
            }

            await _context.SaveChangesAsync();
        }

        private string GetSigningKey()
        {
            var key = GetConfig("JWT__KEY", "JWT:KEY", null);
            if (string.IsNullOrWhiteSpace(key) || key.Length < 32)
            {
                throw new InvalidOperationException(
                    "JWT signing key is missing or too short. Set JWT__KEY (min 32 characters).");
            }
            return key;
        }

        private string GetConfig(string envName, string configKey, string? defaultValue)
        {
            return Environment.GetEnvironmentVariable(envName)
                ?? _configuration[configKey]
                ?? defaultValue
                ?? string.Empty;
        }
    }
}
