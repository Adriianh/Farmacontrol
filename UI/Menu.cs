using Farmacontrol.Model;
using Farmacontrol.Services;
using Farmacontrol.Services.Persistence;
using Farmacontrol.UI.Component;
using Farmacontrol.UI.Helper;
using Farmacontrol.UI.View;
using Farmacontrol.Exception;

namespace Farmacontrol.UI
{
    public class Menu
    {
        private readonly AppDataService _appDataService;
        private readonly AppState _state;

        private User? _actualUser;

        public Menu()
        {
            _appDataService = new AppDataService(new Persistence());
            _state = _appDataService.Load();
        }

        public void Start()
        {
            _actualUser = new LoginComponent(_state.UserManager).Login();

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

            var inventoryView = new InventoryView(_state.Inventory, _state.SupplierManager);
            var salesView = new SalesView(_state.Inventory, _state.Sales, _state.SalesCount);
            var alertsView = new AlertsView(_state.HistoryManager, _state.Inventory);
            var reportsView = new ReportsView(_state.Report);
            var productsView = new ProductsView(_state.Inventory);
            var suppliersView = new SuppliersView(_state.SupplierManager, _state.Inventory);
            var usersView = new UsersView(_state.UserManager, user);

            Dictionary<string, Action> actions = new()
            {
                ["1"] = salesView.RegisterSale,
                ["2"] = inventoryView.ManageInventory,
                ["3"] = productsView.SearchProduct,
                ["4"] = alertsView.ShowTodayAlerts,
                ["5"] = alertsView.ShowHistory,
                ["6"] = reportsView.ShowReportsMenu,
                ["7"] = productsView.ShowExpiredProducts,
                ["8"] = usersView.ManageUsers,
                ["9"] = suppliersView.ManageSuppliers,
                ["10"] = suppliersView.GenerateAllSupplierOrders
            };

            while (running)
            {
                string option = mainMenuComponent.ReadOption(user);

                if (option == "0")
                {
                    try
                    {
                        _appDataService.Save(_state);
                    }
                    catch (PersistenceOperationException ex)
                    {
                        Console.WriteLine($"\n[ERROR CRÍTICO] {ex.Message}");
                        if (ex.InnerException != null)
                            Console.WriteLine($"Detalle: {ex.InnerException.Message}");
                        ConsoleHelper.Pause();
                    }
                    
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