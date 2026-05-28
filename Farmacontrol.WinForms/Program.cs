using Farmacontrol.DependencyInjection;
using Farmacontrol.Repository;
using Farmacontrol.Services;
using Farmacontrol.WinForms.Forms;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Farmacontrol.WinForms
{
    internal static class Program
    {
        public static IServiceProvider Services { get; private set; } = null!;

        [STAThread]
        private static void Main()
        {
            ApplicationConfiguration.Initialize();

            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            var services = new ServiceCollection();

            services.AddSingleton<IConfiguration>(configuration);
            services.AddFarmacontrolCore(configuration);

            services.AddTransient<LoginForm>();
            services.AddTransient<MainForm>();

            Services = services.BuildServiceProvider();

            var logger = Services.GetRequiredService<FileLogger>();

            try
            {
                using var scope = Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.EnsureCreated();
            }
            catch (System.Exception ex)
            {
                logger.LogError("Error crítico al inicializar la base de datos desde WinForms", ex);

                MessageBox.Show(
                    "No se pudo inicializar la base de datos. Revise el archivo farmacontrol.log.",
                    "Error crítico",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);

                return;
            }

            Application.Run(Services.GetRequiredService<LoginForm>());
        }
    }
}