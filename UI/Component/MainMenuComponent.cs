using Farmacontrol.Model;
using Farmacontrol.UI.Helper;

namespace Farmacontrol.UI.Component
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