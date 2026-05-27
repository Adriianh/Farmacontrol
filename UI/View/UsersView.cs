using Farmacontrol.Services;
using Farmacontrol.Model;
using Farmacontrol.Model.UserEntity;
using Farmacontrol.UI.Helper;

namespace Farmacontrol.UI.View
{
    public class UsersView(UserManager userManager)
    {
        public void ManageUsers(User currentUser)
        {
            ConsoleHelper.ShowTitle("Gestionar Usuarios");
            Console.WriteLine("1. Crear usuario");
            Console.WriteLine("2. Eliminar usuario");
            Console.WriteLine("3. Listar usuarios");

            string option = ConsoleHelper.ReadText("\nSeleccione una opción (o 'fin' para cancelar): ");
            if (option.ToLower() == "fin") return;
            switch (option)
            {
                case "1": CreateUser(); break;
                case "2": RemoveUser(currentUser); break;
                case "3": ListUsers(); break;
            }
        }

        private bool VerifyMasterKey()
        {
            Console.Write("Ingrese la clave maestra (o 'fin' para cancelar): ");
            string input = ConsoleHelper.ReadPassword();
            if (input.ToLower() == "fin") return false;
            if (userManager.VerifyMasterKey(input))
                return true;

            Console.WriteLine("Clave maestra incorrecta.");
            ConsoleHelper.Pause();
            return false;
        }

        private void CreateUser()
        {
            ConsoleHelper.ShowTitle("Crear Usuario");

            if (!VerifyMasterKey())
                return;

            string name = ConsoleHelper.ReadText("Nombre completo (o 'fin' para cancelar): ");
            if (name.ToLower() == "fin") return;
            string username;
            while (true)
            {
                username = ConsoleHelper.ReadText("Nombre de usuario (o 'fin' para cancelar): ");
                if (username.ToLower() == "fin") return;
                if (userManager.GetAllUsers().Any(u => u.Username == username))
                {
                    Console.WriteLine("Ya existe un usuario con ese nombre de usuario.");
                    continue;
                }
                break;
            }
            string password;
            while (true)
            {
                Console.Write("Contraseña (o 'fin' para cancelar): ");
                password = ConsoleHelper.ReadPassword();
                if (password.ToLower() == "fin") return;
                if (string.IsNullOrWhiteSpace(password) || password.Contains(" "))
                {
                    Console.WriteLine("Contraseña inválida, ingrese una contraseña no vacía y sin espacios.");
                    continue;
                }
                break;
            }

            Console.WriteLine("\nRol:");
            Console.WriteLine("1. Administrador");
            Console.WriteLine("2. Empleado");

            User? newUser = ConsoleHelper.ReadText("Seleccione (o 'fin' para cancelar): ") switch
            {
                "1" => new Administrator(name, username, password),
                "2" => new Employee(name, username, password),
                _ => null
            };

            if (newUser == null)
            {
                Console.WriteLine("Rol inválido o cancelado.");
                ConsoleHelper.Pause();
                return;
            }

            userManager.AddUser(newUser);
            Console.WriteLine($"Usuario {username} creado correctamente como {newUser.Role}.");
            ConsoleHelper.Pause();
        }

        private void RemoveUser(User currentUser)
        {
            ConsoleHelper.ShowTitle("Eliminar Usuario");

            if (!VerifyMasterKey())
                return;

            ListUsers(false);
            string username = ConsoleHelper.ReadText("\nNombre de usuario a eliminar (o 'fin' para cancelar): ");
            if (username.ToLower() == "fin") return;

            if (username == currentUser.Username)
            {
                Console.WriteLine("No puede eliminar su propio usuario.");
                ConsoleHelper.Pause();
                return;
            }
            if (userManager.GetAllUsers().All(u => u.Username != username))
            {
                Console.WriteLine("No existe un usuario con ese nombre de usuario.");
                ConsoleHelper.Pause();
                return;
            }

            userManager.RemoveUser(username);
            Console.WriteLine($"Usuario {username} eliminado.");
            ConsoleHelper.Pause();
        }

        private void ListUsers(bool pause = true)
        {
            ConsoleHelper.ShowTitle("Usuarios Registrados");
            var users = userManager.GetAllUsers();
            if (!users.Any())
                Console.WriteLine("No hay usuarios registrados.");
            else
                foreach (User u in users)
                    Console.WriteLine($"{u.Username} — {u.Name} ({u.Role})");
            if (pause) ConsoleHelper.Pause();
        }
    }
}