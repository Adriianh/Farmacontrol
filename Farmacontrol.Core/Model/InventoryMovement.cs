using Farmacontrol.Model;

namespace Farmacontrol.Core.Model
{
    public class InventoryMovement
    {
        public int Id { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public Product? Product { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Type { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int PreviousStock { get; set; }
        public int NewStock { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string? Reference { get; set; }
    }
}