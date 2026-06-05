using Farmacontrol.Core.Model.ProductEntity;

namespace Farmacontrol.ConsoleApp.UI.Helper
{
    public static class ConsoleHelper
    {
        public static void Pause()
        {
            Console.WriteLine("\nPresione cualquier tecla para continuar...");
            Console.ReadKey();
        }

        public static void ShowTitle(string title)
        {
            Console.Clear();
            Console.WriteLine($"=== {title.ToUpper()} ===");
            Console.WriteLine();
        }

        public static string ReadText(string message, bool allowEmpty = false)
        {
            while (true)
            {
                Console.Write(message);
                
                var input = Console.ReadLine() ?? "";
                if (allowEmpty && string.IsNullOrWhiteSpace(input))
                    return "";
                    
                if (!string.IsNullOrWhiteSpace(input))
                    return input.Trim();
                
                Console.WriteLine("Valor inválido, ingrese una cadena no vacía.");
            }
        }

        public static int ReadInt(string message)
        {
            while (true)
            {
                Console.Write(message);
                
                if (int.TryParse(Console.ReadLine(), out int result))
                    return result;
                
                Console.WriteLine("Valor inválido, intente de nuevo.");
            }
        }

        public static decimal ReadDecimal(string message)
        {
            while (true)
            {
                Console.Write(message);
                
                if (decimal.TryParse(Console.ReadLine(), out decimal result))
                    return result;
                
                Console.WriteLine("Valor inválido, intente de nuevo.");
            }
        }

        public static DateTime ReadDate(string message, string format = "dd/MM/yyyy")
        {
            while (true)
            {
                Console.Write(message);
                
                if (DateTime.TryParseExact(Console.ReadLine(), format, null,
                        System.Globalization.DateTimeStyles.None, out DateTime result))
                    return result;
                
                Console.WriteLine($"Formato inválido. Use {format}.");
            }
        }

        public static string ReadPassword()
        {
            while (true)
            {
                var password = "";
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
                
                if (!string.IsNullOrWhiteSpace(password) && !password.Contains(" "))
                    return password;
                
                Console.WriteLine("Contraseña inválida, ingrese una contraseña no vacía y sin espacios.");
            }
        }

        public static bool Confirm(string message)
        {
            Console.Write($"{message} (s/n): ");
            return Console.ReadLine()?.ToLower() == "s";
        }

        public static string ReadTextWithDefault(string message, string currentValue)
        {
            Console.Write($"{message} [{currentValue}]: ");
            var input = Console.ReadLine() ?? "";
            return string.IsNullOrWhiteSpace(input) ? currentValue : input.Trim();
        }

        public static int ReadIntWithDefault(string message, int currentValue)
        {
            while (true)
            {
                Console.Write($"{message} [{currentValue}]: ");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) return currentValue;
                
                if (int.TryParse(input, out int result))
                    return result;
                
                Console.WriteLine("Valor inválido, intente de nuevo.");
            }
        }

        public static decimal ReadDecimalWithDefault(string message, decimal currentValue)
        {
            while (true)
            {
                Console.Write($"{message} [{currentValue}]: ");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) return currentValue;
                
                if (decimal.TryParse(input, out decimal result))
                    return result;
                
                Console.WriteLine("Valor inválido, intente de nuevo.");
            }
        }

        public static void PrintProductsTable(IEnumerable<Model.Product> products)
        {
            var productList = products.ToList();
            if (productList.Count == 0)
            {
                Console.WriteLine("No hay productos para mostrar.");
                return;
            }

            Console.WriteLine($"{"Nº",-3} | {"Código",-8} | {"Producto",-25} | {"Tipo",-12} | {"Stock",-6} | {"Mín.",-5} | Precio");
            Console.WriteLine(new string('-', 85));

            for (int i = 0; i < productList.Count; i++)
            {
                var p = productList[i];
                var name = p.Name.Length > 23 ? p.Name.Substring(0, 23) + ".." : p.Name;

                var type = p switch
                {
                    Medicine => "Medicamento",
                    Cosmetic => "Cosmético",
                    Supplement => "Suplemento",
                    Supply => "Suministro",
                    _ => "Genérico"
                };

                var stockStr = p.Stock <= p.MinimumStock ? $"[!] {p.Stock}" : p.Stock.ToString();
                
                Console.WriteLine($"{i + 1,-3} | {p.Code,-8} | {name,-25} | {type,-12} | {stockStr,-6} | {p.MinimumStock,-5} | Q{p.Price:F2}");
            }
            Console.WriteLine(new string('-', 85));
        }

        public static void PrintAlertsTable(IEnumerable<Farmacontrol.Core.Model.Alert> alerts)
        {
            var alertList = alerts.ToList();
            if (alertList.Count == 0)
            {
                Console.WriteLine("No hay alertas para mostrar.");
                return;
            }

            Console.WriteLine($"{"Nº",-3} | {"Fecha",-14} | {"Tipo",-25} | {"Código",-8} | Mensaje");
            Console.WriteLine(new string('-', 100));

            for (var i = 0; i < alertList.Count; i++)
            {
                var a = alertList[i];
                var date = a.Date.ToString("dd/MM HH:mm");
                var type = a.Type.Length > 25 ? a.Type.Substring(0, 25) : a.Type;
                var msg = a.Description.Length > 40 ? a.Description.Substring(0, 40) + ".." : a.Description;
                
                Console.WriteLine($"{i + 1,-3} | {date,-14} | {type,-25} | {a.ProductCode,-8} | {msg}");
            }
            Console.WriteLine(new string('-', 100));
        }

        public static void PrintUsersTable(IEnumerable<Farmacontrol.Core.Model.User> users)
        {
            var userList = users.ToList();
            if (userList.Count == 0)
            {
                Console.WriteLine("No hay usuarios para mostrar.");
                return;
            }

            Console.WriteLine($"{"Nº",-3} | {"Username",-15} | {"Nombre Completo",-25} | Rol");
            Console.WriteLine(new string('-', 60));

            for (var i = 0; i < userList.Count; i++)
            {
                var u = userList[i];
                string name = u.Name.Length > 25 ? u.Name.Substring(0, 25) : u.Name;
                
                Console.WriteLine($"{i + 1,-3} | {u.Username,-15} | {name,-25} | {u.Role}");
            }
            Console.WriteLine(new string('-', 60));
        }
    }
}