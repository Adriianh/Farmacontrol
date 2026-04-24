namespace Farmacontrol.Model
{
    public class Report(List<Sale> sales)
    {
        public void GenerateDailySales()
        {
            DateTime actualDate = DateTime.Today;
            List<Sale> todaySales = sales
                .Where(sale => sale.Date.Date == actualDate)
                .ToList();

            Console.WriteLine($"=== Ventas del {actualDate:dd/MM/yyyy} ===");
            foreach (var sale in todaySales)
                sale.ShowResume();

            decimal total = sales.Sum(sale => sale.Total);
            Console.WriteLine($"Total de ventas del día: Q{total:F2}");
        }

        public void GenerateMonthSales()
        {
            DateTime actualDate = DateTime.Today;
            List<Sale> monthSales = sales
                .Where(sale => sale.Date.Month == actualDate.Month && sale.Date.Year == actualDate.Year)
                .ToList();

            Console.WriteLine($"=== Ventas de {actualDate:MMMM yyyy} ===");
            foreach (var sale in monthSales)
                sale.ShowResume();

            decimal total = sales.Sum(sale => sale.Total);
            Console.WriteLine($"Total de ventas del mes: Q{total:F2}");
        }

        public void BestSellingProducts()
        {
            Console.WriteLine("=== Productos más vendidos ===");

            sales
                .SelectMany(sale => sale.GetDetails)
                .GroupBy(detail => detail.ProductName)
                .Select(group => new
                {
                    ProductName = group.Key,
                    TotalQuantity = group.Sum(detail => detail.Quantity)
                })
                .OrderByDescending(quantity => quantity.TotalQuantity)
                .Take(5)
                .ToList()
                .ForEach(product =>
                    Console.WriteLine($"{product.ProductName}: {product.TotalQuantity} unidades vendidas")
                    );
        }
    }
}