namespace Farmacontrol.ConsoleApp.UI.Helper
{
    public class ConsoleHelper
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

        public static DateTime ReadDateWithDefault(string message, DateTime currentValue, string format = "dd/MM/yyyy")
        {
            while (true)
            {
                Console.Write($"{message} [{currentValue.ToString(format)}]: ");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input)) return currentValue;
                
                if (DateTime.TryParseExact(input, format, null,
                        System.Globalization.DateTimeStyles.None, out DateTime result))
                    return result;
                
                Console.WriteLine($"Formato inválido. Use {format}.");
            }
        }
    }
}