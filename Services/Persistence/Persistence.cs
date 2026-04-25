using System.Text.Json;
using Farmacontrol.Model;
using Farmacontrol.Services.Converter;

namespace Farmacontrol.Services.Persistence
{
    public class Persistence
    {
        private const string ProductsFile = "products.json";
        private const string SalesFile = "sales.json";
        private const string UsersFile = "users.json";
        private const string SuppliersFile = "suppliers.json";
        private const string HistoryFile = "history.json";

        private const string DefaultDataFolderName = "Data";

        private readonly string _baseDirectory;

        public string BaseDirectory => _baseDirectory;

        private readonly JsonSerializerOptions _options = new()
        {
            WriteIndented = true,
            Converters = { new ProductConverter() }
        };

        private readonly JsonSerializerOptions _userOptions = new()
        {
            WriteIndented = true,
            Converters = { new UserConverter() }
        };

        /// <summary>
        /// Create a Persistence instance. If dataDirectory is null, a default "Data" folder
        /// inside the project root directory will be used.
        /// </summary>
        /// <param name="dataDirectory">Optional absolute or relative path for data files.</param>
        public Persistence(string? dataDirectory = null)
        {
            if (!string.IsNullOrWhiteSpace(dataDirectory))
            {
                _baseDirectory = Path.IsPathRooted(dataDirectory)
                    ? dataDirectory
                    : Path.GetFullPath(Path.Combine(GetProjectRootDirectory(), dataDirectory));
            }
            else
            {
                _baseDirectory = Path.Combine(GetProjectRootDirectory(), DefaultDataFolderName);
            }

            Directory.CreateDirectory(_baseDirectory);
        }

        public void SaveProducts(List<Product> products)
        {
            string json = JsonSerializer.Serialize(products, _options);
            File.WriteAllText(GetFilePath(ProductsFile), json);
        }

        public void SaveSales(List<Sale> sales)
        {
            string json = JsonSerializer.Serialize(sales, _options);
            File.WriteAllText(GetFilePath(SalesFile), json);
        }
        
        public void SaveUsers(List<User> users)
        {
            try
            {
                string json = JsonSerializer.Serialize(users, _userOptions);
                File.WriteAllText(GetFilePath(UsersFile), json);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudieron guardar los usuarios.", ex);
            }
        }

        public void SaveSuppliers(List<Supplier> suppliers)
        {
            string json = JsonSerializer.Serialize(suppliers, _options);
            File.WriteAllText(GetFilePath(SuppliersFile), json);
        }

        public void SaveHistory(List<Alert> alerts)
        {
            string json = JsonSerializer.Serialize(alerts, _options);
            File.WriteAllText(GetFilePath(HistoryFile), json);
        }

        public List<Product> LoadProducts()
        {
            var path = GetFilePath(ProductsFile);
            if (!File.Exists(path))
                return new List<Product>();

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<Product>>(json, _options) ?? new List<Product>();
        }

        public List<Sale> LoadSales()
        {
            var path = GetFilePath(SalesFile);
            if (!File.Exists(path))
                return new List<Sale>();

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<Sale>>(json, _options) ?? new List<Sale>();
        }

        public List<User> LoadUsers()
        {
            var path = GetFilePath(UsersFile);
            if (!File.Exists(path))
                return new List<User>();

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<User>>(json, _userOptions) ?? new List<User>();
        }

        public List<Supplier> LoadSuppliers()
        {
            var path = GetFilePath(SuppliersFile);
            if (!File.Exists(path))
                return new List<Supplier>();

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<Supplier>>(json, _options) ?? new List<Supplier>();
        }

        public List<Alert> LoadHistory()
        {
            var path = GetFilePath(HistoryFile);
            if (!File.Exists(path))
                return new List<Alert>();

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<Alert>>(json, _options) ?? new List<Alert>();
        }

        private string GetFilePath(string fileName)
        {
            return Path.Combine(_baseDirectory, fileName);
        }

        private static string GetProjectRootDirectory()
        {
            DirectoryInfo? directory = new(AppContext.BaseDirectory);

            while (directory != null)
            {
                if (directory.GetFiles("*.csproj").Any())
                    return directory.FullName;

                directory = directory.Parent;
            }

            return Directory.GetCurrentDirectory();
        }
    }
}