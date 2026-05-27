using Farmacontrol.Model;
using Farmacontrol.Services;
using Farmacontrol.UI.Component;
using Farmacontrol.UI.Helper;
using Farmacontrol.UI.View;

namespace Farmacontrol.UI
{
    public class Menu(
        UserManager userManager,
        UserSession userSession,
        FileLogger logger,
        InventoryView inventoryView,
        SalesView salesView,
        AlertsView alertsView,
        ReportsView reportsView,
        ProductsView productsView,
        SuppliersView suppliersView,
        UsersView usersView)
    {
        private User? _actualUser;

        public void Start()
        {
            _actualUser = new LoginComponent(userManager).Login();

            if (_actualUser == null)
            {
                Console.WriteLine("Demasiados intentos fallidos. El sistema se cerrará.");
                ConsoleHelper.Pause();
                return;
            }

            userSession.CurrentUser = _actualUser;
            ShowMainMenu(_actualUser);
        }

        private void ShowMainMenu(User user)
        {
            bool running = true;

            var mainMenuComponent = new MainMenuComponent();

            Dictionary<string, Action> actions = new()
            {
                ["1"] = salesView.RegisterSale,
                ["2"] = inventoryView.ManageInventory,
                ["3"] = productsView.SearchProduct,
                ["4"] = alertsView.ShowTodayAlerts,
                ["5"] = alertsView.ShowHistory,
                ["6"] = reportsView.ShowReportsMenu,
                ["7"] = productsView.ShowExpiredProducts,
                ["8"] = () => usersView.ManageUsers(user),
                ["9"] = suppliersView.ManageSuppliers,
                ["10"] = suppliersView.GenerateAllSupplierOrders,
                ["11"] = salesView.VoidSale
            };

            while (running)
            {
                string option = mainMenuComponent.ReadOption(user);

                if (option == "0")
                {
                    running = false;
                    continue;
                }

                if (actions.TryGetValue(option, out var action))
                {
                    try
                    {
                        action();
                    }
                    catch (System.Exception ex)
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
    }
}