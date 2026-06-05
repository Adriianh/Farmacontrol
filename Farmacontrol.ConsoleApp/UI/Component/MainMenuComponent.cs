using Farmacontrol.ConsoleApp.UI.Helper;
using Farmacontrol.Core.Model;

namespace Farmacontrol.ConsoleApp.UI.Component
{
    public class MainMenuComponent
    {
        public string ReadOption(User user)
        {
            var allowed = user.GetAllowedActions();

            Console.WriteLine("\n📊 Ventas y Caja");
            PrintIfAllowed(allowed, "1.");
            PrintIfAllowed(allowed, "2.");
            PrintIfAllowed(allowed, "3.");
            PrintIfAllowed(allowed, "4.");

            Console.WriteLine("\n📦 Gestión de Inventario");
            PrintIfAllowed(allowed, "5.");
            PrintIfAllowed(allowed, "6.");
            PrintIfAllowed(allowed, "7.");

            Console.WriteLine("\n⚠️ Alertas y Avisos");
            PrintIfAllowed(allowed, "8.");
            PrintIfAllowed(allowed, "9.");

            if (allowed.Any(a => a.StartsWith("10.")))
            {
                Console.WriteLine("\n⚙️ Administración");
                PrintIfAllowed(allowed, "10.");
                PrintIfAllowed(allowed, "11.");
            }

            Console.WriteLine("");
            PrintIfAllowed(allowed, "0.");

            return ConsoleHelper.ReadText("\nSeleccione una opción: ");
        }

        private void PrintIfAllowed(List<string> allowed, string prefix)
        {
            var action = allowed.FirstOrDefault(a => a.StartsWith(prefix));
            if (action != null)
            {
                Console.WriteLine("   " + action);
            }
        }
    }
}