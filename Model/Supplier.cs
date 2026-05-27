using System;
using System.Collections.Generic;
using System.Linq;

namespace Farmacontrol.Model
{
    public class Supplier
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int LeadTimeDays { get; set; }

        // Nuevos atributos de proveedor
        public string? ContactName { get; set; }
        public string? Address { get; set; }

        // Relación muchos a muchos con productos
        public ICollection<Product> Products { get; set; } = new List<Product>();

        public Supplier(string code, string name, string phoneNumber, string email, int leadTimeDays, string? contactName = null, string? address = null)
        {
            Code = code;
            Name = name;
            PhoneNumber = phoneNumber;
            Email = email;
            LeadTimeDays = leadTimeDays;
            ContactName = contactName;
            Address = address;
        }

        private Supplier() { }

        public void ShowInformation()
        {
            Console.WriteLine($"Código: {Code}");
            Console.WriteLine($"Nombre: {Name}");
            Console.WriteLine($"Teléfono: {PhoneNumber}");
            Console.WriteLine($"Correo: {Email}");
            if (!string.IsNullOrEmpty(ContactName)) Console.WriteLine($"Contacto: {ContactName}");
            if (!string.IsNullOrEmpty(Address)) Console.WriteLine($"Dirección: {Address}");
            Console.WriteLine($"Tiempo de Entrega: {LeadTimeDays} días");
        }

        public void PlaceOrder(List<Product> products)
        {
            List<Product> orderProducts = products
                .Where(product =>
                    product.Suppliers.Any(s => s.Code == Code) && product.IsStockLow()
                )
                .ToList();

            if (orderProducts.Count == 0)
            {
                Console.WriteLine($"No hay productos para ordenar al proveedor {Name}.");
                return;
            }

            Console.WriteLine($"=== PEDIDO PARA {Name.ToUpper()} ===");
            Console.WriteLine($"Teléfono: {PhoneNumber} | Correo: {Email}");
            if (!string.IsNullOrEmpty(ContactName)) Console.WriteLine($"Contacto de ventas: {ContactName}");
            if (!string.IsNullOrEmpty(Address)) Console.WriteLine($"Dirección de envío: {Address}");
            Console.WriteLine($"Tiempo estimado de entrega: {LeadTimeDays} días");
            Console.WriteLine("----------------------");
            Console.WriteLine("Productos a ordenar:");
            foreach (Product product in orderProducts)
            {
                Console.WriteLine(
                    $"- {product.Name} (Código: {product.Code}) - Stock Actual: {product.Stock}, Stock Mínimo: {product.MinimumStock}"
                );
            }
        }
    }
}