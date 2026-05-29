using Avalonia.Styling;
using Farmacontrol.Desktop.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Farmacontrol.Desktop;

public static class Program
{
    public static Task Main(string[] args)
    {
        try
        {
            var serviceProvider = ConfigureServices();
            BuildApp(args, serviceProvider);
            return Task.CompletedTask;
        }
        catch (System.Exception exception)
        {
            return Task.FromException(exception);
        }
    }

    private static ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        var dbPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Farmacontrol",
            "farmacontrol.db"
        );
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        return services.BuildServiceProvider();
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
            .Content(ViewFactory.Create<MainView>());

        lifetime.Start(args);
    }
}