using Farmacontrol.Model;
using Farmacontrol.Repository;
using Microsoft.EntityFrameworkCore;

namespace Farmacontrol.Services.Persistence
{
    public class Persistence
    {
        private readonly AppDbContext _db;
        
        public Persistence(string? dataDirectory = null)
        {
            string baseDirectory;
            if (!string.IsNullOrWhiteSpace(dataDirectory))
            {
                baseDirectory = Path.IsPathRooted(dataDirectory)
                    ? dataDirectory
                    : Path.GetFullPath(Path.Combine(GetProjectRootDirectory(), dataDirectory));
            }
            else
            {
                baseDirectory = Path.Combine(GetProjectRootDirectory(), "Data");
            }
            Directory.CreateDirectory(baseDirectory);
            _db = new AppDbContext();
            _db.Database.EnsureCreated();
        }
        
        public void SaveProducts(List<Product> products)
        {
            var existing = _db.Products.ToList();
            var toDelete = existing.Except(products).ToList();
            _db.Products.RemoveRange(toDelete);
            var toAdd = products.Except(existing).ToList();
            _db.Products.AddRange(toAdd);
            _db.SaveChanges();
        }
        
        public void SaveSales(List<Sale> sales)
        {
            var existing = _db.Sales.Include(s => s.Details).ToList();
            var toDelete = existing.Except(sales).ToList();
            _db.Sales.RemoveRange(toDelete);
            var toAdd = sales.Except(existing).ToList();
            _db.Sales.AddRange(toAdd);
            _db.SaveChanges();
        }
        
        public void SaveUsers(List<User> users)
        {
            try
            {
                var existing = _db.Users.ToList();
                var toDelete = existing.Except(users).ToList();
                _db.Users.RemoveRange(toDelete);
                var toAdd = users.Except(existing).ToList();
                _db.Users.AddRange(toAdd);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("No se pudieron guardar los usuarios.", ex);
            }
        }
        
        public void SaveSuppliers(List<Supplier> suppliers)
        {
            var existing = _db.Suppliers.ToList();
            var toDelete = existing.Except(suppliers).ToList();
            _db.Suppliers.RemoveRange(toDelete);
            var toAdd = suppliers.Except(existing).ToList();
            _db.Suppliers.AddRange(toAdd);
            _db.SaveChanges();
        }
        
        public void SaveHistory(List<Alert> alerts)
        {
            var existing = _db.Alerts.ToList();
            var toDelete = existing.Except(alerts).ToList();
            _db.Alerts.RemoveRange(toDelete);
            var toAdd = alerts.Except(existing).ToList();
            _db.Alerts.AddRange(toAdd);
            _db.SaveChanges();
        }
        
        public List<Product> LoadProducts() => _db.Products.ToList();
        public List<Sale> LoadSales() => _db.Sales.Include(s => s.Details).ToList();
        public List<User> LoadUsers() => _db.Users.ToList();
        public List<Supplier> LoadSuppliers() => _db.Suppliers.ToList();
        public List<Alert> LoadHistory() => _db.Alerts.ToList();
        
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
