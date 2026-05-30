using Farmacontrol.Core.Repository;
using Farmacontrol.Core.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Farmacontrol.Core.DependencyInjection
{
    public static class ServiceConfiguration
    {
        public static IServiceCollection AddFarmacontrolCore(
            this IServiceCollection services)
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Farmacontrol");

            Directory.CreateDirectory(appDataPath);

            var dbPath = Path.Combine(appDataPath, "farmacontrol.db");
            var connectionString = $"Data Source={dbPath}";

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite(connectionString));

            services.AddSingleton<UserSession>();
            services.AddSingleton<FileLogger>();
            services.AddTransient<AuditService>();

            services.AddTransient<UserService>();
            services.AddTransient<InventoryService>();
            services.AddTransient<SupplierService>();
            services.AddTransient<HistoryService>();
            services.AddTransient<SalesService>();

            return services;
        }
    }
}