using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

using MapsterMapper;
using Startupba.Model;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Database;
using Startupba.Services.Interfaces;
using Startupba.Model.Requests;
using Startupba.Services.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;


namespace Startupba.Services.Services
{
    public class UserService : BaseService<UserResponse, UserSearchObject, User>, IUserService
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<UserService> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(
            StartupbaDbContext context,
            IMapper mapper,
            INotificationService notificationService,
            ILogger<UserService> logger,
            IHttpContextAccessor httpContextAccessor) : base(context, mapper)
        {
            _notificationService = notificationService;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public override async Task<PagedResult<UserResponse>> GetAsync(UserSearchObject search)
        {
            var query = _context.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search.Username))
            {
                query = query.Where(u => u.Username.Contains(search.Username));
            }

            if (!string.IsNullOrEmpty(search.Email))
            {
                query = query.Where(u => u.Email.Contains(search.Email));
            }

            if (!string.IsNullOrEmpty(search.FTS))
            {
                query = query.Where(u =>
                    u.FirstName.Contains(search.FTS) ||
                    u.LastName.Contains(search.FTS) ||
                    u.Username.Contains(search.FTS) ||
                    u.Email.Contains(search.FTS));
            }

            if (search.GenderId.HasValue)
            {
                query = query.Where(u => u.GenderId == search.GenderId.Value);
            }

            if (search.CityId.HasValue)
            {
                query = query.Where(u => u.CityId == search.CityId.Value);
            }

            if (search.RoleId.HasValue)
            {
                query = query.Where(u => u.UserRoles.Any(ur => ur.RoleId == search.RoleId.Value));
            }

