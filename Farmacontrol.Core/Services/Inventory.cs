using Farmacontrol.Interface;
using Farmacontrol.Model;
using Farmacontrol.Repository;
using Microsoft.EntityFrameworkCore;

namespace Farmacontrol.Services
{
    public class Inventory
    {
        private readonly AppDbContext _db;
        private readonly AuditService _audit;

        public Inventory(AppDbContext db, AuditService audit)
        {
            _db = db;
            _audit = audit;
        }
        
        public IReadOnlyList<Product> GetProducts => _db.Products.AsNoTracking().Include(p => p.Suppliers).Include(p => p.Batches).ToList().AsReadOnly();

        public void AddProduct(Product product)
        {
            if (product.Suppliers.Count > 0)
            {
                var attachedSuppliers = new List<Supplier>();
                foreach (var sup in product.Suppliers)
                {
                    var dbSup = _db.Suppliers.Find(sup.Code);
                    if (dbSup != null)
                    {
                        attachedSuppliers.Add(dbSup);
                    }
                }
                product.Suppliers = attachedSuppliers;
            }

            _db.Products.Add(product);
            _db.SaveChanges();
            _audit.Log("Agregar Producto", $"Se agregó el producto '{product.Name}' (Código: {product.Code}) al inventario con stock inicial de {product.Stock}.");
        }

        public void RemoveProduct(Product product)
        {
            product.IsActive = false;
            _db.Products.Update(product);
            _db.SaveChanges();
            _audit.Log("Eliminar Producto", $"Se eliminó (borrado lógico) el producto '{product.Name}' (Código: {product.Code}) del inventario.");
        }

        public void UpdateProduct(Product product)
        {
            product.UpdatedAt = DateTime.Now;
            _db.Products.Update(product);
            _db.SaveChanges();
            _audit.Log("Modificar Producto", $"Se modificó el producto '{product.Name}' (Código: {product.Code}).");
        }

        public Product? SearchProduct(string query)
        {
            string normalizedQuery = query.ToLower();

            return _db.Products.AsNoTracking().Include(p => p.Suppliers).Include(p => p.Batches).FirstOrDefault(product =>
                product.Code.ToLower() == normalizedQuery ||
                product.Name.ToLower() == normalizedQuery
            );
        }

        public IReadOnlyList<Product> SearchProducts(string query)
        {
            string normalizedQuery = query.ToLower();

            return _db.Products.AsNoTracking().Include(p => p.Suppliers).Include(p => p.Batches).Where(product =>
                product.Code.ToLower().Contains(normalizedQuery) ||
                product.Name.ToLower().Contains(normalizedQuery)
            ).ToList().AsReadOnly();
        }
        
        public void ListProducts()
        {
            var products = GetProducts;
            foreach (var product in products)
            {
                product.ShowInformation();
                Console.WriteLine("-------------------");
            }
        }

        public void GetAlerts()
        {
            var products = _db.Products.AsNoTracking().Include(p => p.Suppliers).ToList();
            foreach (var product in products)
            {
                if (product is IAlertable alertable)
                {
                    alertable.VerifyAlert();
                }
            }
        }

        public void GetExpiredProducts()
        {
            var products = _db.Products.AsNoTracking().Include(p => p.Suppliers).ToList();
            foreach (var product in products)
            {
                if (product is IExpirable expirable && expirable.IsExpired())
                {
                    Console.WriteLine($"Producto vencido: {product.Name} (Código: {product.Code})");
                }
            }
        }

        public void DiscardBatch(string productCode, string lotCode, string reason)
        {
            var product = _db.Products.Include(p => p.Batches).FirstOrDefault(p => p.Code == productCode);
            if (product != null)
            {
                var batch = product.Batches.FirstOrDefault(b => b.LotCode == lotCode);
                if (batch != null && batch.Quantity > 0)
                {
                    int qty = batch.Quantity;
                    int previousStock = product.Stock;
                    
                    product.Stock -= qty;
                    batch.Quantity = 0;
                    
                    var movement = new InventoryMovement
                    {
                        ProductCode = product.Code,
                        Date = DateTime.Now,
                        Type = "Baja de inventario",
                        Quantity = -qty,
                        PreviousStock = previousStock,
                        NewStock = product.Stock,
                        Reason = reason,
                        Reference = lotCode
                    };
                    _db.InventoryMovements.Add(movement);
                    _db.SaveChanges();
                    _audit.Log("Baja de Inventario", $"Se dio de baja {qty} unidades del lote {lotCode} del producto '{product.Name}'. Motivo: {reason}");
                }
            }
        }

        public bool AssociateSupplier(string productCode, string supplierCode)
        {
            var product = _db.Products.Include(p => p.Suppliers).FirstOrDefault(p => p.Code == productCode);
            var supplier = _db.Suppliers.FirstOrDefault(s => s.Code == supplierCode);

            if (product != null && supplier != null)
            {
                if (product.Suppliers.Any(s => s.Code == supplierCode)) return false;
                
                product.Suppliers.Add(supplier);
                _db.SaveChanges();
                _audit.Log("Asociar Proveedor", $"Se asoció el proveedor '{supplier.Name}' al producto '{product.Name}'.");
                return true;
            }
            return false;
        }

        public void RegisterPurchase(Purchase purchase)
        {
            using var transaction = _db.Database.BeginTransaction();

            try
            {
                foreach (var detail in purchase.Details)
                {
                    var product = _db.Products
                        .Include(p => p.Batches)
                        .FirstOrDefault(p => p.Code == detail.ProductCode);

                    if (product != null)
                    {
                        int previousStock = product.Stock;
                        product.AddBatch(detail.LotCode, detail.Quantity, detail.ExpirationDate);
                        
                        var movement = new InventoryMovement
                        {
                            ProductCode = product.Code,
                            Date = DateTime.Now,
                            Type = "Entrada por compra",
                            Quantity = detail.Quantity,
                            PreviousStock = previousStock,
                            NewStock = product.Stock,
                            Reason = "Compra a proveedor",
                            Reference = $"FAC-{purchase.InvoiceNumber}"
                        };
                        _db.InventoryMovements.Add(movement);
                    }
                }

                _db.Purchases.Add(purchase);
                _db.SaveChanges();
                transaction.Commit();

                _audit.Log("Registrar Ingreso", $"Factura {purchase.InvoiceNumber} del proveedor {purchase.SupplierCode} registrada por Q{purchase.TotalCost:F2}");
            }
            catch (System.Exception)
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}