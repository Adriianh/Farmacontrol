using System.Text;
using Farmacontrol.ConsoleApp.UI.Helper;
using Farmacontrol.Core.Model;
using Farmacontrol.Core.Services;
using Farmacontrol.Core.Util;

namespace Farmacontrol.ConsoleApp.UI.View
{
    public class ReportsView(SalesService salesService)
    {
        public void ShowReportsMenu()
        {
            var sales = salesService.GetAllSales().ToList();
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
                        content = GenerateDailySalesFormatted(sales);
                        reportType = "Reporte_Ventas_Diarias";
                    }

                    break;
                case "2":
                    if (!report.HasSales)
                        Console.WriteLine("No hay ventas registradas.");
                    else
                    {
                        content = GenerateMonthSalesFormatted(sales);
                        reportType = "Reporte_Ventas_Mensuales";
                    }

                    break;
                case "3":
                    if (!report.HasSales)
                        Console.WriteLine("No hay ventas registradas.");
                    else
                    {
                        content = GenerateBestSellingFormatted(sales);
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
                    catch (Exception ex)
                    {
                        Console.WriteLine($"\n[Error] No se pudo exportar el reporte: {ex.Message}");
                    }
                }
            }

            ConsoleHelper.Pause();
        }

        private string GenerateDailySalesFormatted(List<Sale> sales)
        {
            var todaySales = sales.Where(s => s.Date.Date == DateTime.Today && !s.IsVoided).ToList();
            var sb = new StringBuilder();
            
            sb.AppendLine($"\n=== 📊 REPORTE DE VENTAS DIARIAS ({DateTime.Today:dd/MM/yyyy}) ===");
            sb.AppendLine($"{"ID",-5} | {"Hora",-8} | {"Cliente",-20} | {"Método",-12} | {"Total",-10}");
            sb.AppendLine(new string('-', 65));
            foreach (var s in todaySales)
            {
                var client = s.ClientName is { Length: > 20 } ? s.ClientName[..20] : s.ClientName;
                sb.AppendLine(
                    $"{s.Code,-5} | {s.Date:HH:mm} | {client,-20} | {s.PaymentMethod,-12} | Q{s.Total,-9:F2}");
            }

            sb.AppendLine(new string('=', 65));
            sb.AppendLine($" Ingreso Neto del Día: Q{todaySales.Sum(s => s.Total):F2}");
            sb.AppendLine($" Total Operaciones: {todaySales.Count}");
            sb.AppendLine($" Ventas Anuladas Hoy: {sales.Count(s => s.Date.Date == DateTime.Today && s.IsVoided)}");
            return sb.ToString();
        }

        private string GenerateMonthSalesFormatted(List<Sale> sales)
        {
            var monthSales = sales.Where(s =>
                s.Date.Month == DateTime.Today.Month && s.Date.Year == DateTime.Today.Year && !s.IsVoided).ToList();
            var sb = new StringBuilder();
            
            sb.AppendLine($"\n=== 📅 REPORTE DE VENTAS DEL MES ({DateTime.Today:MMMM yyyy}) ===");
            sb.AppendLine($"{"Fecha",-10} | {"ID",-5} | {"Cliente",-20} | {"Total",-10}");
            sb.AppendLine(new string('-', 55));
            foreach (var s in monthSales)
            {
                var client = s.ClientName is { Length: > 20 } ? s.ClientName[..20] : s.ClientName;
                sb.AppendLine($"{s.Date:dd/MM} | {s.Code,-5} | {client,-20} | Q{s.Total,-9:F2}");
            }

            sb.AppendLine(new string('=', 55));
            sb.AppendLine($" Ingreso Neto del Mes: Q{monthSales.Sum(s => s.Total):F2}");
            sb.AppendLine($" Total Operaciones: {monthSales.Count}");
            sb.AppendLine(
                $" Ventas Anuladas este Mes: {sales.Count(s => s.Date.Month == DateTime.Today.Month && s.Date.Year == DateTime.Today.Year && s.IsVoided)}");
            return sb.ToString();
        }

        private string GenerateBestSellingFormatted(List<Sale> sales)
        {
            var bestSelling = sales
                .Where(sale => !sale.IsVoided)
                .SelectMany(sale => sale.Details)
                .GroupBy(detail => new { detail.ProductCode, detail.ProductName })
                .Select(group => new
                {
                    group.Key.ProductCode,
                    group.Key.ProductName,
                    TotalQuantity = group.Sum(detail => detail.Quantity),
                    TotalRevenue = group.Sum(detail => detail.Subtotal)
                })
                .OrderByDescending(x => x.TotalQuantity)
                .Take(10)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"\n=== 🏆 TOP 10 PRODUCTOS MÁS VENDIDOS ===");
            sb.AppendLine($"{"Código",-10} | {"Producto",-30} | {"Cant",-6} | {"Ingresos",-10}");
            sb.AppendLine(new string('-', 65));
            foreach (var p in bestSelling)
            {
                var name = p.ProductName.Length > 28 ? p.ProductName.Substring(0, 28) + ".." : p.ProductName;
                sb.AppendLine($"{p.ProductCode,-10} | {name,-30} | {p.TotalQuantity,-6} | Q{p.TotalRevenue,-9:F2}");
            }

            sb.AppendLine(new string('=', 65));
            return sb.ToString();
        }
    }
}