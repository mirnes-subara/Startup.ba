using Microsoft.AspNetCore.Mvc;
using Startupba.Services.Helpers;

namespace Startupba.WebAPI.Helpers
{
    public static class ControllerCurrentUserExtensions
    {
        public static int? GetUserId(this ControllerBase controller)
            => controller.User.GetUserId();

        public static int RequireUserId(this ControllerBase controller)
            => controller.User.RequireUserId();

        public static bool IsAdministrator(this ControllerBase controller)
            => controller.User.IsAdministrator();
    }
}
