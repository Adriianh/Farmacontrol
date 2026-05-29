using Farmacontrol.ConsoleApp.UI.Helper;
using Farmacontrol.Core.Model;
using Farmacontrol.Model;

namespace Farmacontrol.ConsoleApp.UI.Component
{
    public class MainMenuComponent
    {
        public string ReadOption(User user)
        {
            Console.Clear();
            user.GetAllowedActions().ForEach(Console.WriteLine);

            return ConsoleHelper.ReadText("\nSeleccione una opción: ");
        }
    }
}