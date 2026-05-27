namespace Farmacontrol.Model
{
    public class Sale
    {
        private readonly List<SaleDetail> _details = new();
        
        public int Code { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public decimal Total { get; set; }

        public Sale(int code)
        {
            Code = code;
        }

        public IReadOnlyList<SaleDetail> GetDetails => _details.AsReadOnly();

        public List<SaleDetail> Details => _details;

        public void AddDetail(Product product, int quantity)
        {
            product.UpdateStock(-quantity);
            _details.Add(new SaleDetail(product, quantity));
            Total = CalculateTotal();
        }

        private decimal CalculateTotal() => _details.Sum(detail => detail.Subtotal);

        public void ShowResume()
        {
            Console.WriteLine($"Venta #{Code} - {Date:dd/MM/yyyy}");
            Console.WriteLine("------------------");

            foreach (var detail in _details)
            {
                detail.ShowDetails();
            }

            Console.WriteLine("------------------");
            Console.WriteLine($"Total: Q{Total:F2}");
        }
    }
}