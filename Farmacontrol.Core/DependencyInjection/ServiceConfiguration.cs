using Farmacontrol.Core.Repository;
using Farmacontrol.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Farmacontrol.Core.DependencyInjection
{
    public static class ServiceConfiguration
    {
        public static IServiceCollection AddFarmacontrolCore(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=farmacontrol.db";

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(connectionString));

            services.AddSingleton<UserSession>();
            services.AddSingleton<FileLogger>();
            services.AddTransient<AuditService>();

            services.AddTransient<UserManager>();
            services.AddTransient<Inventory>();
            services.AddTransient<SupplierManager>();
            services.AddTransient<HistoryManager>();
            services.AddTransient<SalesManager>();

            return services;
        }
    }
}