namespace Farmacontrol.Core.Model;

public class ReceivedBatch
{
    public int Id { get; set; }
    public int PurchaseDetailId { get; set; }
    public PurchaseDetail? PurchaseDetail { get; set; }
        
    public string LotCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime ManufacturingDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public decimal UnitCost { get; set; }
        
    public DateTime ReceivedAt { get; set; } = DateTime.Now;
        
    public string? ExistingBatchId { get; set; }
}