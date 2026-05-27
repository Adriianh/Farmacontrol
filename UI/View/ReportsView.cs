using Farmacontrol.Model;
using Farmacontrol.Services;
using Farmacontrol.UI.Helper;
using Farmacontrol.Util;

namespace Farmacontrol.UI.View
{
    public class ReportsView(SalesManager salesManager)
    {
        public void ShowReportsMenu()
        {
            var sales = salesManager.GetAllSales().ToList();
            var report = new Report(sales);

            ConsoleHelper.ShowTitle("Reportes");
            Console.WriteLine("1. Ventas del día");
            Console.WriteLine("2. Ventas del mes");
            Console.WriteLine("3. Productos más vendidos");

            string option = ConsoleHelper.ReadText("\nSeleccione una opción (o 'fin' para cancelar): ");
            if (option.ToLower() == "fin") return;

            string content = string.Empty;
            string reportType = string.Empty;

            switch (option)
            {
                case "1":
                    if (!report.HasSales)
                        Console.WriteLine("No hay ventas registradas.");
                    else
                    {
                        content = report.GenerateDailySales();
                        reportType = "Reporte_Ventas_Diarias";
                    }
                    break;
                case "2":
                    if (!report.HasSales)
                        Console.WriteLine("No hay ventas registradas.");
                    else
                    {
                        content = report.GenerateMonthSales();
                        reportType = "Reporte_Ventas_Mensuales";
                    }
                    break;
                case "3":
                    if (!report.HasSales)
                        Console.WriteLine("No hay ventas registradas.");
                    else
                    {
                        content = report.BestSellingProducts();
                        reportType = "Reporte_Productos_Mas_Vendidos";
                    }
                    break;
            }

            if (!string.IsNullOrEmpty(content))
            {
                Console.WriteLine("\n" + content);
                if (ConsoleHelper.Confirm("¿Desea exportar este reporte a un archivo de texto?"))
                {
                    try
                    {
                        string path = ReportExporter.Export(reportType, content);
                        Console.WriteLine($"\n[Éxito] Reporte exportado correctamente en: {path}");
                    }
                    catch (System.Exception ex)
                    {
                        Console.WriteLine($"\n[Error] No se pudo exportar el reporte: {ex.Message}");
                    }
                }
            }

            ConsoleHelper.Pause();
        }
    }
}