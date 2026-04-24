using Farmacontrol.Model;
using Farmacontrol.Services;
using Farmacontrol.Services.Persistence;
using Farmacontrol.UI.Component;
using Farmacontrol.UI.Helper;
using Farmacontrol.UI.View;

namespace Farmacontrol.UI
{
    public class Menu
    {
        private readonly Inventory _inventory = new();
        private readonly List<Sale> _sales;
        private readonly Report _report;
        private readonly Persistence _persistence = new();
        private readonly UserManager _userManager = new();
        private readonly SupplierManager _supplierManager = new();
        private readonly HistoryManager _historyManager = new();

        private User? _actualUser;
        private int _salesCount;

        public Menu()
        {
            List<User> savedUsers = _persistence.LoadUsers();
            if (savedUsers.Count > 0)
                savedUsers.ForEach(user => _userManager.AddUser(user));

            List<Product> savedProducts = _persistence.LoadProducts();
            if (savedProducts.Count > 0)
                savedProducts.ForEach(product => _inventory.AddProduct(product));

            List<Supplier> savedSuppliers = _persistence.LoadSuppliers();
            if (savedSuppliers.Count > 0)
                savedSuppliers.ForEach(supplier => _supplierManager.AddSupplier(supplier));

            List<Alert> savedHistory = _persistence.LoadHistory();
            if (savedHistory.Count > 0)
                _historyManager.LoadHistory(savedHistory);

            _sales = _persistence.LoadSales();
            _report = new Report(_sales);

            _salesCount = _sales.Count > 0
                ? _sales.Max(sale => sale.Code)
                : 0;
        }

        public void Start()
        {
            _actualUser = new LoginComponent(_userManager).Login();

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

            var inventoryView = new InventoryView(_inventory, _supplierManager);
            var alertsView = new AlertsView(_historyManager, _inventory);
            var reportsView = new ReportsView(_report);
            var productsView = new ProductsView(_inventory);
            var suppliersView = new SuppliersView(_supplierManager, _inventory);
            var usersView = new UsersView(_userManager, user);

            while (running)
            {
                string option = mainMenuComponent.ReadOption(user);

                switch (option)
                {
                    case "1":
                        RegisterSale();
                        break;

                    case "2":
                        inventoryView.ManageInventory();
                        break;

                    case "3":
                        productsView.SearchProduct();
                        break;

                    case "4":
                        alertsView.ShowTodayAlerts();
                        break;

                    case "5":
                        alertsView.ShowHistory();
                        break;

                    case "6":
                        reportsView.ShowReportsMenu();
                        break;

                    case "7":
                        productsView.ShowExpiredProducts();
                        break;

                    case "8":
                        usersView.ManageUsers();
                        break;

                    case "9":
                        suppliersView.ManageSuppliers();
                        break;

                    case "10":
                        suppliersView.GenerateAllSupplierOrders();
                        break;

                    case "0":
                        SaveData();
                        running = false;
                        break;

                    default:
                        Console.WriteLine("Opción inválida.");
                        ConsoleHelper.Pause();
                        break;
                }
            }
        }

        private void RegisterSale()
        {
            var salesView = new SalesView(_inventory, _sales, _salesCount);
            salesView.RegisterSale();
            _salesCount = salesView.SalesCounter;
        }

        private void SaveData()
        {
            _persistence.SaveProducts(_inventory.GetProducts.ToList());
            _persistence.SaveSales(_sales);
            _persistence.SaveUsers(_userManager.GetAllUsers().ToList());
            _persistence.SaveSuppliers(_supplierManager.GetSuppliers().ToList());
            _persistence.SaveHistory(_historyManager.GetHistory().ToList());
        }
    }
}