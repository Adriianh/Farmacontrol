using Farmacontrol.Model;
using Farmacontrol.Model.UserEntity;
using Farmacontrol.Repository;
using Farmacontrol.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Farmacontrol.Services
{
    public class UserManager
    {
        private readonly AppDbContext _db;
        private readonly string _masterKey;
        private readonly AuditService _audit;

        public UserManager(AppDbContext db, IConfiguration configuration, AuditService audit)
        {
            _db = db;
            _masterKey = configuration["Security:MasterKey"] ?? "ef92b778bafe771207a9974a1257addb1a13c195";
            _audit = audit;
            EnsureAdminUser();
        }

        private void EnsureAdminUser()
        {
            if (_db.Users.Any()) return;
            _db.Users.Add(new Administrator("Admin", "admin", "admin123"));
            _db.SaveChanges();
        }
        
        public bool VerifyMasterKey(string password) =>
            Hash.Validate(password, _masterKey);

        public User? Authenticate(string username, string password)
        {
            var user = _db.Users.AsNoTracking().FirstOrDefault(u => u.Username == username);
            if (user != null && user.ValidatePassword(password))
            {
                return user;
            }
            return null;
        }
        
        public void AddUser(User user)
        {
            _db.Users.Add(user);
            _db.SaveChanges();
            _audit.Log("Crear Usuario", $"Se creó el usuario '{user.Username}' con rol '{user.Role}'.");
        }
        
        public void RemoveUser(string username)
        {
            var user = _db.Users.Find(username);
            if (user != null)
            {
                _db.Users.Remove(user);
                _db.SaveChanges();
                _audit.Log("Eliminar Usuario", $"Se eliminó el usuario '{username}'.");
            }
        }
        
        public IReadOnlyList<User> GetAllUsers() => _db.Users.AsNoTracking().ToList().AsReadOnly();
    }
}