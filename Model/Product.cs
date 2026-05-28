using Farmacontrol.Exception;

namespace Farmacontrol.Model
{
    public abstract class Product
    {
        public string Name { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public int Stock { get; set; }

        public int MinimumStock { get; set; }

        public string? Barcode { get; set; }
        public string? Location { get; set; }
        public string? Laboratory { get; set; }

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        public ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();

        public ICollection<Batch> Batches { get; set; } = new List<Batch>();

        public abstract string GetDescription();

        public bool IsStockLow()
        {
            return Stock < MinimumStock;
        }

        public void UpdateStock(int quantity)
        {
            if (Stock + quantity < 0)
                throw new InsufficientStockException(Name, Math.Abs(quantity), Stock);

            Stock += quantity;
        }

        public void AddBatch(string lotCode, int quantity, DateTime expirationDate)
        {
            var batch = Batches.FirstOrDefault(b => b.LotCode == lotCode);
            if (batch != null)
            {
                batch.Quantity += quantity;
            }
            else
            {
                Batches.Add(new Batch(Code, lotCode, quantity, expirationDate));
            }

            Stock += quantity;
        }

        public void ReduceBatchStock(int quantity)
        {
            if (Stock < quantity)
                throw new InsufficientStockException(Name, quantity, Stock);

            var batchesToReduce = Batches.Where(b => b.Quantity > 0).OrderBy(b => b.ExpirationDate).ToList();
            int remaining = quantity;

            foreach (var batch in batchesToReduce.TakeWhile(_ => remaining != 0))
            {
                if (batch.Quantity >= remaining)
                {
                    batch.Quantity -= remaining;
                    remaining = 0;
                }
                else
                {
                    remaining -= batch.Quantity;
                    batch.Quantity = 0;
                }
            }

            Stock -= quantity;
        }

        public void ShowInformation()
        {
            Console.WriteLine($"Nombre: {Name}");
            Console.WriteLine($"Código: {Code}");
            Console.WriteLine($"Precio: Q{Price:F2}");
            Console.WriteLine($"Stock: {Stock}");
            if (!string.IsNullOrEmpty(Barcode)) Console.WriteLine($"Código de Barras: {Barcode}");
            if (!string.IsNullOrEmpty(Location)) Console.WriteLine($"Ubicación: {Location}");
            if (!string.IsNullOrEmpty(Laboratory)) Console.WriteLine($"Laboratorio: {Laboratory}");
            if (Suppliers.Count > 0)
            {
                var supplierNames = string.Join(", ", Enumerable.Select(Suppliers, s => s.Name));
                Console.WriteLine($"Proveedores: {supplierNames}");
            }

            Console.WriteLine(GetDescription());
        }
    }
}