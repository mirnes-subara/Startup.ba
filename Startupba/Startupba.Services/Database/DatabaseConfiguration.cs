using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Startupba.Services.Database
{
    public static class DatabaseConfiguration
    {
        public static void AddDatabaseServices(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<StartupbaDbContext>(options =>
                options.UseSqlServer(connectionString));
        }

        public static void AddDatabaseStartupba(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<StartupbaDbContext>(options =>
                options.UseSqlServer(connectionString));
        }
    }
}