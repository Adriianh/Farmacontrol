using Farmacontrol.Repository;
using Farmacontrol.Services;
using Farmacontrol.UI;
using Farmacontrol.UI.View;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Farmacontrol
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(configuration);

            var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=farmacontrol.db";
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

            services.AddTransient<InventoryView>();
            services.AddTransient<SalesView>();
            services.AddTransient<AlertsView>();
            services.AddTransient<ReportsView>();
            services.AddTransient<ProductsView>();
            services.AddTransient<SuppliersView>();
            services.AddTransient<UsersView>();

            services.AddTransient<Menu>();

            var serviceProvider = services.BuildServiceProvider();

            var logger = serviceProvider.GetRequiredService<FileLogger>();
            try
            {
                using (var scope = serviceProvider.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    db.Database.EnsureCreated();
                }
            }
            catch (System.Exception ex)
            {
                logger.LogError("Error crítico al inicializar la base de datos", ex);
                Console.WriteLine("Error crítico: No se pudo establecer la conexión con la base de datos.");
                Console.WriteLine("Consulte 'farmacontrol.log' para obtener más detalles.");
                return;
            }

            var menu = serviceProvider.GetRequiredService<Menu>();
            menu.Start();
        }
    }
}