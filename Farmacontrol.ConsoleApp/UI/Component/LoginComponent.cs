using Farmacontrol.ConsoleApp.UI.Helper;
using Farmacontrol.Core.Model;
using Farmacontrol.Core.Services;
using Farmacontrol.Model;

namespace Farmacontrol.ConsoleApp.UI.Component
{
    public class LoginComponent(UserService userService)
    {
        public User? Login(int maxAttempts = 3)
        {
            int attempts = 0;

            while (attempts < maxAttempts)
            {
                ConsoleHelper.ShowTitle("Farmacontrol");
                Console.WriteLine($"Intentos restantes: {maxAttempts - attempts}");
                Console.WriteLine();

                string username = ConsoleHelper.ReadText("Usuario: ");

                Console.Write("Contraseña: ");
                string password = ConsoleHelper.ReadPassword();

                User? user = userService.Authenticate(username, password);

                if (user != null)
                {
                    Console.WriteLine($"\nBienvenido, {user.Name} ({user.Role}).");
                    ConsoleHelper.Pause();
                    return user;
                }

                Console.WriteLine("\nUsuario o contraseña incorrectos.");
                ConsoleHelper.Pause();
                attempts++;
            }

            return null;
        }
    }
}