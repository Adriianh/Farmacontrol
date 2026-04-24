using Farmacontrol.Interface;
using Farmacontrol.Model;

namespace Farmacontrol.Services
{
    public class Inventory
    {
        private readonly List<Product> _products = new();

        public void AddProduct(Product product)
        {
            _products.Add(product);
        }

        public void RemoveProduct(Product product)
        {
            _products.Remove(product);
        }

        public Product? SearchProduct(String query) =>
            _products.FirstOrDefault(product =>
                product.Code.Equals(query, StringComparison.OrdinalIgnoreCase) ||
                product.Name.Equals(query, StringComparison.OrdinalIgnoreCase)
            );
        
        public void ListProducts()
        {
            foreach (var product in _products)
            {
                product.ShowInformation();
                Console.WriteLine("----------");
            }
        }

        public void GetAlerts()
        {
            foreach (var product in _products)
            {
                if (product is IAlertable alertable)
                {
                    alertable.VerifyAlert();
                }
            }
        }

        public void GetExpiredProducts()
        {
            foreach (var product in _products)
            {
                if (product is IExpirable expirable && expirable.IsExpired())
                {
                    Console.WriteLine($"Producto vencido: {product.Name} (Código: {product.Code})");
                }
            }
        }
    }
}