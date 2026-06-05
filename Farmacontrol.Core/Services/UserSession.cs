using System.Text.Json;
using Farmacontrol.Core.Model;
using Farmacontrol.Core.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Farmacontrol.Core.Services;

public class UserSession(IServiceProvider serviceProvider)
{
    public User? CurrentUser { get; private set; }

    public void SetUser(User? user)
    {
        CurrentUser = user;
    }

    private string GetSessionFilePath()
    {
        var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Farmacontrol");
        Directory.CreateDirectory(appDataPath);
        return Path.Combine(appDataPath, "session.json");
    }

    public void SaveSession()
    {
        if (CurrentUser == null) return;
        
        var path = GetSessionFilePath();
        var data = new { CurrentUser.Username };
        var json = JsonSerializer.Serialize(data);
        File.WriteAllText(path, json);
    }

    public void LoadSession()
    {
        var path = GetSessionFilePath();
        if (!File.Exists(path)) return;

        try
        {
            var json = File.ReadAllText(path);
            using var doc = JsonDocument.Parse(json);
            
            if (!doc.RootElement.TryGetProperty("Username", out var usernameProp)) return;
            var username = usernameProp.GetString();
            
            if (string.IsNullOrEmpty(username)) return;
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var user = db.Users.AsNoTracking().FirstOrDefault(u => u.Username == username && u.IsActive);
            
            if (user != null)
            {
                CurrentUser = user;
            }
            else
            {
                ClearSession();
            }
        }
        catch (System.Exception)
        {
            ClearSession();
        }
    }

    public void ClearSession()
    {
        CurrentUser = null;
        var path = GetSessionFilePath();
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
