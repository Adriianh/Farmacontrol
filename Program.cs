using Farmacontrol.Repository;
using Farmacontrol.Services;
using Farmacontrol.UI;
using Farmacontrol.UI.View;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Farmacontrol
{
    internal static class Program
    {
        private static void Main(string[] args)
        {
            var services = new ServiceCollection();

            var dbPath = Path.Combine(AppContext.BaseDirectory, "farmacontrol.db");
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

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

            using (var scope = serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            }

            var menu = serviceProvider.GetRequiredService<Menu>();
            menu.Start();
        }
    }
}