using Farmacontrol.ConsoleApp.UI.Helper;
using Farmacontrol.Core.Model;
using Farmacontrol.Model;

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

            Console.WriteLine("\n📦 Gestión de Inventario");
            PrintIfAllowed(allowed, "4.");
            PrintIfAllowed(allowed, "5.");
            PrintIfAllowed(allowed, "6.");
            PrintIfAllowed(allowed, "7.");
            PrintIfAllowed(allowed, "8.");

            Console.WriteLine("\n⚠️ Alertas y Avisos");
            PrintIfAllowed(allowed, "9.");
            PrintIfAllowed(allowed, "10.");

            if (allowed.Any(a => a.StartsWith("11.")))
            {
                Console.WriteLine("\n⚙️ Administración");
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