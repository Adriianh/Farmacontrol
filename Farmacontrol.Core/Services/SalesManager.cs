using Farmacontrol.Core.Model;
using Farmacontrol.Core.Repository;
using Farmacontrol.Model;
using Microsoft.EntityFrameworkCore;

namespace Farmacontrol.Core.Services
{
    public class SalesManager
    {
        private readonly AppDbContext _db;
        private readonly AuditService _audit;

        public SalesManager(AppDbContext db, AuditService audit)
        {
            _db = db;
            _audit = audit;
        }

        public int GetSalesCount() => _db.Sales.Any() ? _db.Sales.Max(s => s.Code) : 0;

        public IReadOnlyList<Sale> GetAllSales() => _db.Sales.AsNoTracking().Include(s => s.Details).ToList().AsReadOnly();

        public void RegisterSale(Sale sale)
        {
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                _db.Sales.Add(sale);
                
                foreach (var detail in sale.Details)
                {
                    var dbProduct = _db.Products.Include(p => p.Batches).FirstOrDefault(p => p.Code == detail.ProductCode);
                    if (dbProduct == null) continue;
                    
                    int previousStock = dbProduct.Stock;
                    
                    var entry = _db.Entry(dbProduct);
                    if (entry.State != EntityState.Modified)
                    {
                        dbProduct.ReduceBatchStock(detail.Quantity);
                    }
                    
                    var movement = new InventoryMovement
                    {
                        ProductCode = dbProduct.Code,
                        Date = DateTime.Now,
                        Type = "Salida por venta",
                        Quantity = -detail.Quantity,
                        PreviousStock = previousStock,
                        NewStock = dbProduct.Stock,
                        Reason = "Venta a cliente",
                        Reference = $"Venta #{sale.Code}"
                    };
                    _db.InventoryMovements.Add(movement);
                }
                
                _db.SaveChanges();
                transaction.Commit();
                _audit.Log("Registrar Venta", $"Venta #{sale.Code} registrada con éxito. Total: Q{sale.Total:F2}");
            }
            catch (System.Exception)
            {
                transaction.Rollback();
                throw;
            }
        }

        public void VoidSale(int saleCode)
        {
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                var sale = _db.Sales.Include(s => s.Details).FirstOrDefault(s => s.Code == saleCode);
                if (sale == null || sale.IsVoided) return;

                sale.VoidSale();

                foreach (var detail in sale.Details)
                {
                    var dbProduct = _db.Products.Include(p => p.Batches).FirstOrDefault(p => p.Code == detail.ProductCode);
                    if (dbProduct != null)
                    {
                        int previousStock = dbProduct.Stock;
                        dbProduct.AddBatch("DEVOLUCION", detail.Quantity, DateTime.Today.AddYears(1));
                        
                        var movement = new InventoryMovement
                        {
                            ProductCode = dbProduct.Code,
                            Date = DateTime.Now,
                            Type = "Entrada por devolución",
                            Quantity = detail.Quantity,
                            PreviousStock = previousStock,
                            NewStock = dbProduct.Stock,
                            Reason = "Anulación de venta",
                            Reference = $"Venta #{sale.Code}"
                        };
                        _db.InventoryMovements.Add(movement);
                    }
                }

                _db.SaveChanges();
                transaction.Commit();
                _audit.Log("Anular Venta", $"Venta #{saleCode} ha sido anulada y el inventario restaurado.");
            }
            catch (System.Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}