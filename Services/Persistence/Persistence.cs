using System.Text.Json;
using Farmacontrol.Model;

namespace Farmacontrol.Services.Persistence
{
    public class Persistence
    {
        private const string ProductsFile = "products.json";
        private const string SalesFile = "sales.json";
        private const string UsersFile = "users.json";
        private const string SuppliersFile = "suppliers.json";
        
        private JsonSerializerOptions Options => new()
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
            string json = JsonSerializer.Serialize(products, Options);
            File.WriteAllText(ProductsFile, json);
        }

        public void SaveSales(List<Sale> sales)
        {
            string json = JsonSerializer.Serialize(sales, Options);
            File.WriteAllText(SalesFile, json);
        }

        public void SaveUsers(List<User> users)
        {
            string json = JsonSerializer.Serialize(users, UserOptions);
            File.WriteAllText(UsersFile, json);
        }

        public void SaveSuppliers(List<Supplier> suppliers)
        {
            string json = JsonSerializer.Serialize(suppliers, Options);
            File.WriteAllText(SuppliersFile, json);
        }

        public List<Product> LoadProducts()
        {
            if (!File.Exists(ProductsFile))
                return new List<Product>();

            string json = File.ReadAllText(ProductsFile);
            return JsonSerializer.Deserialize<List<Product>>(json, Options) ?? new List<Product>();
        }

        public List<Sale> LoadSales()
        {
            if (!File.Exists(SalesFile))
                return new List<Sale>();

            string json = File.ReadAllText(SalesFile);
            return JsonSerializer.Deserialize<List<Sale>>(json, Options) ?? new List<Sale>();
        }

        public List<User> LoadUsers()
        {
            if (!File.Exists(UsersFile))
                return new List<User>();
            
            string json = File.ReadAllText(UsersFile);
            return JsonSerializer.Deserialize<List<User>>(json, UserOptions) ?? new List<User>();
        }

        public List<Supplier> LoadSuppliers()
        {
            if (!File.Exists(SuppliersFile))
                return new List<Supplier>();

            string json = File.ReadAllText(SuppliersFile);
            return JsonSerializer.Deserialize<List<Supplier>>(json, Options) ?? new List<Supplier>();
        }
    }
}