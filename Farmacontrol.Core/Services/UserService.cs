using System.Text.RegularExpressions;
using Farmacontrol.Core.Model;
using Farmacontrol.Core.Model.UserEntity;
using Farmacontrol.Core.Repository;
using Farmacontrol.Core.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Farmacontrol.Core.Services
{
    public class UserService
    {
        private readonly AppDbContext _db;
        private string _masterKey;
        private readonly AuditService _audit;

        public UserService(AppDbContext db, IConfiguration configuration, AuditService audit)
        {
            _db = db;
            _masterKey = configuration["Security:MasterKey"] ?? string.Empty;
            _audit = audit;
            EnsureAdminUser();
        }

        public bool IsMasterKeyConfigured => !string.IsNullOrEmpty(_masterKey) && _masterKey.StartsWith("$2");

        public void SetMasterKey(string newPassword)
        {
            _masterKey = Hash.Hashing(newPassword);
            
            try 
            {
                string path = "appsettings.json";
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    string replacement = _masterKey.Replace("$", "$$");
                    var pattern = @"(""MasterKey""\s*:\s*"")[^""]*("")";
                    json = Regex.Replace(json, pattern, $"${{1}}{replacement}${{2}}");
                    File.WriteAllText(path, json);
                }
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Advertencia: No se pudo guardar la clave maestra permanentemente en appsettings.json: {ex.Message}");
            }
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
                user.IsActive = false;
                _db.Users.Update(user);
                _db.SaveChanges();
                _audit.Log("Eliminar Usuario", $"Se eliminó (borrado lógico) el usuario '{username}'.");
            }
        }
        
        public void UpdateUser(User user)
        {
            _db.Users.Update(user);
            _db.SaveChanges();
            _audit.Log("Modificar Usuario", $"Se modificó el usuario '{user.Username}'.");
        }
        
        public IReadOnlyList<User> GetAllUsers() => _db.Users.AsNoTracking().ToList().AsReadOnly();
    }
}