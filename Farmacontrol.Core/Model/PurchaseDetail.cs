using Farmacontrol.Model;

namespace Farmacontrol.Core.Model;

public class PurchaseDetail
{
    public int Id { get; set; }
    public int PurchaseId { get; set; }
    public Purchase? Purchase { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public Product? Product { get; set; }
    public string LotCode { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitCost { get; set; }
    public DateTime ExpirationDate { get; set; }
    public decimal SubTotal => Quantity * UnitCost;

    private readonly List<ReceivedBatch> _receivedBatches = new();
    public IReadOnlyList<ReceivedBatch> ReceivedBatches => _receivedBatches.AsReadOnly();

    public void AddReceivedBatch(ReceivedBatch batch)
    {
        batch.PurchaseDetail = this;
        batch.PurchaseDetailId = Id;
        _receivedBatches.Add(batch);
    }

    public bool IsFullyReceived => ReceivedBatches.Sum(b => b.Quantity) >= Quantity;
    public int PendingQuantity => Quantity - ReceivedBatches.Sum(b => b.Quantity);

    private PurchaseDetail()
    {
    }

    public PurchaseDetail(Purchase purchase, string productCode, string lotCode, int quantity, decimal unitCost,
        DateTime expDate)
    {
        Purchase = purchase;
        ProductCode = productCode;
        LotCode = lotCode;
        Quantity = quantity;
        UnitCost = unitCost;
        ExpirationDate = expDate;
    }
}