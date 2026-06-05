using Farmacontrol.Model;

namespace Farmacontrol.Core.Model
{
    public class Supplier
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int LeadTimeDays { get; set; }

        public string? TaxId { get; set; }
        public string? PaymentTerms { get; set; }
        public bool IsActive { get; set; } = true;

        public string? ContactName { get; set; }
        public string? Address { get; set; }

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
            if (!string.IsNullOrEmpty(TaxId)) Console.WriteLine($"NIT/ID Fiscal: {TaxId}");
            Console.WriteLine($"Teléfono: {PhoneNumber}");
            Console.WriteLine($"Correo: {Email}");
            if (!string.IsNullOrEmpty(ContactName)) Console.WriteLine($"Contacto: {ContactName}");
            if (!string.IsNullOrEmpty(Address)) Console.WriteLine($"Dirección: {Address}");
            if (!string.IsNullOrEmpty(PaymentTerms)) Console.WriteLine($"Condiciones de pago: {PaymentTerms}");
            Console.WriteLine($"Tiempo de Entrega: {LeadTimeDays} días");
            Console.WriteLine($"Estado: {(IsActive ? "Activo" : "Inactivo")}");
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

            Console.WriteLine("\n=======================================================");
            Console.WriteLine($"                 PEDIDO DE REABASTECIMIENTO            ");
            Console.WriteLine("=======================================================");
            Console.WriteLine($" PROVEEDOR : {Name.ToUpper()} ({Code})");
            Console.WriteLine($" TELÉFONO  : {PhoneNumber}");
            Console.WriteLine($" CORREO    : {Email}");
            if (!string.IsNullOrEmpty(ContactName)) Console.WriteLine($" CONTACTO  : {ContactName}");
            if (!string.IsNullOrEmpty(Address))     Console.WriteLine($" ENVÍO A   : {Address}");
            Console.WriteLine($" TIEMPO EST: {LeadTimeDays} días hábiles");
            Console.WriteLine("=======================================================\n");

            Console.WriteLine($"{"CÓDIGO",-8} | {"PRODUCTO",-25} | {"STOCK",-6} | {"MÍNIMO",-6} | {"A PEDIR"}");
            Console.WriteLine(new string('-', 65));
            foreach (Product p in orderProducts)
            {
                string name = p.Name.Length > 23 ? p.Name.Substring(0, 23) + ".." : p.Name;
                int toOrder = (p.MinimumStock * 2) - p.Stock; // Just an example formula for the table
                if (toOrder <= 0) toOrder = p.MinimumStock; // Fallback
                Console.WriteLine($"{p.Code,-8} | {name,-25} | {p.Stock,-6} | {p.MinimumStock,-6} | {toOrder} und.");
            }
            Console.WriteLine(new string('-', 65));
            Console.WriteLine(" * Fin del documento de pedido *\n");
        }
    }
}