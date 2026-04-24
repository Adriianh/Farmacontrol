namespace Farmacontrol.Model
{
    public class Supplier(string code, string name, string phoneNumber, string email, int leadTimeDays)
    {
        public string Code { get; set; } = code;
        public string Name { get; set; } = name;
        public string PhoneNumber { get; set; } = phoneNumber;
        public string Email { get; set; } = email;
        public int LeadTimeDays { get; set; } = leadTimeDays;

        public void ShowInformation()
        {
            Console.WriteLine($"Código: {Code}");
            Console.WriteLine($"Nombre: {Name}");
            Console.WriteLine($"Teléfono: {PhoneNumber}");
            Console.WriteLine($"Correo: {Email}");
            Console.WriteLine($"Tiempo de Entrega: {LeadTimeDays}");
        }

        public void PlaceOrder(List<Product> products)
        {
            List<Product> orderProducts = products
                .Where(product =>
                    product.SupplierCode == Code && product.IsStockLow()
                )
                .ToList();

            if (orderProducts.Count == 0)
            {
                Console.WriteLine($"No hay productos para ordenar al proveedor {Name}.");
                return;
            }

            Console.WriteLine($"=== PEDIDO PARA {Name.ToUpper()} ===");
            Console.WriteLine($"Teléfono: {PhoneNumber} | Correo: {Email}");
            Console.WriteLine($"Tiempo estimado de entrega: {LeadTimeDays} días");
            Console.WriteLine("----------------------");
            Console.WriteLine("Productos a ordenar:");
            foreach (Product product in orderProducts)
            {
                Console.WriteLine(
                    $"{product.Name} (Código: {product.Code}) - Stock Actual: {product.Stock}, Stock Mínimo: {product.MinimumStock}"
                    );
            }
        }
    }
}