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
        private string _masterKey;
        private readonly AuditService _audit;
        private readonly IConfiguration _configuration;

        public UserManager(AppDbContext db, IConfiguration configuration, AuditService audit)
        {
            _db = db;
            _configuration = configuration;
            _masterKey = configuration["Security:MasterKey"] ?? string.Empty;
            _audit = audit;
            EnsureAdminUser();
        }

        public bool IsMasterKeyConfigured => !string.IsNullOrEmpty(_masterKey) && _masterKey.StartsWith("$2");

        public void SetMasterKey(string newPassword)
        {
            _masterKey = Hash.Hashing(newPassword);
            
            // Reflejarlo en el archivo appsettings.json
            try 
            {
                string path = "appsettings.json";
                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    // Para C# Regex.Replace, un '$' literal en el reemplazo debe escaparse como '$$'
                    string replacement = _masterKey.Replace("$", "$$");
                    var pattern = @"(""MasterKey""\s*:\s*"")[^""]*("")";
                    json = System.Text.RegularExpressions.Regex.Replace(json, pattern, $"${{1}}{replacement}${{2}}");
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
                _db.Users.Remove(user);
                _db.SaveChanges();
                _audit.Log("Eliminar Usuario", $"Se eliminó el usuario '{username}'.");
            }
        }
        
        public IReadOnlyList<User> GetAllUsers() => _db.Users.AsNoTracking().ToList().AsReadOnly();
    }
}