using Farmacontrol.Interface;
using Farmacontrol.Model;
using Farmacontrol.Repository;

namespace Farmacontrol.Services
{
    public class Inventory
    {
        private readonly AppDbContext _db;

        public Inventory(AppDbContext db)
        {
            _db = db;
        }
        
        public IReadOnlyList<Product> GetProducts => _db.Products.ToList().AsReadOnly();

        public void AddProduct(Product product)
        {
            _db.Products.Add(product);
            _db.SaveChanges();
        }

        public void RemoveProduct(Product product)
        {
            _db.Products.Remove(product);
            _db.SaveChanges();
        }

        public Product? SearchProduct(string query) =>
            _db.Products.FirstOrDefault(product =>
                product.Code.Equals(query, StringComparison.OrdinalIgnoreCase) ||
                product.Name.Equals(query, StringComparison.OrdinalIgnoreCase)
            );
        
        public void ListProducts()
        {
            var products = _db.Products.ToList();
            foreach (var product in products)
            {
                product.ShowInformation();
                Console.WriteLine("----------");
            }
        }

        public void GetAlerts()
        {
            var products = _db.Products.ToList();
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
            var products = _db.Products.ToList();
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