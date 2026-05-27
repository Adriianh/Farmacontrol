using Farmacontrol.Model;
using Farmacontrol.Repository;
using Microsoft.EntityFrameworkCore;

namespace Farmacontrol.Services
{
    public class SupplierManager
    {
        private readonly AppDbContext _db;
        private readonly AuditService _audit;

        public SupplierManager(AppDbContext db, AuditService audit)
        {
            _db = db;
            _audit = audit;
        }
        
        public void AddSupplier(Supplier supplier)
        {
            _db.Suppliers.Add(supplier);
            _db.SaveChanges();
            _audit.Log("Agregar Proveedor", $"Se agregó el proveedor '{supplier.Name}' (Código: {supplier.Code}).");
        }
        
        public void RemoveSupplier(string code)
        {
            var supplier = _db.Suppliers.Find(code);
            if (supplier != null)
            {
                _db.Suppliers.Remove(supplier);
                _db.SaveChanges();
                _audit.Log("Eliminar Proveedor", $"Se eliminó el proveedor '{supplier.Name}' (Código: {code}).");
            }
        }

        public Supplier? SearchSupplier(string query) =>
            _db.Suppliers.AsNoTracking().FirstOrDefault(supplier =>
                supplier.Code.Equals(query, StringComparison.OrdinalIgnoreCase) ||
                supplier.Name.Equals(query, StringComparison.OrdinalIgnoreCase)
            );

        public void GetAllSuppliers()
        {
            var suppliers = _db.Suppliers.AsNoTracking().ToList();
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
            var suppliers = _db.Suppliers.AsNoTracking().ToList();
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
        
        public IReadOnlyList<Supplier> GetSuppliers() => _db.Suppliers.AsNoTracking().ToList().AsReadOnly();
    }
}