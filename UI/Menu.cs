using Farmacontrol.Model;
using Farmacontrol.Services;
using Farmacontrol.UI.Component;
using Farmacontrol.UI.Helper;
using Farmacontrol.UI.View;

namespace Farmacontrol.UI
{
    public class Menu(
        UserManager userManager,
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
                ["10"] = suppliersView.GenerateAllSupplierOrders
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
                    action();
                else
                {
                    Console.WriteLine("Opción inválida.");
                    ConsoleHelper.Pause();
                }
            }
        }
    }
}