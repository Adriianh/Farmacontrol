namespace Farmacontrol.Core.Util
{
    public static class ReportExporter
    {
        public static string Export(string reportName, string content)
        {
            try
            {
                var directoryPath = Path.Combine(AppContext.BaseDirectory, "Reportes");
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                var fileName = $"{reportName}_{DateTime.Now:yyyyMMdd_HHmmss}.txt";
                var filePath = Path.Combine(directoryPath, fileName);
                
                File.WriteAllText(filePath, content);
                return filePath;
            }
            catch (System.Exception ex)
            {
                throw new IOException("Error al escribir el archivo de reporte en disco.", ex);
            }
        }
    }
}