namespace Farmacontrol.Model
{
    public class SaleDetail
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal { get; set; }

        public SaleDetail(Product product, int quantity)
        {
            Product = product;
            Quantity = quantity;
            UnitPrice = product.Price;
            Subtotal = CalculateSubtotal();
        }
        
        private decimal CalculateSubtotal() => Quantity * UnitPrice;
        
        public void ShowDetails() =>
            Console.WriteLine($"{Product.GetDescription()} | Cantidad: {Quantity} | Subtotal: Q{Subtotal:F2}");
    }
}