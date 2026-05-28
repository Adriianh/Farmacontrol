namespace Farmacontrol.Model
{
    public class Batch
    {
        public int Id { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public Product? Product { get; set; }
        public string LotCode { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime ManufacturingDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public decimal UnitCost { get; set; }
        public DateTime ReceivedDate { get; set; } = DateTime.Now;
        public string? SupplierCode { get; set; }
        
        private Batch() { }
        
        public Batch(string productCode, string lotCode, int quantity, DateTime expDate, DateTime? mfgDate = null)
        {
            ProductCode = productCode;
            LotCode = lotCode;
            Quantity = quantity;
            ExpirationDate = expDate;
            ManufacturingDate = mfgDate ?? DateTime.Today;
        }
    }
}
