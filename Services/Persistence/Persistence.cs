using Farmacontrol.Exception;
using Farmacontrol.Model;
using Farmacontrol.Repository;
using Microsoft.EntityFrameworkCore;

namespace Farmacontrol.Services.Persistence
{
    public class Persistence
    {
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
            using var db = new AppDbContext();
            db.Database.EnsureCreated();
        }
        
        public void SaveProducts(List<Product> products)
        {
            using var db = new AppDbContext();
            var distinctProducts = products.GroupBy(p => p.Code).Select(g => g.First()).ToList();
            var existingCodes = db.Products.Select(p => p.Code).ToList();
            var toDelete = existingCodes.Except(distinctProducts.Select(p => p.Code)).ToList();
            
            foreach (var code in toDelete)
            {
                var p = db.Products.Find(code);
                if (p != null) db.Products.Remove(p);
            }
            
            foreach (var product in distinctProducts)
            {
                var existing = db.Products.Find(product.Code);
                if (existing != null)
                {
                    db.Entry(existing).CurrentValues.SetValues(product);
                }
                else
                {
                    db.Products.Add(product);
                }
            }
            
            db.SaveChanges();
        }
        
        public void SaveSales(List<Sale> sales)
        {
            using var db = new AppDbContext();
            var distinctSales = sales.GroupBy(s => s.Code).Select(g => g.First()).ToList();
            var existingCodes = db.Sales.Select(s => s.Code).ToList();
            var toDelete = existingCodes.Except(distinctSales.Select(s => s.Code)).ToList();
            
            foreach (var code in toDelete)
            {
                var s = db.Sales.Include(sale => sale.Details).FirstOrDefault(sale => sale.Code == code);
                if (s != null) db.Sales.Remove(s);
            }
            
            foreach (var sale in distinctSales)
            {
                var existing = db.Sales.Include(s => s.Details).FirstOrDefault(s => s.Code == sale.Code);
                if (existing != null)
                {
                    db.Entry(existing).CurrentValues.SetValues(sale);
                    // Simplify: just remove old details and add new ones
                    db.SaleDetails.RemoveRange(existing.Details);
                    foreach(var detail in sale.Details) {
                        existing.Details.Add(detail);
                    }
                }
                else
                {
                    db.Sales.Add(sale);
                }
            }
            
            db.SaveChanges();
        }
        
        public void SaveUsers(List<User> users)
        {
            try
            {
                using var db = new AppDbContext();
                var distinctUsers = users.GroupBy(u => u.Username).Select(g => g.First()).ToList();
                var existingNames = db.Users.Select(u => u.Username).ToList();
                var toDelete = existingNames.Except(distinctUsers.Select(u => u.Username)).ToList();
                
                foreach (var name in toDelete)
                {
                    var u = db.Users.Find(name);
                    if (u != null) db.Users.Remove(u);
                }
                
                foreach (var user in distinctUsers)
                {
                    var existing = db.Users.Find(user.Username);
                    if (existing != null)
                    {
                        if (existing.Role != user.Role)
                        {
                            db.Users.Remove(existing);
                            db.SaveChanges(); // to avoid conflict
                            db.Users.Add(user);
                        }
                        else
                        {
                            db.Entry(existing).CurrentValues.SetValues(user);
                        }
                    }
                    else
                    {
                        db.Users.Add(user);
                    }
                }
                
                db.SaveChanges();
            }
            catch (System.Exception ex)
            {
                throw new PersistenceOperationException("No se pudieron guardar los usuarios en la base de datos.", ex);
            }
        }

        public void SaveSuppliers(List<Supplier> suppliers)
        {
            using var db = new AppDbContext();
            var distinctSuppliers = suppliers.GroupBy(s => s.Code).Select(g => g.First()).ToList();
            var existingCodes = db.Suppliers.Select(s => s.Code).ToList();
            var toDelete = existingCodes.Except(distinctSuppliers.Select(s => s.Code)).ToList();
            
            foreach (var code in toDelete)
            {
                var s = db.Suppliers.Find(code);
                if (s != null) db.Suppliers.Remove(s);
            }
            
            foreach (var supplier in distinctSuppliers)
            {
                var existing = db.Suppliers.Find(supplier.Code);
                if (existing != null)
                {
                    db.Entry(existing).CurrentValues.SetValues(supplier);
                }
                else
                {
                    db.Suppliers.Add(supplier);
                }
            }
            
            db.SaveChanges();
        }
        
        public void SaveHistory(List<Alert> alerts)
        {
            using var db = new AppDbContext();
            db.Alerts.RemoveRange(db.Alerts);
            db.Alerts.AddRange(alerts);
            db.SaveChanges();
        }
        
        public List<Product> LoadProducts() 
        {
            using var db = new AppDbContext();
            return db.Products.ToList();
        }
        
        public List<Sale> LoadSales()
        {
            using var db = new AppDbContext();
            return db.Sales.Include(s => s.Details).ToList();
        }
        
        public List<User> LoadUsers()
        {
            using var db = new AppDbContext();
            return db.Users.ToList();
        }
        
        public List<Supplier> LoadSuppliers()
        {
            using var db = new AppDbContext();
            return db.Suppliers.ToList();
        }
        
        public List<Alert> LoadHistory()
        {
            using var db = new AppDbContext();
            return db.Alerts.ToList();
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