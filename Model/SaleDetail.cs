namespace Farmacontrol.Model
{
    public class SaleDetail
    {
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }

        public SaleDetail(Product product, int quantity)
        {
            ProductCode = product.Code;
            ProductName = product.Name;
            Quantity = quantity;
            UnitPrice = product.Price;
            Subtotal = CalculateSubtotal();
        }

        private decimal CalculateSubtotal() => Quantity * UnitPrice;
        
        public void ShowDetails() =>
            Console.WriteLine($"{ProductName} (${ProductCode}) | Cantidad: {Quantity} | Subtotal: Q{Subtotal:F2}");
    }
}