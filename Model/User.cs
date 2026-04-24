using Farmacontrol.Util;

namespace Farmacontrol.Model
{
    public abstract class User
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public abstract string Role { get; }

        public abstract List<string> GetAllowedActions();

        public User(string name, string username, string password)
        {
            Name = name;
            Username = username;
            Password = Hash.Hashing(password);
        }
        
        public bool ValidatePassword(string password) =>
            Password == Hash.Hashing(password);
    }
}