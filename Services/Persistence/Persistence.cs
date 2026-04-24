using System.Text.Json;
using Farmacontrol.Model;

namespace Farmacontrol.Services.Persistence
{
    public class Persistence
    {
        private const string ProductsArchive = "products.json";
        private const string SalesArchive = "sales.json";
        private const string UsersArchive = "users.json";

        private JsonSerializerOptions ProductOptions => new()
        {
            WriteIndented = true,
            Converters = { new Converter.ProductConverter() }
        };

        private JsonSerializerOptions UserOptions => new()
        {
            WriteIndented = true,
            Converters = { new Converter.UserConverter() }
        };

        public void SaveProducts(List<Product> products)
        {
            string json = JsonSerializer.Serialize(products, ProductOptions);
            File.WriteAllText(ProductsArchive, json);
        }

        public void SaveSales(List<Sale> sales)
        {
            string json = JsonSerializer.Serialize(sales, ProductOptions);
            File.WriteAllText(SalesArchive, json);
        }

        public void SaveUsers(List<User> users)
        {
            string json = JsonSerializer.Serialize(users, UserOptions);
            File.WriteAllText(UsersArchive, json);
        }

        public List<Product> LoadProducts()
        {
            if (!File.Exists(ProductsArchive))
                return new List<Product>();

            string json = File.ReadAllText(ProductsArchive);
            return JsonSerializer.Deserialize<List<Product>>(json, ProductOptions) ?? new List<Product>();
        }

        public List<Sale> LoadSales()
        {
            if (!File.Exists(SalesArchive))
                return new List<Sale>();

            string json = File.ReadAllText(SalesArchive);
            return JsonSerializer.Deserialize<List<Sale>>(json, ProductOptions) ?? new List<Sale>();
        }

        public List<User> LoadUsers()
        {
            if (!File.Exists(UsersArchive))
                return new List<User>();
            
            string json = File.ReadAllText(UsersArchive);
            return JsonSerializer.Deserialize<List<User>>(json, UserOptions) ?? new List<User>();
        }
    }
}