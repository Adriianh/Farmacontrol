using Farmacontrol.Model;

namespace Farmacontrol.Core.Model
{
    public enum PurchaseStatus { Pending, Received, Cancelled }
    
    public class Purchase
    {
        public int Id { get; set; }
        public string SupplierCode { get; set; } = string.Empty;
        public Supplier? Supplier { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public decimal TotalCost { get; set; }
        public string InvoiceNumber { get; set; } = string.Empty;
        private readonly List<PurchaseDetail> _details = new();
        public IReadOnlyList<PurchaseDetail> Details => _details.AsReadOnly();
        public PurchaseStatus Status { get; set; } = PurchaseStatus.Pending;
        
        private Purchase() { }
        
        public Purchase(string supplierCode, string invoiceNumber)
        {
            SupplierCode = supplierCode;
            InvoiceNumber = invoiceNumber;
        }
        
        public void AddDetail(Product product, string lotCode, int quantity, decimal unitCost, DateTime expDate)
        {
            var detail = new PurchaseDetail(this, product.Code, lotCode, quantity, unitCost, expDate);
            _details.Add(detail);
            TotalCost = _details.Sum(d => d.SubTotal);
        }
        
        public void ConfirmReception()
        {
            if (Status == PurchaseStatus.Received) return;
            Status = PurchaseStatus.Received;
        }
    }
}