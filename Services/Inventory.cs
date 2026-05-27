using Farmacontrol.Interface;
using Farmacontrol.Model;
using Farmacontrol.Repository;
using Microsoft.EntityFrameworkCore;

namespace Farmacontrol.Services
{
    public class Inventory
    {
        private readonly AppDbContext _db;
        private readonly AuditService _audit;

        public Inventory(AppDbContext db, AuditService audit)
        {
            _db = db;
            _audit = audit;
        }
        
        public IReadOnlyList<Product> GetProducts => _db.Products.AsNoTracking().ToList().AsReadOnly();

        public void AddProduct(Product product)
        {
            _db.Products.Add(product);
            _db.SaveChanges();
            _audit.Log("Agregar Producto", $"Se agregó el producto '{product.Name}' (Código: {product.Code}) al inventario con stock inicial de {product.Stock}.");
        }

        public void RemoveProduct(Product product)
        {
            _db.Products.Remove(product);
            _db.SaveChanges();
            _audit.Log("Eliminar Producto", $"Se eliminó el producto '{product.Name}' (Código: {product.Code}) del inventario.");
        }

        public Product? SearchProduct(string query) =>
            _db.Products.AsNoTracking().FirstOrDefault(product =>
                product.Code.Equals(query, StringComparison.OrdinalIgnoreCase) ||
                product.Name.Equals(query, StringComparison.OrdinalIgnoreCase)
            );
        
        public void ListProducts()
        {
            var products = _db.Products.AsNoTracking().ToList();
            foreach (var product in products)
            {
                product.ShowInformation();
                Console.WriteLine("----------");
            }
        }

        public void GetAlerts()
        {
            var products = _db.Products.AsNoTracking().ToList();
            foreach (var product in products)
            {
                if (product is IAlertable alertable)
                {
                    alertable.VerifyAlert();
                }
            }
        }

        public void GetExpiredProducts()
        {
            var products = _db.Products.AsNoTracking().ToList();
            foreach (var product in products)
            {
                if (product is IExpirable expirable && expirable.IsExpired())
                {
                    Console.WriteLine($"Producto vencido: {product.Name} (Código: {product.Code})");
                }
            }
        }
    }
}