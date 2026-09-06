using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Startupba.Model;

namespace Startupba.Services.Helpers
{
    /// <summary>
    /// Reads the authenticated user id and admin role from JWT claims.
    /// Prefer this over trusting owner ids from request bodies.
    /// </summary>
    public static class CurrentUserHelper
    {
        public const string AdministratorRole = "Administrator";

        public static int? GetUserId(this ClaimsPrincipal? user)
        {
            var claim = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }

        public static int RequireUserId(this ClaimsPrincipal? user)
        {
            var id = user.GetUserId();
            if (!id.HasValue)
                throw new UserException("You must be signed in.");
            return id.Value;
        }

        public static bool IsAdministrator(this ClaimsPrincipal? user)
        {
            return user?.IsInRole(AdministratorRole) == true;
        }

        public static int? GetUserId(this IHttpContextAccessor accessor)
        {
            return accessor.HttpContext?.User.GetUserId();
        }

        public static int RequireUserId(this IHttpContextAccessor accessor)
        {
            return accessor.HttpContext?.User.RequireUserId()
                ?? throw new UserException("You must be signed in.");
        }

        public static bool IsAdministrator(this IHttpContextAccessor accessor)
        {
            return accessor.HttpContext?.User.IsAdministrator() == true;
        }

        /// <summary>
        /// Throws if the caller is neither the owner nor an administrator.
        /// </summary>
        public static void EnsureOwnerOrAdmin(this IHttpContextAccessor accessor, int ownerId, string message = "You are not allowed to modify this resource.")
        {
            var userId = accessor.RequireUserId();
            if (accessor.IsAdministrator() || userId == ownerId)
                return;

            throw new UserException(message);
        }
    }
}
