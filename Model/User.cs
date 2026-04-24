using Farmacontrol.Util;

namespace Farmacontrol.Model
{
    public abstract class User(string name, string username, string password)
    {
        public string Name { get; set; } = name;
        public string Username { get; set; } = username;
        public string Password { get; set; } = Hash.Hashing(password);
        public abstract string Role { get; }

        public abstract List<string> GetAllowedActions();

        public bool ValidatePassword(string password) =>
            Password == Hash.Hashing(password);
    }
}