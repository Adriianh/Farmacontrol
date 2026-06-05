namespace Farmacontrol.Core.Model
{
    public class AuditLog
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string Details { get; set; }

        public AuditLog(string username, string action, string details)
        {
            Username = username;
            Action = action;
            Details = details;
        }

        private AuditLog()
        {
            Username = string.Empty;
            Action = string.Empty;
            Details = string.Empty;
        }
    }
}