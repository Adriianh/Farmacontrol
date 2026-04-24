using Farmacontrol.Model;

namespace Farmacontrol.Services
{
    public class SupplierManager
    {
        private readonly List<Supplier> _suppliers = new();
        
        public void AddSupplier(Supplier supplier) =>  _suppliers.Add(supplier);
        
        public void RemoveSupplier(string code)  =>  _suppliers.RemoveAll(x => x.Code == code);

        public Supplier? SearchSupplier(string query) =>
            _suppliers.FirstOrDefault(supplier =>
                supplier.Code.Equals(query, StringComparison.OrdinalIgnoreCase) ||
                supplier.Name.Equals(query, StringComparison.OrdinalIgnoreCase)
            );

        public void GetAllSuppliers()
        {
            if (_suppliers.Count == 0)
            {
                Console.WriteLine("No suppliers found.");
                return;
            }

            foreach (var supplier in _suppliers)
            {
                supplier.ShowInformation();
                Console.WriteLine("------------");
            }
        }

        public void GenerateAllOrders(List<Product> products)
        {
            if (_suppliers.Count == 0)
            {
                Console.WriteLine("No suppliers found.");
                return;
            }
            
            var orderFound = false;
            foreach (var supplier in _suppliers)
            {
                List<Product> productsToOrder = products
                    .Where(product => product.SupplierCode == supplier.Code && product.IsStockLow())
                    .ToList();

                if (productsToOrder.Count <= 0) continue;
                
                supplier.PlaceOrder(productsToOrder);
                Console.WriteLine($"Order {supplier.Code} has been placed.");
                orderFound = true;
            }
            
            if (!orderFound)
                Console.WriteLine("No hay pedidos pendientes.");
        }
        
        public IReadOnlyList<Supplier> GetSuppliers() => _suppliers.AsReadOnly();
    }
}