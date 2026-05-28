using Farmacontrol.Model;
using Farmacontrol.Repository;

namespace Farmacontrol.Services
{
    public class AuditService(AppDbContext db, UserSession userSession)
    {
        public void Log(string action, string details)
        {
            var username = userSession.CurrentUser?.Username ?? "System";
            var log = new AuditLog(username, action, details);
            db.AuditLogs.Add(log);
            db.SaveChanges();
        }
    }
}
