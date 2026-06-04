using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Farmacontrol.Core.Model;
using Farmacontrol.Core.Repository;
using Microsoft.EntityFrameworkCore;

namespace Farmacontrol.Desktop.States;

public partial class PendingOrdersState : ObservableObject
{
    private readonly AppDbContext _db;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowSuggestions))]
    [NotifyPropertyChangedFor(nameof(ShowEmpty))]
    private ObservableCollection<ProductOrderSuggestion> _lowStockSuggestions = new();

    [ObservableProperty] private ObservableCollection<Supplier> _suppliers = new();
    [ObservableProperty] private Supplier? _selectedSupplier;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasErrorMessage))]
    private string _errorMessage = string.Empty;

    public bool HasErrorMessage => !string.IsNullOrEmpty(ErrorMessage);

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowSuccess))]
    [NotifyPropertyChangedFor(nameof(ShowSuggestions))]
    [NotifyPropertyChangedFor(nameof(ShowEmpty))]
    private bool _isOrderGeneratedSuccessfully;

    [ObservableProperty] private ObservableCollection<Purchase> _pendingPurchases = new();

    public bool ShowEmpty => !IsOrderGeneratedSuccessfully && LowStockSuggestions.Count == 0;
    public bool ShowSuggestions => !IsOrderGeneratedSuccessfully && LowStockSuggestions.Count > 0;
    public bool ShowSuccess => IsOrderGeneratedSuccessfully;
    public bool HasPendingPurchases => PendingPurchases.Count > 0;

    public string PendingPurchasesCount => PendingPurchases.Count.ToString();
    public bool HasSuppliers => Suppliers.Count > 0;
    

    public PendingOrdersState(AppDbContext db)
    {
        _db = db;
        LoadDashboardData();
        LoadPendingPurchases();
    }

    [RelayCommand]
    private void LoadDashboardData()
    {
        ErrorMessage = string.Empty;

        var supplierList = _db.Suppliers.AsQueryable().Where(s => s.IsActive).ToList();
        Suppliers = new ObservableCollection<Supplier>(supplierList);

        var lowStockProducts = _db.Products
            .AsEnumerable()
            .Where(p => p.IsActive && p.Stock <= p.MinimumStock)
            .Select(p => new ProductOrderSuggestion
            {
                ProductCode = p.Code,
                ProductName = p.Name,
                CurrentStock = p.Stock,
                MinStock = p.MinimumStock,
                SuggestedQuantity = (p.MinimumStock * 2) - p.Stock,
                IsSelected = true
            })
            .ToList();

        LowStockSuggestions = new ObservableCollection<ProductOrderSuggestion>(lowStockProducts);
    }

    [RelayCommand]
    public void GeneratePurchaseOrder()
    {
        ErrorMessage = string.Empty;
        IsOrderGeneratedSuccessfully = false;

        if (SelectedSupplier == null)
        {
            ErrorMessage = "⚠️ Debe seleccionar un proveedor para dirigir el pedido.";
            return;
        }

        var itemsToOrder = LowStockSuggestions
            .Where(x => x is { IsSelected: true, SuggestedQuantity: > 0 })
            .ToList();

        if (!itemsToOrder.Any())
        {
            ErrorMessage = "⚠️ Seleccione al menos un producto con una cantidad mayor a 0.";
            return;
        }

        try
        {
            var orderReference = $"ORD-{DateTime.Now:yyyyMMddHHmmss}";
            var newPurchase = new Purchase(SelectedSupplier.Code, orderReference);

            foreach (var item in itemsToOrder)
            {
                var product = _db.Products.FirstOrDefault(p => p.Code == item.ProductCode);
                if (product == null) continue;
                var estimatedUnitCost = product.Price * 0.70m;

                newPurchase.AddDetail(
                    product,
                    lotCode: "PENDIENTE",
                    quantity: item.SuggestedQuantity,
                    unitCost: estimatedUnitCost,
                    expDate: DateTime.Now.AddYears(2)
                );
            }

            _db.Purchases.Add(newPurchase);
            _db.SaveChanges();

            IsOrderGeneratedSuccessfully = true;
            LoadDashboardData();
            LoadPendingPurchases();
        }
        catch (Exception ex)
        {
            ErrorMessage = $"❌ Error al guardar la orden en la base de datos: {ex.Message}";
        }
    }

    private void LoadPendingPurchases()
    {
        var list = _db.Purchases
            .Include(p => p.Details)
            .Where(p => p.Status == PurchaseStatus.Pending)
            .OrderByDescending(p => p.Date)
            .ToList();
        PendingPurchases = new ObservableCollection<Purchase>(list);
    }

    public void CompletePurchase(Purchase purchase)
    {
        var fullPurchase = _db.Purchases
            .Include(p => p.Details)
            .FirstOrDefault(p => p.Id == purchase.Id);

        if (fullPurchase == null) return;

        foreach (var detail in fullPurchase.Details)
        {
            var product = _db.Products.FirstOrDefault(p => p.Code == detail.ProductCode);
            if (product == null) continue;

            var previousStock = product.Stock;
            product.Stock += detail.Quantity;

            var movement = new InventoryMovement
            {
                ProductCode = product.Code,
                Quantity = detail.Quantity,
                Type = "Entrada por Compra",
                Reason = $"Recepción de orden #{fullPurchase.InvoiceNumber}",
                PreviousStock = previousStock,
                NewStock = product.Stock
            };
            _db.InventoryMovements.Add(movement);
        }

        fullPurchase.ConfirmReception();
        _db.SaveChanges();

        LoadDashboardData();
        LoadPendingPurchases();
    }
}

public class ProductOrderSuggestion
{
    public required string ProductCode { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int CurrentStock { get; init; }
    public int MinStock { get; init; }
    public int SuggestedQuantity { get; init; }
    public bool IsSelected { get; init; }
}