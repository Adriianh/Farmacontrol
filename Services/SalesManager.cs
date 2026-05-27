using Farmacontrol.Model;
using Farmacontrol.Repository;
using Microsoft.EntityFrameworkCore;

namespace Farmacontrol.Services
{
    public class SalesManager
    {
        private readonly AppDbContext _db;

        public SalesManager(AppDbContext db)
        {
            _db = db;
        }

        public int GetSalesCount() => _db.Sales.Any() ? _db.Sales.Max(s => s.Code) : 0;

        public IReadOnlyList<Sale> GetAllSales() => _db.Sales.Include(s => s.Details).ToList().AsReadOnly();

        public void RegisterSale(Sale sale)
        {
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                _db.Sales.Add(sale);
                
                foreach (var detail in sale.Details)
                {
                    var dbProduct = _db.Products.Find(detail.ProductCode);
                    if (dbProduct != null)
                    {
                        var entry = _db.Entry(dbProduct);
                        if (entry.State != EntityState.Modified)
                        {
                            dbProduct.UpdateStock(-detail.Quantity);
                        }
                    }
                }
                
                _db.SaveChanges();
                transaction.Commit();
            }
            catch (System.Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}
