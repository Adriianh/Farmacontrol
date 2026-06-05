using Farmacontrol.ConsoleApp.UI.Component;
using Farmacontrol.ConsoleApp.UI.Helper;
using Farmacontrol.ConsoleApp.UI.View;
using Farmacontrol.Core.Model;
using Farmacontrol.Core.Services;

namespace Farmacontrol.ConsoleApp.UI
{
    public class Menu(
        UserService userService,
        UserSession userSession,
        FileLogger logger,
        InventoryView inventoryView,
        SalesView salesView,
        AlertsView alertsView,
        ReportsView reportsView,
        ProductsView productsView,
        SuppliersView suppliersView,
        UsersView usersView,
        SalesService salesService,
        HistoryService historyService)
    {
        private User? _actualUser;

        public void Start()
        {


            _actualUser = new LoginComponent(userService).Login();

            if (_actualUser == null)
            {
                Console.WriteLine("Demasiados intentos fallidos. El sistema se cerrará.");
                ConsoleHelper.Pause();
                return;
            }

            userSession.SetUser(_actualUser);
            ShowMainMenu(_actualUser);
        }

        private void ShowMainMenu(User user)
        {
            bool running = true;

            var mainMenuComponent = new MainMenuComponent();

            Dictionary<string, Action> actions = new()
            {
                ["1"] = reportsView.ShowReportsMenu,
                ["2"] = salesView.RegisterSale,
                ["3"] = salesView.VoidSale,
                ["4"] = reportsView.ShowReportsMenu,
                ["5"] = inventoryView.ManageInventory,
                ["6"] = productsView.SearchProduct,
                ["7"] = suppliersView.GenerateAllSupplierOrders,
                ["8"] = alertsView.ShowTodayAlerts,
                ["9"] = alertsView.ShowHistory,
                ["10"] = () => usersView.ManageUsers(user),
                ["11"] = suppliersView.ManageSuppliers
            };

            while (running)
            {
                Console.Clear();
                ShowDashboard();
                var option = mainMenuComponent.ReadOption(user);

                if (option == "0")
                {
                    running = false;
                    continue;
                }

                var isAllowed = user.GetAllowedActions().Any(a => a.StartsWith(option + "."));
                if (!isAllowed)
                {
                    Console.WriteLine("Opción no permitida para su rol.");
                    ConsoleHelper.Pause();
                    continue;
                }

                if (actions.TryGetValue(option, out var action))
                {
                    try
                    {
                        action();
                    }
                    catch (Exception ex)
                    {
                        logger.LogError($"Excepción no controlada al ejecutar la opción {option}", ex);
                        Console.WriteLine($"\n[ERROR] Ocurrió un error inesperado al procesar la solicitud: {ex.Message}");
                        Console.WriteLine("Los detalles del error se han registrado en 'farmacontrol.log'.");
                        ConsoleHelper.Pause();
                    }
                }
                else
                {
                    Console.WriteLine("Opción inválida.");
                    ConsoleHelper.Pause();
                }
            }
        }
        private void ShowDashboard()
        {
            var today = DateTime.Today;
            var sales = salesService.GetAllSales().Where(s => s.Date.Date == today && !s.IsVoided).ToList();
            
            var todaysSalesCount = sales.Count;
            var todaysSalesTotal = sales.Sum(s => s.Total);
            var activeAlertsCount = historyService.GetHistory().Count(a => a.Date.Date == today);

            Console.WriteLine("=================================================");
            Console.WriteLine("                PANEL PRINCIPAL                  ");
            Console.WriteLine("=================================================");
            Console.WriteLine($" 💰 Ventas de Hoy: {todaysSalesCount}             ");
            Console.WriteLine($" 💵 Ingresos del Día: Q{todaysSalesTotal:F2}      ");
            Console.WriteLine($" ⚠️ Alertas Activas: {activeAlertsCount}          ");
            Console.WriteLine("=================================================\n");
        }
    }
}