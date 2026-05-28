using Farmacontrol.ConsoleApp.UI.Helper;
using Farmacontrol.Model;
using Farmacontrol.Model.UserEntity;
using Farmacontrol.Services;

namespace Farmacontrol.ConsoleApp.UI.View
{
    public class UsersView(UserManager userManager)
    {
        public void ManageUsers(User currentUser)
        {
            ConsoleHelper.ShowTitle("Gestionar Usuarios");
            Console.WriteLine("1. Crear usuario");
            Console.WriteLine("2. Eliminar usuario");
            Console.WriteLine("3. Listar usuarios");
            Console.WriteLine("4. Modificar usuario");

            string option = ConsoleHelper.ReadText("\nSeleccione una opción (o 'fin' para cancelar): ");
            if (option.ToLower() == "fin") return;
            switch (option)
            {
                case "1": CreateUser(currentUser); break;
                case "2": RemoveUser(currentUser); break;
                case "3": ListUsers(); break;
                case "4": UpdateUser(currentUser); break;
            }
        }

        private bool VerifyMasterKey(User currentUser)
        {
            if (currentUser is Administrator)
                return true;

            Console.Write("Ingrese la clave maestra (o 'fin' para cancelar): ");
            string input = ConsoleHelper.ReadPassword();
            if (input.ToLower() == "fin") return false;
            if (userManager.VerifyMasterKey(input))
                return true;

            Console.WriteLine("Clave maestra incorrecta.");
            ConsoleHelper.Pause();
            return false;
        }

        private void CreateUser(User currentUser)
        {
            ConsoleHelper.ShowTitle("Crear Usuario");

            if (!VerifyMasterKey(currentUser))
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

            if (!VerifyMasterKey(currentUser))
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

        private void UpdateUser(User currentUser)
        {
            ConsoleHelper.ShowTitle("Modificar Usuario");

            if (!VerifyMasterKey(currentUser))
                return;

            ListUsers(false);
            string username = ConsoleHelper.ReadText("\nNombre de usuario a modificar (o 'fin' para cancelar): ");
            if (username.ToLower() == "fin") return;

            var userToEdit = userManager.GetAllUsers().FirstOrDefault(u => u.Username == username);
            if (userToEdit == null)
            {
                Console.WriteLine("No existe un usuario con ese nombre de usuario.");
                ConsoleHelper.Pause();
                return;
            }

            userToEdit.Name = ConsoleHelper.ReadTextWithDefault("Nombre completo", userToEdit.Name);
            Console.WriteLine("Nota: El nombre de usuario (username) no puede ser modificado.");

            string newPassword = ConsoleHelper.ReadText("Nueva contraseña (deje en blanco para mantener la actual): ", allowEmpty: true);
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                // To update password properly we'd need to hash it. Since it's done in the constructor, we might need a method or direct hash.
                userToEdit.Password = Farmacontrol.Util.Hash.Hashing(newPassword);
            }

            userManager.UpdateUser(userToEdit);
            Console.WriteLine($"Usuario {username} modificado.");
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