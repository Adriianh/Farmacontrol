namespace Farmacontrol.Model
{
    public class Supplier
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public int LeadTimeDays { get; set; }

        public Supplier(string code, string name, string phoneNumber, string email, int leadTimeDays)
        {
            Code = code;
            Name = name;
            PhoneNumber = phoneNumber;
            Email = email;
            LeadTimeDays = leadTimeDays;
        }

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