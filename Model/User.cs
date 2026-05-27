using System.Collections.Generic;
using Farmacontrol.Util;
namespace Farmacontrol.Model
{
    public abstract class User
    {
        public string Name { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        
        protected User(string name, string username, string password)
        {
            Name = name;
            Username = username;
            Password = Hash.Hashing(password);
        }
        
        protected User() { }
        
        public abstract string Role { get; }
        
        public abstract List<string> GetAllowedActions();
        
        public bool ValidatePassword(string password) =>
            Password == Hash.Hashing(password);
    }
}