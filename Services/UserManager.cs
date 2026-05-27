using Farmacontrol.Model;
using Farmacontrol.Model.UserEntity;
using Farmacontrol.Repository;
using Farmacontrol.Util;

namespace Farmacontrol.Services
{
    public class UserManager
    {
        private readonly AppDbContext _db;
        private const string MasterKey = "ef92b778bafe771207a9974a1257addb1a13c195"; // hash de "claveMaestra123"

        public UserManager(AppDbContext db)
        {
            _db = db;
            EnsureAdminUser();
        }

        private void EnsureAdminUser()
        {
            if (!_db.Users.Any())
            {
                _db.Users.Add(new Administrator("Admin", "admin", "admin123"));
                _db.SaveChanges();
            }
        }
        
        public bool VerifyMasterKey(string password) =>
            Hash.Hashing(password) == MasterKey;

        public User? Authenticate(string username, string password) =>
            _db.Users.FirstOrDefault(user =>
                user.Username == username && user.Password == Hash.Hashing(password)
            );
        
        public void AddUser(User user)
        {
            _db.Users.Add(user);
            _db.SaveChanges();
        }
        
        public void RemoveUser(string username)
        {
            var user = _db.Users.Find(username);
            if (user != null)
            {
                _db.Users.Remove(user);
                _db.SaveChanges();
            }
        }
        
        public IReadOnlyList<User> GetAllUsers() => _db.Users.ToList().AsReadOnly();
    }
}