using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Farmacontrol.Core.Model;
using Farmacontrol.Core.Repository;
using Microsoft.EntityFrameworkCore;
using static System.Decimal;

namespace Farmacontrol.Desktop.States;

public partial class ReceivePurchaseState : ObservableObject
{
    private readonly Purchase _purchase;
    private readonly AppDbContext _db;

    [ObservableProperty] private ObservableCollection<PurchaseProductState> _productItems = new();

    [ObservableProperty] private PurchaseProductState? _selectedProduct;
    [ObservableProperty] private string _lotCode = string.Empty;
    [ObservableProperty] private string _quantity = string.Empty;
    [ObservableProperty] private DateTimeOffset? _expirationDate = DateTime.Today.AddYears(1);
    [ObservableProperty] private DateTimeOffset? _manufacturingDate = DateTime.Today;
    [ObservableProperty] private string _unitCost = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLotMode))]
    [NotifyPropertyChangedFor(nameof(IsManualMode))]
    private bool _isManualStockMode;

    public bool IsLotMode => !IsManualStockMode;
    public bool IsManualMode => IsManualStockMode;
    public string IsManualModeTitle => IsManualStockMode ? "🏷️ Cambiar a modo Lote" : "📊 Cambiar a Stock Manual";

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasQuantityWarning))]
    private string _quantityWarning = string.Empty;
    public bool HasQuantityWarning => !string.IsNullOrEmpty(QuantityWarning);

    public string PurchaseInfo => $"Pedido: {_purchase.InvoiceNumber}";
    public string SupplierName => _purchase.SupplierCode;
    public DateTime PurchaseDate => _purchase.Date;

    public int TotalProductsCount => ProductItems.Count;
    public int FullyReceivedCount => ProductItems.Count(x => x.IsComplete);
    public int PendingCount => TotalProductsCount - FullyReceivedCount;
    public bool CanComplete => PendingCount == 0;
    public string ProgressText => $"{FullyReceivedCount} de {TotalProductsCount} productos recibidos";

    public ReceivePurchaseState(Purchase purchase, AppDbContext db)
    {
        _purchase = purchase;
        _db = db;
        LoadProducts();
    }

    private void LoadProducts()
    {
        _db.Entry(_purchase)
            .Collection(p => p.Details)
            .Query()
            .Include(d => d.Product)
            .Include(d => d.ReceivedBatches)
            .Load();

            var items = _purchase.Details.Select(d => new PurchaseProductState
        {
            PurchaseDetailId = d.Id,
            ProductCode = d.ProductCode,
            ProductName = d.Product?.Name ?? "Producto Desconocido",
            TotalQuantity = d.Quantity,
            PendingQuantity = d.PendingQuantity,
            ReceivedBatches = new ObservableCollection<ReceivedBatchState>(
                d.ReceivedBatches.Select(b => new ReceivedBatchState
                {
                    Id = b.Id,
                    LotCode = b.LotCode,
                    Quantity = b.Quantity,
                    ManufacturingDate = b.ManufacturingDate,
                    ExpirationDate = b.ExpirationDate,
                    UnitCost = b.UnitCost
                }))
        }).ToList();

        ProductItems = new ObservableCollection<PurchaseProductState>(items);

        foreach (var item in ProductItems)
        {
            item.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(PurchaseProductState.IsComplete) ||
                    e.PropertyName == nameof(PurchaseProductState.ReceivedQuantity))
                {
                    OnPropertyChanged(nameof(FullyReceivedCount));
                    OnPropertyChanged(nameof(PendingCount));
                    OnPropertyChanged(nameof(ProgressText));
                    OnPropertyChanged(nameof(CanComplete));
                }
            };
        }
    }

    partial void OnQuantityChanged(string value)
    {
        QuantityWarning = string.Empty;
        if (SelectedProduct == null) return;
        if (!int.TryParse(value, out var qty)) return;
        var max = SelectedProduct.PendingQuantity;
        if (qty > max)
            QuantityWarning = $"⚠️ La cantidad ingresada ({qty}) supera las {max} unidades pendientes del pedido.";
    }

    [RelayCommand]
    private void ToggleReceiveMode()
    {
        IsManualStockMode = !IsManualStockMode;
        LotCode = string.Empty;
        Quantity = string.Empty;
        UnitCost = string.Empty;
        ManufacturingDate = DateTime.Today;
        ExpirationDate = DateTime.Today.AddYears(1);
        ErrorMessage = string.Empty;
        QuantityWarning = string.Empty;
    }
    
    public void RemoveBatch(PurchaseProductState item, ReceivedBatchState batch)
    {
        item.ReceivedBatches.Remove(batch);
        item.UpdateProgress();
        RefreshUi();
    }

    [RelayCommand]
    public void CompleteReception(Action onComplete)
    {
        using var transaction = _db.Database.BeginTransaction();

        try
        {
            foreach (var item in ProductItems)
            {
                var purchaseDetail = _purchase.Details.First(d => d.Id == item.PurchaseDetailId);

                foreach (var batchVm in item.ReceivedBatches)
                {
                    var receivedBatch = new ReceivedBatch
                    {
                        PurchaseDetailId = purchaseDetail.Id,
                        LotCode = batchVm.LotCode,
                        Quantity = batchVm.Quantity,
                        ManufacturingDate = batchVm.ManufacturingDate,
                        ExpirationDate = batchVm.ExpirationDate,
                        UnitCost = batchVm.UnitCost,
                        ReceivedAt = DateTime.Now
                    };

                    _db.ReceivedBatches.Add(receivedBatch);

                    var product = _db.Products.Include(p => p.Batches)
                        .FirstOrDefault(p => p.Code == item.ProductCode);
                    if (product == null) continue;

                    var existingBatch = product.Batches.FirstOrDefault(b => b.LotCode == batchVm.LotCode);
                    if (existingBatch != null)
                    {
                        existingBatch.Quantity += batchVm.Quantity;
                    }
                    else
                    {
                        product.Batches.Add(new Batch(
                            product.Code,
                            batchVm.LotCode,
                            batchVm.Quantity,
                            batchVm.ExpirationDate,
                            batchVm.ManufacturingDate)
                        {
                            UnitCost = batchVm.UnitCost
                        });
                    }

                    var previousStock = product.Stock;
                    product.Stock += batchVm.Quantity;

                    var movement = new InventoryMovement
                    {
                        ProductCode = product.Code,
                        Quantity = batchVm.Quantity,
                        Type = "Entrada por Compra",
                        Reason = $"Recepción de orden #{_purchase.InvoiceNumber} - Lote {batchVm.LotCode}",
                        PreviousStock = previousStock,
                        NewStock = product.Stock
                    };
                    _db.InventoryMovements.Add(movement);
                }
            }

            _purchase.ConfirmReception();
            _db.SaveChanges();
            transaction.Commit();

            onComplete.Invoke();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Debug.WriteLine($"Error al completar recepción: {ex.Message}");
        }
    }


    [RelayCommand]
    private void SelectProduct(PurchaseProductState product)
    {
        SelectedProduct = product;
        LotCode = string.Empty;
        Quantity = string.Empty;
        UnitCost = string.Empty;
        ManufacturingDate = DateTime.Today;
        ExpirationDate = DateTime.Today.AddYears(1);
        ErrorMessage = string.Empty;
    }

    [RelayCommand]
    private void AddBatchToSelected()
    {
        if (SelectedProduct == null)
        {
            ErrorMessage = "Seleccione un producto primero";
            return;
        }

        if (!int.TryParse(Quantity, out var qty) || qty <= 0)
        {
            ErrorMessage = "Ingrese una cantidad válida";
            return;
        }

        var maxQty = SelectedProduct.PendingQuantity;
        if (qty > maxQty)
        {
            ErrorMessage = $"La cantidad no puede exceder las {maxQty} unidades pendientes del pedido";
            return;
        }

        TryParse(UnitCost, out var cost);

        ReceivedBatchState newBatch;

        if (IsManualStockMode)
        {
            newBatch = new ReceivedBatchState
            {
                LotCode = $"MANUAL-{DateTime.Now:yyyyMMddHHmmss}",
                Quantity = qty,
                ManufacturingDate = DateTime.Today,
                ExpirationDate = DateTime.Today.AddYears(99),
                UnitCost = cost,
                IsManualStock = true
            };
        }
        else
        {
            if (string.IsNullOrWhiteSpace(LotCode))
            {
                ErrorMessage = "El número de lote es requerido";
                return;
            }

            if (!ExpirationDate.HasValue)
            {
                ErrorMessage = "La fecha de expiración es requerida";
                return;
            }

            if (ExpirationDate.Value.DateTime <= DateTime.Today)
            {
                ErrorMessage = "La fecha de expiración debe ser futura";
                return;
            }

            if (ExpirationDate.Value.DateTime <= (ManufacturingDate?.DateTime ?? DateTime.Today))
            {
                ErrorMessage = "La fecha de expiración debe ser posterior a la de fabricación";
                return;
            }

            newBatch = new ReceivedBatchState
            {
                LotCode = LotCode.Trim(),
                Quantity = qty,
                ManufacturingDate = ManufacturingDate?.DateTime ?? DateTime.Today,
                ExpirationDate = ExpirationDate.Value.DateTime,
                UnitCost = cost,
                IsManualStock = false
            };
        }

        SelectedProduct.ReceivedBatches.Add(newBatch);
        SelectedProduct.UpdateProgress();

        LotCode = string.Empty;
        Quantity = string.Empty;
        UnitCost = string.Empty;
        ManufacturingDate = DateTime.Today;
        ExpirationDate = DateTime.Today.AddYears(1);
        ErrorMessage = string.Empty;
        QuantityWarning = string.Empty;

        RefreshUi();
    }

    public void RefreshUi()
    {
        OnPropertyChanged(nameof(FullyReceivedCount));
        OnPropertyChanged(nameof(PendingCount));
        OnPropertyChanged(nameof(ProgressText));
        OnPropertyChanged(nameof(CanComplete));
    }
}

public partial class PurchaseProductState : ObservableObject
{
    public int PurchaseDetailId { get; set; }
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int TotalQuantity { get; set; }

    [ObservableProperty] private int _pendingQuantity;

    [ObservableProperty] private ObservableCollection<ReceivedBatchState> _receivedBatches = new();

    public int ReceivedQuantity => ReceivedBatches.Sum(b => b.Quantity);
    public bool IsComplete => ReceivedQuantity >= TotalQuantity;

    public void UpdateProgress()
    {
        PendingQuantity = TotalQuantity - ReceivedQuantity;
        OnPropertyChanged(nameof(ReceivedQuantity));
        OnPropertyChanged(nameof(IsComplete));
        OnPropertyChanged(nameof(PendingQuantity));
    }
}

public partial class ReceivedBatchState : ObservableObject
{
    public int Id { get; set; }

    [ObservableProperty] private string _lotCode = string.Empty;

    [ObservableProperty] private int _quantity;

    [ObservableProperty] private DateTime _manufacturingDate = DateTime.Today;

    [ObservableProperty] private DateTime _expirationDate = DateTime.Today.AddYears(1);

    [ObservableProperty] private decimal _unitCost;

    public bool IsManualStock { get; set; }
}