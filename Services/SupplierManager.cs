using Farmacontrol.Model;
using Farmacontrol.Repository;

namespace Farmacontrol.Services
{
    public class SupplierManager
    {
        private readonly AppDbContext _db;

        public SupplierManager(AppDbContext db)
        {
            _db = db;
        }
        
        public void AddSupplier(Supplier supplier)
        {
            _db.Suppliers.Add(supplier);
            _db.SaveChanges();
        }
        
        public void RemoveSupplier(string code)
        {
            var supplier = _db.Suppliers.Find(code);
            if (supplier != null)
            {
                _db.Suppliers.Remove(supplier);
                _db.SaveChanges();
            }
        }

        public Supplier? SearchSupplier(string query) =>
            _db.Suppliers.FirstOrDefault(supplier =>
                supplier.Code.Equals(query, StringComparison.OrdinalIgnoreCase) ||
                supplier.Name.Equals(query, StringComparison.OrdinalIgnoreCase)
            );

        public void GetAllSuppliers()
        {
            var suppliers = _db.Suppliers.ToList();
            if (suppliers.Count == 0)
            {
                Console.WriteLine("No suppliers found.");
                return;
            }

            foreach (var supplier in suppliers)
            {
                supplier.ShowInformation();
                Console.WriteLine("------------");
            }
        }

        public void GenerateAllOrders(List<Product> products)
        {
            var suppliers = _db.Suppliers.ToList();
            if (suppliers.Count == 0)
            {
                Console.WriteLine("No suppliers found.");
                return;
            }
            
            var orderFound = false;
            foreach (var supplier in suppliers)
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
        
        public IReadOnlyList<Supplier> GetSuppliers() => _db.Suppliers.ToList().AsReadOnly();
    }
}