using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Farmacontrol.Core.Model;
using Farmacontrol.Core.Repository;
using Farmacontrol.Model;
using Microsoft.EntityFrameworkCore;
using static System.Decimal;

namespace Farmacontrol.Desktop.States;

public partial class SaleState(AppDbContext db) : ObservableObject
{
    private List<Product> _baseProducts = [];
    [ObservableProperty] private ObservableCollection<Product> _filteredCatalogProducts = [];

    public void LoadCatalog()
    {
        _baseProducts = db.Products.Include(p => p.Batches).Where(p => p.IsActive).ToList();
        UpdateCatalogFilter();
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSearchResults))]
    [NotifyPropertyChangedFor(nameof(ShowSearchResults))]
    private ObservableCollection<Product> _searchResults = new();

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ShowSearchResults))]
    private string _searchQuery = string.Empty;

    public bool HasSearchResults => SearchResults.Count > 0;
    public bool ShowSearchResults => !string.IsNullOrWhiteSpace(SearchQuery) && HasSearchResults;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CartIsEmpty))]
    [NotifyPropertyChangedFor(nameof(CartItemCount))]
    [NotifyPropertyChangedFor(nameof(Subtotal))]
    [NotifyPropertyChangedFor(nameof(DiscountAmount))]
    [NotifyPropertyChangedFor(nameof(Total))]
    private ObservableCollection<CartItemState> _cartItems = new();

    public bool CartIsEmpty => CartItems.Count == 0;
    public int CartItemCount => CartItems.Sum(i => i.Quantity);
    public bool IsNotProcessingEmptyCar => !IsProcessing && !CartIsEmpty;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(DiscountAmount))] [NotifyPropertyChangedFor(nameof(Total))]
    private string _discountPercent = "0";
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Total))]
    private string _taxAmount = "0";
    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ChangeAmount))]
    private string _amountTendered = string.Empty;

    public decimal Subtotal => CartItems.Sum(i => i.Subtotal);

    public decimal DiscountAmount
    {
        get
        {
            TryParse(DiscountPercent, out var pct);
            return Subtotal * (pct / 100m);
        }
    }

    public decimal Total
    {
        get
        {
            TryParse(TaxAmount, out var tax);
            return Subtotal - DiscountAmount + tax;
        }
    }

    public decimal ChangeAmount
    {
        get
        {
            TryParse(AmountTendered, out var tendered);
            return tendered >= Total ? tendered - Total : 0;
        }
    }

    public bool HasChange => ChangeAmount > 0;

    [ObservableProperty] 
    [NotifyPropertyChangedFor(nameof(IsCashPayment))]
    [NotifyPropertyChangedFor(nameof(CanConfirmSale))]
    private PaymentMethod _selectedPaymentMethod = PaymentMethod.Cash;

    public bool IsCashPayment => SelectedPaymentMethod == PaymentMethod.Cash;

    public List<PaymentMethodOption> PaymentMethods { get; } =
    [
        new(PaymentMethod.Cash, "💵 Efectivo"),
        new(PaymentMethod.CreditCard, "💳 Tarjeta de Crédito"),
        new(PaymentMethod.DebitCard, "💳 Tarjeta de Débito"),
        new(PaymentMethod.Transfer, "🏦 Transferencia")
    ];

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(ExtraDataIcon))]
    private bool _extraDataExpanded;

    public string ExtraDataIcon => ExtraDataExpanded ? "▲" : "▼";

    [ObservableProperty] private string _clientName = string.Empty;
    [ObservableProperty] private string _doctorLicense = string.Empty;
    [ObservableProperty] private string _invoiceNumber = string.Empty;
    [ObservableProperty] private string _notes = string.Empty;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasPrescription))]
    private bool _hasPrescriptionAttached;

    public bool HasPrescription => HasPrescriptionAttached;

    [ObservableProperty] private string _prescriptionDoctorName = string.Empty;
    [ObservableProperty] private string _prescriptionPatientName = string.Empty;
    [ObservableProperty] private string _prescriptionFolio = string.Empty;
    [ObservableProperty] private DateTimeOffset? _prescriptionIssuedDate = DateTime.Today;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasError))]
    private string _errorMessage = string.Empty;

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(HasSuccess))]
    private string _successMessage = string.Empty;

    public bool HasSuccess => !string.IsNullOrEmpty(SuccessMessage);

    [ObservableProperty] private bool _isProcessing;
    [ObservableProperty] private int _lastSaleCode;
    
    [ObservableProperty] private string _catalogSearchQuery = string.Empty;
    [ObservableProperty] private string _saleSuccessMessage = string.Empty;

    private decimal TenderedAmountDecimal => TryParse(AmountTendered, out var d) ? d : 0m;
    public bool CanConfirmSale => !CartIsEmpty && (!IsCashPayment || TenderedAmountDecimal >= Total);

    partial void OnCatalogSearchQueryChanged(string value)
    {
        UpdateCatalogFilter();
    }

    private void UpdateCatalogFilter()
    {
        if (string.IsNullOrWhiteSpace(CatalogSearchQuery))
        {
            FilteredCatalogProducts = new ObservableCollection<Product>(_baseProducts);
            return;
        }

        var query = CatalogSearchQuery.ToLower().Trim();
        var results = _baseProducts.Where(p => 
            p.Name.ToLower().Contains(query) || 
            p.Code.ToLower().Contains(query) || 
            (p.Tags.Any(t => t.ToLower().Contains(query)))
        ).ToList();

        FilteredCatalogProducts = new ObservableCollection<Product>(results);
    }

    partial void OnSearchQueryChanged(string value)
    {
        SuccessMessage = string.Empty;
        
        if (string.IsNullOrWhiteSpace(value))
        {
            SearchResults = [];
            return;
        }

        var query = value.Trim();

        var exact = db.Products
            .Include(p => p.Batches)
            .FirstOrDefault(p =>
                (p.Code == query || p.Barcode == query) && p.IsActive && p.Stock > 0);

        if (exact != null)
        {
            AddToCart(exact);
            SearchQuery = string.Empty;
            return;
        }

        var results = db.Products
            .Include(p => p.Batches)
            .Where(p => p.Name.Contains(query) && p.IsActive && p.Stock > 0)
            .Take(8)
            .ToList();

        SearchResults = new ObservableCollection<Product>(results);
    }

    [RelayCommand]
    private void SelectSearchResult(Product product)
    {
        AddToCart(product);
        SearchQuery = string.Empty;
        SearchResults = new ObservableCollection<Product>();
    }

    public void AddToCart(Product product)
    {
        var existing = CartItems.FirstOrDefault(i => i.ProductCode == product.Code);
        if (existing != null)
        {
            if (existing.Quantity >= product.Stock)
            {
                ErrorMessage = $"⚠️ No hay suficiente stock de {product.Name} (disponible: {product.Stock})";
                return;
            }

            existing.Quantity++;
        }
        else
        {
            if (product.Stock <= 0)
            {
                ErrorMessage = $"⚠️ {product.Name} no tiene stock disponible";
                return;
            }

            var item = new CartItemState(product);
            item.PropertyChanged += (_, _) => RefreshTotals();
            CartItems.Add(item);
        }

        ErrorMessage = string.Empty;
        RefreshTotals();
    }

    [RelayCommand]
    private void RemoveFromCart(CartItemState item)
    {
        CartItems.Remove(item);
        RefreshTotals();
    }

    [RelayCommand]
    private void IncrementItem(CartItemState item)
    {
        var product = db.Products.Find(item.ProductCode);
        if (product == null) return;
        if (item.Quantity >= product.Stock)
        {
            ErrorMessage = $"⚠️ Stock máximo disponible: {product.Stock}";
            return;
        }

        item.Quantity++;
        ErrorMessage = string.Empty;
        RefreshTotals();
    }

    [RelayCommand]
    private void DecrementItem(CartItemState item)
    {
        if (item.Quantity <= 1)
        {
            RemoveFromCart(item);
            return;
        }

        item.Quantity--;
        RefreshTotals();
    }

    [RelayCommand]
    private void ClearCart()
    {
        CartItems.Clear();
        DiscountPercent = "0";
        TaxAmount = "0";
        AmountTendered = string.Empty;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
        RefreshTotals();
    }

    private void RefreshTotals()
    {
        OnPropertyChanged(nameof(CartIsEmpty));
        OnPropertyChanged(nameof(CartItemCount));
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(DiscountAmount));
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(ChangeAmount));
        OnPropertyChanged(nameof(HasChange));
        OnPropertyChanged(nameof(CanConfirmSale));
    }

    [RelayCommand]
    private void ToggleExtraData()
    {
        ExtraDataExpanded = !ExtraDataExpanded;
    }

    [RelayCommand]
    private void TogglePrescription()
    {
        HasPrescriptionAttached = !HasPrescriptionAttached;
    }

    [RelayCommand]
    private void ConfirmSale()
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;

        if (CartItems.Count == 0)
        {
            ErrorMessage = "⚠️ El carrito está vacío";
            return;
        }

        if (IsCashPayment)
        {
            TryParse(AmountTendered, out var tendered);
            if (tendered < Total)
            {
                ErrorMessage = $"⚠️ El monto recibido (Q{tendered:F2}) es menor al total (Q{Total:F2})";
                return;
            }
        }

        IsProcessing = true;

        using var transaction = db.Database.BeginTransaction();
        try
        {
            var nextCode = (db.Sales.Max(s => (int?)s.Code) ?? 0) + 1;

            TryParse(DiscountPercent, out var discPct);
            TryParse(TaxAmount, out var tax);

            var sale = new Sale(nextCode)
            {
                PaymentMethod = SelectedPaymentMethod,
                DiscountPercentage = discPct,
                TaxAmount = tax,
                ClientName = string.IsNullOrWhiteSpace(ClientName) ? null : ClientName.Trim(),
                DoctorLicense = string.IsNullOrWhiteSpace(DoctorLicense) ? null : DoctorLicense.Trim(),
                InvoiceNumber = string.IsNullOrWhiteSpace(InvoiceNumber) ? null : InvoiceNumber.Trim(),
                Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes.Trim(),
            };

            foreach (var cartItem in CartItems)
            {
                var product = db.Products.Include(p => p.Batches)
                    .First(p => p.Code == cartItem.ProductCode);

                sale.AddDetail(product, cartItem.Quantity);

                product.ReduceBatchStock(cartItem.Quantity);

                var movement = new InventoryMovement
                {
                    ProductCode = product.Code,
                    Quantity = cartItem.Quantity,
                    Type = "Salida por Venta",
                    Reason = $"Venta #{nextCode}",
                    PreviousStock = product.Stock + cartItem.Quantity,
                    NewStock = product.Stock
                };
                db.InventoryMovements.Add(movement);
            }

            sale.RecalculateTotal();
            db.Sales.Add(sale);

            if (HasPrescriptionAttached && !string.IsNullOrWhiteSpace(PrescriptionFolio))
            {
                var prescription = new Prescription(
                    nextCode,
                    DoctorLicense.Trim(),
                    PrescriptionDoctorName.Trim(),
                    PrescriptionPatientName.Trim(),
                    PrescriptionIssuedDate?.DateTime ?? DateTime.Today,
                    PrescriptionFolio.Trim()
                );
                db.Prescriptions.Add(prescription);
            }

            db.SaveChanges();
            transaction.Commit();

            LastSaleCode = nextCode;
            SuccessMessage = $"✅ Venta #{nextCode} registrada exitosamente — Total: Q{sale.Total:F2}";

            ResetAfterSale();
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            ErrorMessage = $"❌ Error al registrar la venta: {ex.Message}";
            Debug.WriteLine(ex);
        }
        finally
        {
            IsProcessing = false;
        }
    }

    private void ResetAfterSale()
    {
        CartItems.Clear();
        SearchQuery = string.Empty;
        SearchResults = new ObservableCollection<Product>();
        DiscountPercent = "0";
        TaxAmount = "0";
        AmountTendered = string.Empty;
        ClientName = string.Empty;
        DoctorLicense = string.Empty;
        InvoiceNumber = string.Empty;
        Notes = string.Empty;
        HasPrescriptionAttached = false;
        PrescriptionDoctorName = string.Empty;
        PrescriptionPatientName = string.Empty;
        PrescriptionFolio = string.Empty;
        PrescriptionIssuedDate = DateTime.Today;
        ExtraDataExpanded = false;
        SelectedPaymentMethod = PaymentMethod.Cash;
        RefreshTotals();
    }

    partial void OnDiscountPercentChanged(string value) => RefreshTotals();
    partial void OnTaxAmountChanged(string value) => RefreshTotals();

    partial void OnAmountTenderedChanged(string value)
    {
        OnPropertyChanged(nameof(ChangeAmount));
        OnPropertyChanged(nameof(HasChange));
        OnPropertyChanged(nameof(CanConfirmSale));
    }
}

public partial class CartItemState(Product product) : ObservableObject
{
    public string ProductCode { get; } = product.Code;
    public string ProductName { get; } = product.Name;
    public decimal UnitPrice { get; } = product.Price;
    public int MaxStock { get; } = product.Stock;

    [ObservableProperty] [NotifyPropertyChangedFor(nameof(Subtotal))]
    private int _quantity = 1;

    public decimal Subtotal => Quantity * UnitPrice;
}

public record PaymentMethodOption(PaymentMethod Value, string Label);