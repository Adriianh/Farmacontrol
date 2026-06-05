using System.Text.RegularExpressions;
using Farmacontrol.Core.Model;
using Farmacontrol.Core.Repository;
using Farmacontrol.Core.Util;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Farmacontrol.Core.Services
{
    public class UserService(AppDbContext db, IConfiguration configuration, AuditService audit)
    {
        private string _masterKey = configuration["Security:MasterKey"] ?? string.Empty;

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


        
        public bool VerifyMasterKey(string password) =>
            Hash.Validate(password, _masterKey);

        public User? Authenticate(string username, string password)
        {
            var user = db.Users.AsNoTracking().FirstOrDefault(u => u.Username == username);
            if (user != null && user.ValidatePassword(password))
            {
                return user;
            }
            return null;
        }
        
        public void AddUser(User user)
        {
            db.Users.Add(user);
            db.SaveChanges();
            audit.Log("Crear Usuario", $"Se creó el usuario '{user.Username}' con rol '{user.Role}'.");
        }
        
        public void RemoveUser(string username)
        {
            var user = db.Users.Find(username);
            if (user != null)
            {
                user.IsActive = false;
                db.Users.Update(user);
                db.SaveChanges();
                audit.Log("Eliminar Usuario", $"Se eliminó (borrado lógico) el usuario '{username}'.");
            }
        }
        
        public void UpdateUser(User user)
        {
            db.Users.Update(user);
            db.SaveChanges();
            audit.Log("Modificar Usuario", $"Se modificó el usuario '{user.Username}'.");
        }
        
        public IReadOnlyList<User> GetAllUsers() => db.Users.AsNoTracking().ToList().AsReadOnly();
    }
}