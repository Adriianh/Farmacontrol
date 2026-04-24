using Farmacontrol.Model;
using Farmacontrol.Model.ProductEntity;
using Farmacontrol.Model.UserEntity;
using Farmacontrol.Services;
using Farmacontrol.Services.Persistence;

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
                savedUsers.ForEach(u => _userManager.AddUser(u));

            List<Product> savedProducts = _persistence.LoadProducts(); 
            if (savedProducts.Count > 0)
                savedProducts.ForEach(p => _inventory.AddProduct(p));

            List<Supplier> savedSuppliers = _persistence.LoadSuppliers();
            if (savedSuppliers.Count > 0)
                savedSuppliers.ForEach(s => _supplierManager.AddSupplier(s));

            List<Alert> savedHistory = _persistence.LoadHistory();
            if (savedHistory.Count > 0)
                savedHistory.ForEach(_ => _persistence.SaveHistory(savedHistory));

            _sales = _persistence.LoadSales();
            _report = new Report(_sales);

            _salesCount = _sales.Count > 0
                ? _sales.Max(v => v.Code)
                : 0;
        }

        public void Iniciar()
        {
            if (Login())
                ShowMainMenu(_actualUser ?? throw new InvalidOperationException("Usuario no encontrado."));
            else
            {
                Console.WriteLine("Demasiados intentos fallidos. El sistema se cerrará.");
                Pause();
            }
        }

        private bool Login()
        {
            int attempts = 0;

            while (attempts < 3)
            {
                Console.Clear();
                Console.WriteLine("=== FARMACONTROL ===");
                Console.WriteLine($"Intentos restantes: {3 - attempts}");
                Console.WriteLine();

                Console.Write("Usuario: ");
                string username = Console.ReadLine() ?? throw new InvalidOperationException();

                Console.Write("Contraseña: ");
                string password = ReadPassword();

                _actualUser = _userManager.Authenticate(username, password);

                if (_actualUser != null)
                {
                    Console.WriteLine($"\nBienvenido, {_actualUser.Name} ({_actualUser.Role}).");
                    Pause();
                    return true;
                }

                Console.WriteLine("\nUsuario o contraseña incorrectos.");
                Pause();
                attempts++;
            }

            return false;
        }

        private void ManageUsers()
        {
            Console.Clear();
            Console.WriteLine("=== GESTIONAR USUARIOS ===");
            Console.WriteLine("1. Crear usuario");
            Console.WriteLine("2. Eliminar usuario");
            Console.WriteLine("3. Listar usuarios");
            Console.Write("\nSeleccione una opción: ");

            switch (Console.ReadLine())
            {
                case "1": CreateUser(); break;
                case "2": DeleteUser(); break;
                case "3": GetUsers(); break;
            }
        }

        private void CreateUser()
        {
            Console.Clear();
            Console.WriteLine("=== CREAR USUARIO ===");
            Console.Write("Ingrese la clave maestra: ");

            if (!_userManager.VerifyMasterKey(ReadPassword()))
            {
                Console.WriteLine("\nClave maestra incorrecta.");
                Pause();
                return;
            }

            Console.Write("\nNombre completo: ");
            string name = Console.ReadLine() ?? throw new InvalidOperationException();

            Console.Write("Nombre de usuario: ");
            string username = Console.ReadLine() ?? throw new InvalidOperationException();

            Console.Write("Contraseña: ");
            string password = ReadPassword();

            Console.WriteLine("\nRol:");
            Console.WriteLine("1. Administrador");
            Console.WriteLine("2. Encargado");
            Console.Write("Seleccione: ");

            User? newUser = Console.ReadLine() switch
            {
                "1" => new Administrator(name, username, password),
                "2" => new Employee(name, username, password),
                _ => null
            };

            if (newUser == null)
            {
                Console.WriteLine("Rol inválido.");
                Pause();
                return;
            }

            _userManager.AddUser(newUser);
            Console.WriteLine($"\nUsuario {username} creado correctamente como {newUser.Role}.");
            Pause();
        }

        private void DeleteUser()
        {
            Console.Clear();
            Console.WriteLine("=== ELIMINAR USUARIO ===");
            Console.Write("Ingrese la clave maestra: ");

            if (!_userManager.VerifyMasterKey(ReadPassword()))
            {
                Console.WriteLine("\nClave maestra incorrecta.");
                Pause();
                return;
            }

            GetUsers();
            Console.Write("\nNombre de usuario a eliminar: ");
            string username = Console.ReadLine() ?? throw new InvalidOperationException();

            if (username == _actualUser?.Username)
            {
                Console.WriteLine("No puede eliminar su propio usuario.");
                Pause();
                return;
            }

            _userManager.RemoveUser(username);
            Console.WriteLine($"Usuario {username} eliminado.");
            Pause();
        }

        private void GetUsers()
        {
            Console.Clear();
            Console.WriteLine("=== USUARIOS REGISTRADOS ===");
            foreach (User user in _userManager.GetAllUsers())
                Console.WriteLine($"{user.Username} — {user.Name} ({user.Role})");
            Pause();
        }

        private string ReadPassword()
        {
            string password = "";
            ConsoleKeyInfo key;

            do
            {
                key = Console.ReadKey(intercept: true);

                if (key.Key != ConsoleKey.Enter && key.Key != ConsoleKey.Backspace)
                {
                    password += key.KeyChar;
                    Console.Write("*");
                }
                else if (key.Key == ConsoleKey.Backspace && password.Length > 0)
                {
                    password = password[..^1];
                    Console.Write("\b \b");
                }
            } while (key.Key != ConsoleKey.Enter);

            Console.WriteLine();
            return password;
        }

        private void ShowMainMenu(User user)
        {
            bool running = true;
            while (running)
            {
                Console.Clear();
                user.GetAllowedActions().ForEach(Console.WriteLine);
                Console.Write("\nSeleccione una opción: ");

                switch (Console.ReadLine())
                {
                    case "1": RegisterSale(); break;
                    case "2": ManageInventory(); break;
                    case "3": SearchProduct(); break;
                    case "4":
                        _historyManager.VerifyAlert(_inventory.GetProducts.ToList());
                        _historyManager.ShowRecentHistory();
                        Pause();
                        break;
                    case "5":
                        _historyManager.ShowHistory();
                        break;
                    case "6": ShowReportsMenu(); break;
                    case "7":
                        _inventory.GetExpiredProducts();
                        Pause();
                        break;
                    case "8": ManageUsers(); break;
                    case "9": ManageSuppliers(); break;
                    case "10":
                        _supplierManager.GenerateAllOrders(_inventory.GetProducts.ToList());
                        Pause();
                        break;
                    case "0":
                    {
                        _persistence.SaveProducts(_inventory.GetProducts.ToList());
                        _persistence.SaveSales(_sales);
                        _persistence.SaveUsers(_userManager.GetAllUsers().ToList());
                        _persistence.SaveSuppliers(_supplierManager.GetSuppliers().ToList());

                        running = false;
                        break;
                    }
                    default:
                        Console.WriteLine("Opción inválida.");
                        Pause();
                        break;
                }
            }
        }

        private void RegisterSale()
        {
            _salesCount++;
            Sale sale = new Sale(_salesCount);
            bool adding = true;

            while (adding)
            {
                Console.Clear();
                Console.Write("Nombre o código del producto (o 'fin' para terminar): ");
                string input = Console.ReadLine() ?? throw new InvalidOperationException();

                if (input.ToLower() == "fin")
                {
                    adding = false;
                    continue;
                }

                Product? product = _inventory.SearchProduct(input);

                if (product == null)
                {
                    Console.WriteLine("Producto no encontrado.");
                    Pause();
                    continue;
                }

                product.ShowInformation();
                Console.Write("Cantidad: ");

                if (int.TryParse(Console.ReadLine(), out int quantity))
                {
                    try
                    {
                        sale.AddDetail(product, quantity);
                        Console.WriteLine("Producto agregado.");
                    }
                    catch (InvalidOperationException ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("Cantidad inválida.");
                }

                Pause();
            }

            _sales.Add(sale);
            Console.Clear();
            sale.ShowResume();
            Pause();
        }

        private void ManageInventory()
        {
            Console.Clear();
            Console.WriteLine("=== GESTIONAR INVENTARIO ===");
            Console.WriteLine("1. Agregar medicamento");
            Console.WriteLine("2. Agregar producto de belleza");
            Console.WriteLine("3. Mostrar todo el inventario");
            Console.Write("\nSeleccione una opción: ");

            switch (Console.ReadLine())
            {
                case "1": AddMedicine(); break;
                case "2": AddCosmetic(); break;
                case "3":
                    _inventory.ListProducts();
                    Pause();
                    break;
            }
        }

        private void AddMedicine()
        {
            Console.Clear();
            Console.WriteLine("=== AGREGAR MEDICAMENTO ===");

            Medicine medicine = new Medicine();

            Console.Write("Nombre: ");
            medicine.Name = Console.ReadLine() ?? throw new InvalidOperationException();

            Console.Write("Código: ");
            medicine.Code = Console.ReadLine() ?? throw new InvalidOperationException();

            Console.Write("Precio: ");
            medicine.Price = decimal.Parse(Console.ReadLine() ?? throw new InvalidOperationException());

            Console.Write("Stock inicial: ");
            medicine.Stock = int.Parse(Console.ReadLine() ?? throw new InvalidOperationException());

            Console.Write("Stock mínimo: ");
            medicine.MinimumStock = int.Parse(Console.ReadLine() ?? throw new InvalidOperationException());

            Console.Write("Principio activo: ");
            medicine.ActivePrinciple = Console.ReadLine() ?? throw new InvalidOperationException();

            Console.Write("Fecha de vencimiento (dd/MM/yyyy): ");
            medicine.ExpirationDate = DateTime.ParseExact(
                Console.ReadLine() ?? throw new InvalidOperationException(), "dd/MM/yyyy", null
            );

            Console.Write("¿Requiere receta? (s/n): ");
            medicine.RequiresPrescription = Console.ReadLine()?.ToLower() == "s";

            _inventory.AddProduct(medicine);
            Console.WriteLine("Medicamento agregado correctamente.");
            Pause();
        }

        private void AddCosmetic()
        {
            Console.Clear();
            Console.WriteLine("=== AGREGAR PRODUCTO DE BELLEZA ===");

            Cosmetic cosmetic = new Cosmetic();

            Console.Write("Nombre: ");
            cosmetic.Name = Console.ReadLine() ?? throw new InvalidOperationException();

            Console.Write("Código: ");
            cosmetic.Code = Console.ReadLine() ?? throw new InvalidOperationException();

            Console.Write("Precio: ");
            cosmetic.Price = decimal.Parse(Console.ReadLine() ?? throw new InvalidOperationException());

            Console.Write("Stock inicial: ");
            cosmetic.Stock = int.Parse(Console.ReadLine() ?? throw new InvalidOperationException());

            Console.Write("Stock mínimo: ");
            cosmetic.MinimumStock = int.Parse(Console.ReadLine() ?? throw new InvalidOperationException());

            Console.Write("Marca: ");
            cosmetic.Brand = Console.ReadLine() ?? throw new InvalidOperationException();

            Console.Write("Tipo (shampoo, crema, etc.): ");
            cosmetic.Type = Console.ReadLine() ?? throw new InvalidOperationException();

            Console.Write("Fecha de vencimiento (dd/MM/yyyy): ");
            cosmetic.ExpirationDate = DateTime.ParseExact(
                Console.ReadLine() ?? throw new InvalidOperationException(), "dd/MM/yyyy", null
            );

            _inventory.AddProduct(cosmetic);
            Console.WriteLine("Producto de belleza agregado correctamente.");
            Pause();
        }

        private void SearchProduct()
        {
            Console.Clear();
            Console.Write("Nombre o código del producto: ");
            Product? product = _inventory.SearchProduct(Console.ReadLine() ?? throw new InvalidOperationException());

            if (product == null)
                Console.WriteLine("Producto no encontrado.");
            else
                product.ShowInformation();

            Pause();
        }

        private void ShowReportsMenu()
        {
            Console.Clear();
            Console.WriteLine("=== REPORTES ===");
            Console.WriteLine("1. Ventas del día");
            Console.WriteLine("2. Ventas del mes");
            Console.WriteLine("3. Productos más vendidos");
            Console.Write("\nSeleccione una opción: ");

            switch (Console.ReadLine())
            {
                case "1": _report.GenerateDailySales(); break;
                case "2": _report.GenerateMonthSales(); break;
                case "3": _report.BestSellingProducts(); break;
            }

            Pause();
        }

        private void ManageSuppliers()
        {
            Console.Clear();
            Console.WriteLine("=== GESTIONAR PROVEEDORES ===");
            Console.WriteLine("1. Agregar proveedor");
            Console.WriteLine("2. Eliminar proveedor");
            Console.WriteLine("3. Listar proveedores");
            Console.WriteLine("4. Generar pedido por proveedor");
            Console.Write("\nSeleccione una opción: ");

            switch (Console.ReadLine())
            {
                case "1": AddSupplier(); break;
                case "2": RemoveSupplier(); break;
                case "3":
                    _supplierManager.GetAllSuppliers();
                    Pause();
                    break;
                case "4": PlaceOrderBySupplier(); break;
            }
        }

        private void AddSupplier()
        {
            Console.Clear();
            Console.WriteLine("=== AGREGAR PROVEEDOR ===");

            Console.Write("Código: ");
            string code = Console.ReadLine() ?? throw new InvalidOperationException();

            Console.Write("Nombre: ");
            string name = Console.ReadLine() ?? throw new InvalidOperationException();

            Console.Write("Teléfono: ");
            string phoneNumber = Console.ReadLine() ?? throw new InvalidOperationException();

            Console.Write("Correo: ");
            string email = Console.ReadLine() ?? throw new InvalidOperationException();

            Console.Write("Días de entrega estimados: ");
            int leadTimeDays = int.Parse(Console.ReadLine() ?? throw new InvalidOperationException());

            _supplierManager.AddSupplier(new Supplier(code, name, phoneNumber, email, leadTimeDays));
            Console.WriteLine("Proveedor agregado correctamente.");
            Pause();
        }

        private void RemoveSupplier()
        {
            Console.Clear();
            _supplierManager.GetAllSuppliers();
            Console.Write("Código del proveedor a eliminar: ");
            _supplierManager.RemoveSupplier(Console.ReadLine() ?? throw new InvalidOperationException());
            Console.WriteLine("Proveedor eliminado.");
            Pause();
        }

        private void PlaceOrderBySupplier()
        {
            Console.Clear();
            Console.Write("Nombre o código del proveedor: ");
            Supplier supplier =
                _supplierManager.SearchSupplier(Console.ReadLine() ?? throw new InvalidOperationException())
                ?? throw new InvalidOperationException("Proveedor no encontrado.");

            supplier.PlaceOrder(_inventory.GetProducts.ToList());
            Pause();
        }

        private void Pause()
        {
            Console.WriteLine("\nPresione cualquier tecla para continuar...");
            Console.ReadKey();
        }
    }
}