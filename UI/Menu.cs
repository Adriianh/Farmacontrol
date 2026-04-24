using Farmacontrol.Model;
using Farmacontrol.Services;
using Farmacontrol.Services.Persistence;
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
            if (Login())
                ShowMainMenu(_actualUser ?? throw new InvalidOperationException("Usuario no encontrado."));
            else
            {
                Console.WriteLine("Demasiados intentos fallidos. El sistema se cerrará.");
                ConsoleHelper.Pause();
            }
        }

        private bool Login()
        {
            int attempts = 0;

            while (attempts < 3)
            {
                ConsoleHelper.ShowTitle("Farmacontrol");
                Console.WriteLine($"Intentos restantes: {3 - attempts}");
                Console.WriteLine();

                string username = ConsoleHelper.ReadText("Usuario: ");

                Console.Write("Contraseña: ");
                string password = ConsoleHelper.ReadPassword();

                _actualUser = _userManager.Authenticate(username, password);

                if (_actualUser != null)
                {
                    Console.WriteLine($"\nBienvenido, {_actualUser.Name} ({_actualUser.Role}).");
                    ConsoleHelper.Pause();
                    return true;
                }

                Console.WriteLine("\nUsuario o contraseña incorrectos.");
                ConsoleHelper.Pause();
                attempts++;
            }

            return false;
        }

        private void ShowMainMenu(User user)
        {
            bool running = true;

            while (running)
            {
                Console.Clear();
                user.GetAllowedActions().ForEach(Console.WriteLine);

                string option = ConsoleHelper.ReadText("\nSeleccione una opción: ");

                switch (option)
                {
                    case "1":
                        RegisterSale();
                        break;

                    case "2":
                        new InventoryView(_inventory, _supplierManager).ManageInventory();
                        break;

                    case "3":
                        SearchProduct();
                        break;

                    case "4":
                        new AlertsView(_historyManager, _inventory).ShowTodayAlerts();
                        break;

                    case "5":
                        new AlertsView(_historyManager, _inventory).ShowHistory();
                        break;

                    case "6":
                        new ReportsView(_report).ShowReportsMenu();
                        break;

                    case "7":
                        ShowExpiredProducts();
                        break;

                    case "8":
                        new UsersView(_userManager, user).ManageUsers();
                        break;

                    case "9":
                        new SuppliersView(_supplierManager, _inventory).ManageSuppliers();
                        break;

                    case "10":
                        GenerateAllSupplierOrders();
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

        private void SearchProduct()
        {
            ConsoleHelper.ShowTitle("Buscar Producto");

            if (!_inventory.GetProducts.Any())
            {
                Console.WriteLine("No hay productos en inventario.");
                ConsoleHelper.Pause();
                return;
            }

            string input = ConsoleHelper.ReadText("Nombre o código del producto (o 'fin' para cancelar): ");
            if (input.ToLower() == "fin") return;

            Product? product = _inventory.SearchProduct(input);

            if (product == null)
                Console.WriteLine("Producto no encontrado.");
            else
                product.ShowInformation();

            ConsoleHelper.Pause();
        }

        private void ShowExpiredProducts()
        {
            ConsoleHelper.ShowTitle("Productos Vencidos");

            if (!_inventory.GetProducts.Any())
            {
                Console.WriteLine("No hay productos en inventario.");
                ConsoleHelper.Pause();
                return;
            }

            _inventory.GetExpiredProducts();
            ConsoleHelper.Pause();
        }

        private void GenerateAllSupplierOrders()
        {
            ConsoleHelper.ShowTitle("Generar Pedidos");

            if (!_supplierManager.GetSuppliers().Any())
            {
                Console.WriteLine("No hay proveedores registrados.");
                ConsoleHelper.Pause();
                return;
            }

            if (!_inventory.GetProducts.Any())
            {
                Console.WriteLine("No hay productos en inventario.");
                ConsoleHelper.Pause();
                return;
            }

            _supplierManager.GenerateAllOrders(_inventory.GetProducts.ToList());
            ConsoleHelper.Pause();
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