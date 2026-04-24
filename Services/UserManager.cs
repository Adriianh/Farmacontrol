using Farmacontrol.Model;
using Farmacontrol.Model.UserEntity;
using Farmacontrol.Util;

namespace Farmacontrol.Services
{
    public class UserManager
    {
        private readonly List<User> _users = new();
        private const string MasterKey = "ef92b778bafe771207a9974a1257addb1a13c195"; // hash de "claveMaestra123"

        public UserManager()
        {
            if (_users.Count == 0)
                _users.Add(new Administrator("Admin", "admin", "admin123"));
        }
        
        public bool VerifyMasterKey(string password) =>
            Hash.Hashing(password) == MasterKey;

        public User? Authenticate(string username, string password) =>
            _users.FirstOrDefault(user =>
                user.Username == username && user.Password == Hash.Hashing(password)
            );
        
        public void AddUser(User user)  => _users.Add(user);
        
        public void RemoveUser(string username) => _users.RemoveAll(user => user.Username == username);
        
        public IReadOnlyList<User> GetAllUsers() => _users.AsReadOnly();
    }
}