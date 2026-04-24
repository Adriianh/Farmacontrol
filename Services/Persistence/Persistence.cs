using System.Text.Json;
using Farmacontrol.Model;

namespace Farmacontrol.Services.Persistence
{
    public class Persistence
    {
        private const string ProductsArchive = "products.json";
        private const string SalesArchive = "sales.json";
        private const string UsersArchive = "users.json";
        private const string SuppliersArchive = "suppliers.json";
        
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
            File.WriteAllText(ProductsArchive, json);
        }

        public void SaveSales(List<Sale> sales)
        {
            string json = JsonSerializer.Serialize(sales, Options);
            File.WriteAllText(SalesArchive, json);
        }

        public void SaveUsers(List<User> users)
        {
            string json = JsonSerializer.Serialize(users, UserOptions);
            File.WriteAllText(UsersArchive, json);
        }

        public void SaveSuppliers(List<Supplier> suppliers)
        {
            string json = JsonSerializer.Serialize(suppliers, Options);
            File.WriteAllText(SuppliersArchive, json);
        }

        public List<Product> LoadProducts()
        {
            if (!File.Exists(ProductsArchive))
                return new List<Product>();

            string json = File.ReadAllText(ProductsArchive);
            return JsonSerializer.Deserialize<List<Product>>(json, Options) ?? new List<Product>();
        }

        public List<Sale> LoadSales()
        {
            if (!File.Exists(SalesArchive))
                return new List<Sale>();

            string json = File.ReadAllText(SalesArchive);
            return JsonSerializer.Deserialize<List<Sale>>(json, Options) ?? new List<Sale>();
        }

        public List<User> LoadUsers()
        {
            if (!File.Exists(UsersArchive))
                return new List<User>();
            
            string json = File.ReadAllText(UsersArchive);
            return JsonSerializer.Deserialize<List<User>>(json, UserOptions) ?? new List<User>();
        }

        public List<Supplier> LoadSuppliers()
        {
            if (!File.Exists(SuppliersArchive))
                return new List<Supplier>();

            string json = File.ReadAllText(SuppliersArchive);
            return JsonSerializer.Deserialize<List<Supplier>>(json, Options) ?? new List<Supplier>();
        }
    }
}