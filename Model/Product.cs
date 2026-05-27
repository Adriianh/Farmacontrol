using System;
using System.Collections.Generic;
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

        // Nuevos atributos de farmacia
        public string? Barcode { get; set; }
        public string? Location { get; set; }
        public string? Laboratory { get; set; }

        // Relación muchos a muchos con proveedores
        public ICollection<Supplier> Suppliers { get; set; } = new List<Supplier>();

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
                var supplierNames = string.Join(", ", System.Linq.Enumerable.Select(Suppliers, s => s.Name));
                Console.WriteLine($"Proveedores: {supplierNames}");
            }
            Console.WriteLine(GetDescription());
        }
    }
}