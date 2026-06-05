namespace Farmacontrol.Core.Services
{
    public class FileLogger
    {
        private readonly string _filePath;

        public FileLogger()
        {
            _filePath = Path.Combine(AppContext.BaseDirectory, "farmacontrol.log");
        }

        public void LogError(string message, System.Exception? exception = null)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var logMessage = $"[{timestamp}] [ERROR] {message}";
                if (exception != null)
                {
                    logMessage += $"\nException: {exception.Message}\nStack trace: {exception.StackTrace}";
                }
                logMessage += "\n--------------------------------------------------\n";
                File.AppendAllText(_filePath, logMessage);
            }
            catch
            {
                Console.WriteLine("No se pudo escribir en el archivo de registro de errores.");
            }
        }

        public void LogInfo(string message)
        {
            try
            {
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var logMessage = $"[{timestamp}] [INFO] {message}\n";
                File.AppendAllText(_filePath, logMessage);
            }
            catch
            {
                // ignored
            }
        }
    }
}