using System.Text.Json;
using Farmacontrol.Model;

namespace Farmacontrol.Services.Persistence
{
    public class Persistence
    {
        private const string ProductsArchive = "products.json";
        private const string SalesArchive = "sales.json";

        private JsonSerializerOptions Options => new()
        {
            WriteIndented = true,
            Converters = { new Converter.ProductConverter() }
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
    }
}