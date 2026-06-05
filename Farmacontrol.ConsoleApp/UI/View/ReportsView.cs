using Farmacontrol.ConsoleApp.UI.Helper;
using Farmacontrol.Core.Model;
using Farmacontrol.Core.Services;
using Farmacontrol.Core.Util;
using Farmacontrol.Model;

namespace Farmacontrol.ConsoleApp.UI.View
{
    public class ReportsView(SalesService salesService)
    {
        public void ShowReportsMenu()
        {
            var sales = salesService.GetAllSales().ToList();
            var report = new Report(sales);

            ConsoleHelper.ShowTitle("Reportes");
            System.Console.WriteLine("1. Ventas del día");
            System.Console.WriteLine("2. Ventas del mes");
            System.Console.WriteLine("3. Productos más vendidos");

            string option = ConsoleHelper.ReadText("\nSeleccione una opción (o 'fin' para cancelar): ");
            if (option.ToLower() == "fin") return;

            string content = string.Empty;
            string reportType = string.Empty;

            switch (option)
            {
                case "1":
                    if (!report.HasSales)
                        System.Console.WriteLine("No hay ventas registradas.");
                    else
                    {
                        content = report.GenerateDailySales();
                        reportType = "Reporte_Ventas_Diarias";
                    }
                    break;
                case "2":
                    if (!report.HasSales)
                        System.Console.WriteLine("No hay ventas registradas.");
                    else
                    {
                        content = report.GenerateMonthSales();
                        reportType = "Reporte_Ventas_Mensuales";
                    }
                    break;
                case "3":
                    if (!report.HasSales)
                        System.Console.WriteLine("No hay ventas registradas.");
                    else
                    {
                        content = report.BestSellingProducts();
                        reportType = "Reporte_Productos_Mas_Vendidos";
                    }
                    break;
            }

            if (!string.IsNullOrEmpty(content))
            {
                System.Console.WriteLine("\n" + content);
                if (ConsoleHelper.Confirm("¿Desea exportar este reporte a un archivo de texto?"))
                {
                    try
                    {
                        string path = ReportExporter.Export(reportType, content);
                        System.Console.WriteLine($"\n[Éxito] Reporte exportado correctamente en: {path}");
                    }
                    catch (System.Exception ex)
                    {
                        System.Console.WriteLine($"\n[Error] No se pudo exportar el reporte: {ex.Message}");
                    }
                }
            }

            ConsoleHelper.Pause();
        }
    }
}