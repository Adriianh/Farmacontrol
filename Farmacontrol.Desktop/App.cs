using Avalonia.Styling;
using Farmacontrol.Core.DependencyInjection;
using Farmacontrol.Core.Repository;
using Farmacontrol.Desktop.States;
using Farmacontrol.Desktop.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Farmacontrol.Desktop;

public static class Program
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    [STAThread]
    public static async Task Main(string[] args)
    {
        var serviceProvider = ConfigureServices();
        ServiceProvider = serviceProvider;

        await ApplyMigrationsAsync(serviceProvider);
        BuildApp(args, serviceProvider);
    }

    private static ServiceProvider ConfigureServices()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        return new ServiceCollection()
            .AddSingleton<IConfiguration>(configuration)
            .AddFarmacontrolCore()
            .AddTransient<InventoryState>()
            .AddTransient<SupplierState>()
            .AddTransient<SearchProductState>()
            .AddTransient<PendingOrdersState>()
            .AddTransient<SaleState>()
            .AddTransient<VoidSaleState>()
            .AddTransient<SalesReportState>()
            .AddTransient<SalesHistoryState>()
            .AddTransient<AlertsState>()
            .AddTransient<AlertsHistoryState>()
            .AddTransient<UsersState>()
            .AddTransient<LoginState>()
            .AddSingleton<RootState>()
            .AddSingleton<MainView.State>()
            .AddTransient<DashboardState>()
            .AddSingleton<ProductState>()
            .BuildServiceProvider();
    }

    private static async Task ApplyMigrationsAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
    }

    private static void BuildApp(string[] args, IServiceProvider serviceProvider)
    {
        var lifetime = new ClassicDesktopStyleApplicationLifetime
        {
            Args = args,
            ShutdownMode = ShutdownMode.OnLastWindowClose
        };

        AppBuilder.Configure<Application>()
            .UsePlatformDetect()
            .LogToTrace()
            .AfterSetup(builder =>
            {
                builder.Instance!.RequestedThemeVariant = ThemeVariant.Light;
                builder.Instance.Styles.Add(new FluentTheme());
            })
            .UseServiceProvider(serviceProvider)
            .UseComponentControlFactory(type =>
                (Control)ActivatorUtilities.CreateInstance(serviceProvider, type))
            .UseViewInitializationStrategy(ViewInitializationStrategy.Lazy)
            .SetupWithLifetime(lifetime);

        lifetime.MainWindow = new Window()
            .Title("Farmacontrol")
            .Width(1200)
            .Height(800)
            .MinWidth(1000)
            .MinHeight(600)
            .Content(ViewFactory.Create<RootView>());

        lifetime.Start(args);
    }
}