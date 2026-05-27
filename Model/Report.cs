using System.Text;

namespace Farmacontrol.Model
{
    public class Report(List<Sale>? sales)
    {
        public bool HasSales => sales is { Count: > 0 };

        public string GenerateDailySales()
        {
            if (sales == null) return "No hay ventas registradas.";
            
            DateTime actualDate = DateTime.Today;
            List<Sale> todaySales = sales
                .Where(sale => sale.Date.Date == actualDate && !sale.IsVoided)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"=== Ventas del {actualDate:dd/MM/yyyy} ===");
            foreach (var sale in todaySales)
            {
                sb.AppendLine($"Venta #{sale.Code} - {sale.Date:dd/MM/yyyy HH:mm:ss}");
                sb.AppendLine("------------------");
                foreach (var detail in sale.Details)
                {
                    sb.AppendLine($"{detail.ProductName} ({detail.ProductCode}) | Cantidad: {detail.Quantity} | Subtotal: Q{detail.Subtotal:F2}");
                }
                sb.AppendLine("------------------");
                sb.AppendLine($"Total: Q{sale.Total:F2}");
                sb.AppendLine();
            }

            decimal total = todaySales.Sum(sale => sale.Total);
            sb.AppendLine($"Ingreso Neto del día: Q{total:F2}");
            sb.AppendLine($"Ventas Anuladas: {sales.Count(s => s.Date.Date == actualDate && s.IsVoided)}");
            return sb.ToString();
        }

        public string GenerateMonthSales()
        {
            if (sales == null) return "No hay ventas registradas.";
            
            DateTime actualDate = DateTime.Today;
            List<Sale> monthSales = sales
                .Where(sale => sale.Date.Month == actualDate.Month && sale.Date.Year == actualDate.Year && !sale.IsVoided)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"=== Ventas de {actualDate:MMMM yyyy} ===");
            foreach (var sale in monthSales)
            {
                sb.AppendLine($"Venta #{sale.Code} - {sale.Date:dd/MM/yyyy HH:mm:ss}");
                sb.AppendLine("------------------");
                foreach (var detail in sale.Details)
                {
                    sb.AppendLine($"{detail.ProductName} ({detail.ProductCode}) | Cantidad: {detail.Quantity} | Subtotal: Q{detail.Subtotal:F2}");
                }
                sb.AppendLine("------------------");
                sb.AppendLine($"Total: Q{sale.Total:F2}");
                sb.AppendLine();
            }

            decimal total = monthSales.Sum(sale => sale.Total);
            sb.AppendLine($"Ingreso Neto del mes: Q{total:F2}");
            sb.AppendLine($"Ventas Anuladas: {sales.Count(s => s.Date.Month == actualDate.Month && s.Date.Year == actualDate.Year && s.IsVoided)}");
            return sb.ToString();
        }

        public string BestSellingProducts()
        {
            if (sales == null) return "No hay ventas registradas.";
            var sb = new StringBuilder();
            sb.AppendLine("=== Productos más vendidos ===");

            var bestSelling = sales
                .Where(sale => !sale.IsVoided)
                .SelectMany(sale => sale.GetDetails)
                .GroupBy(detail => detail.ProductName)
                .Select(group => new
                {
                    ProductName = group.Key,
                    TotalQuantity = group.Sum(detail => detail.Quantity)
                })
                .OrderByDescending(quantity => quantity.TotalQuantity)
                .Take(5)
                .ToList();

            foreach (var product in bestSelling)
            {
                sb.AppendLine($"{product.ProductName}: {product.TotalQuantity} unidades vendidas");
            }
            return sb.ToString();
        }
    }
}