            query = query
                .Include(u => u.Gender)
                .Include(u => u.City)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role);

            int? totalCount = null;
            if (search.IncludeTotalCount)
            {
                totalCount = await query.CountAsync();
            }

            query = ApplyPaging(query, search);

            var users = await query.ToListAsync();
            return new PagedResult<UserResponse>
            {
                Items = users.Select(MapToResponse).ToList(),
                TotalCount = totalCount
            };
        }

        public override async Task<UserResponse?> GetByIdAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.Gender)
                .Include(u => u.City)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return null;

            return MapToResponse(user);
        }


        public async Task<UserResponse> CreateAsync(UserUpsertRequest request)
        {
            // Check if user with same email or username already exists
            if (await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                throw new InvalidOperationException("User with this email already exists.");
            }

            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                throw new InvalidOperationException("User with this username already exists.");
            }

            var user = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Username = request.Username,
                PhoneNumber = request.PhoneNumber,
                GenderId = request.GenderId,
                CityId = request.CityId,
                IsActive = request.IsActive,
                CreatedAt = DateTime.UtcNow,
                Picture = request.Picture
            };

            // Hash password if provided
            if (!string.IsNullOrEmpty(request.Password))
            {
                user.PasswordSalt = PasswordGenerator.GenerateSalt();
                user.PasswordHash = PasswordGenerator.GenerateHash(request.Password, user.PasswordSalt);
            }

            // Only administrators may assign roles from the request; others always get User (id 2).
            var isAdmin = _httpContextAccessor.HttpContext?.User?.IsInRole("Administrator") == true;
            var roleIdsToAssign = isAdmin && request.RoleIds != null && request.RoleIds.Any()
                ? request.RoleIds
                : new List<int> { 2 };

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                foreach (var roleId in roleIdsToAssign)
                {
                    var userRole = new UserRole
                    {
                        UserId = user.Id,
                        RoleId = roleId,
                        DateAssigned = DateTime.UtcNow
                    };
                    _context.UserRoles.Add(userRole);
                }
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return await GetUserResponseWithRolesAsync(user.Id);
        }

        public async Task<UserResponse?> UpdateAsync(int id, UserUpsertRequest request)
        {
            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
                return null;

            // Check if email is being changed and if it already exists
            if (request.Email != user.Email && await _context.Users.AnyAsync(u => u.Email == request.Email))
            {
                throw new InvalidOperationException("User with this email already exists.");
            }

            // Check if username is being changed and if it already exists
            if (request.Username != user.Username && await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                throw new InvalidOperationException("User with this username already exists.");
            }

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.Email = request.Email;
            user.Username = request.Username;
            user.PhoneNumber = request.PhoneNumber;
            user.GenderId = request.GenderId;
            user.CityId = request.CityId;
            user.IsActive = request.IsActive;
            user.Picture = request.Picture;

            // Only administrators may change roles; ignore RoleIds for everyone else.
            var isAdmin = _httpContextAccessor.HttpContext?.User?.IsInRole("Administrator") == true;
            if (isAdmin && request.RoleIds != null && request.RoleIds.Any())
            {
                _context.UserRoles.RemoveRange(user.UserRoles);

                foreach (var roleId in request.RoleIds)
                {
                    var userRole = new UserRole
                    {
                        UserId = user.Id,
                        RoleId = roleId,
                        DateAssigned = DateTime.UtcNow
                    };
                    _context.UserRoles.Add(userRole);
                }
            }

            await _context.SaveChangesAsync();
            return await GetUserResponseWithRolesAsync(user.Id);
        }

        public async Task ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                throw new UserException("User not found.");
            }

            if (request.NewPassword != request.NewPasswordConfirmation)
            {
                throw new UserException("New password and confirmation do not match.");
            }

            if (!PasswordGenerator.VerifyPassword(request.CurrentPassword, user.PasswordHash, user.PasswordSalt))
            {
                throw new UserException("Current password is incorrect.");
            }

            user.PasswordSalt = PasswordGenerator.GenerateSalt();
            user.PasswordHash = PasswordGenerator.GenerateHash(request.NewPassword, user.PasswordSalt);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }

        protected override UserResponse MapToResponse(User user)
        {
            var response = new UserResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Username = user.Username,
                Picture = user.Picture,
                IsActive = user.IsActive,
                IsVerified = user.IsVerified,
                IsVerificationRequested = user.IsVerificationRequested,
                CreatedAt = user.CreatedAt,
                LastLoginAt = user.LastLoginAt,
                PhoneNumber = user.PhoneNumber,
                GenderId = user.GenderId,
                GenderName = user.Gender?.Name ?? string.Empty,
                CityId = user.CityId,
                CityName = user.City?.Name ?? string.Empty,
                Roles = user.UserRoles?.Select(ur => new RoleResponse
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name,
                    Description = ur.Role.Description
                }).ToList() ?? new List<RoleResponse>()
            };

            return response;
        }

        private async Task<UserResponse> GetUserResponseWithRolesAsync(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Gender)
                .Include(u => u.City)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new InvalidOperationException("User not found.");

            return MapToResponse(user);
        }

        public async Task<UserResponse?> VerifyAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return null;

            user.IsVerified = true;
            user.IsVerificationRequested = false;
            await _context.SaveChangesAsync();

            try
            {
                await _notificationService.CreateNotificationAsync(
                    user.Id,
                    "Profile Verified",
                    "Your profile has been verified. You now have a verified badge.",
                    NotificationTypes.Announcement,
                    user.Id,
                    "User");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification");
            }

            return await GetUserResponseWithRolesAsync(user.Id);
        }

        public async Task<UserResponse?> RequestVerificationAsync(int id)
        {
            var user = await _context.Users
                .Include(u => u.Gender)
                .Include(u => u.City)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
                return null;

            if (user.IsVerified)
            {
                throw new InvalidOperationException("Profile is already verified.");
            }

            // Idempotent: already requested — do not spam admins
            if (user.IsVerificationRequested)
            {
                return MapToResponse(user);
            }

            user.IsVerificationRequested = true;
            await _context.SaveChangesAsync();

            try
            {
                var adminIds = await _context.UserRoles
                    .Where(ur => ur.Role.Name == "Administrator")
                    .Select(ur => ur.UserId)
                    .Distinct()
                    .ToListAsync();

                var displayName = $"{user.FirstName} {user.LastName}".Trim();
                if (string.IsNullOrEmpty(displayName))
                    displayName = user.Username;

                foreach (var adminId in adminIds)
                {
                    await _notificationService.CreateNotificationAsync(
                        adminId,
                        "Verification Requested",
                        $"\"{displayName}\" (@{user.Username}) requested profile verification.",
                        NotificationTypes.VerificationRequested,
                        user.Id,
                        "User");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification");
            }

            return MapToResponse(user);
        }

        public async Task<UserResponse?> AuthenticateAsync(UserLoginRequest request)
        {
            var user = await _context.Users
                .Include(u => u.Gender)
                .Include(u => u.City)
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
                return null;

            // Update last login time
            user.LastLoginAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return MapToResponse(user);
        }

        private bool VerifyPassword(string password, string passwordHash, string passwordSalt)
        {
            return PasswordGenerator.VerifyPassword(password, passwordHash, passwordSalt);
        }
    }
}