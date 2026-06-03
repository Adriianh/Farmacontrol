using CommunityToolkit.Mvvm.ComponentModel;
using Farmacontrol.Core.Interface;
using Farmacontrol.Core.Services;
using Farmacontrol.Model;
using System.Collections.ObjectModel;
using Farmacontrol.Core.Repository;

namespace Farmacontrol.Desktop.States;

public partial class InventoryState : ObservableObject
{
    private readonly InventoryService _inventoryService;
    private List<Product> _baseProducts = [];

    [ObservableProperty] private string _searchText = string.Empty;

    [ObservableProperty] private ObservableCollection<Product> _filteredProducts = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilteredProducts))]
    public partial int SortCriterionIndex { get; set; } = 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilteredProducts))]
    [NotifyPropertyChangedFor(nameof(SortIcon))]
    public partial bool AscendingOrder { get; private set; } = true;

    [ObservableProperty] private bool _isAddModalOpen;

    [ObservableProperty] private bool _isBatchesModalOpen;

    [ObservableProperty] private Product? _selectedProduct;

    [ObservableProperty] private bool _isEditingProduct;

    public ProductState ProductForm { get; }

    public string SortIcon => AscendingOrder ? "🔼 Asc" : "🔽 Desc";

    public InventoryState(InventoryService inventoryService, AppDbContext db)
    {
        _inventoryService = inventoryService;
        ProductForm = new ProductState(inventoryService, db);
        LoadProducts();
    }

    partial void OnSearchTextChanged(string value)
    {
        UpdateFilteredProducts();
    }

    partial void OnSortCriterionIndexChanged(int value)
    {
        UpdateFilteredProducts();
    }

    partial void OnAscendingOrderChanged(bool value)
    {
        UpdateFilteredProducts();
    }

    private void UpdateFilteredProducts()
    {
        IEnumerable<Product> result = _baseProducts;

        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            var query = SearchText.ToLower().Trim();
            result = result.Where(p =>
                p.Name.ToLower().Contains(query) ||
                p.Code.ToLower().Contains(query) ||
                p.Tags.Any(t => t.ToLower().Contains(query)) ||
                p.Ingredients.Any(i => i.ToLower().Contains(query))
            );
        }

        result = SortCriterionIndex switch
        {
            1 => AscendingOrder ? result.OrderBy(p => p.Stock) : result.OrderByDescending(p => p.Stock),
            2 => AscendingOrder ? result.OrderBy(p => p.Price) : result.OrderByDescending(p => p.Price),
            _ => AscendingOrder ? result.OrderBy(p => p.Name) : result.OrderByDescending(p => p.Name)
        };

        var newList = result.ToList();
        FilteredProducts.Clear();
        foreach (var product in newList)
        {
            FilteredProducts.Add(product);
        }
    }

    public void LoadProducts()
    {
        _baseProducts = _inventoryService.GetProducts.Where(p => p.IsActive).ToList();
        UpdateFilteredProducts();
    }

    public void ToggleSortDirection()
    {
        AscendingOrder = !AscendingOrder;
    }

    public void PrepareAddProduct()
    {
        ProductForm.PrepareForAdd();
        IsEditingProduct = false;
        IsAddModalOpen = true;
    }

    public void PrepareEditProduct(Product product)
    {
        var freshProduct = _inventoryService.GetProductForEdit(product.Code);
        if (freshProduct == null) return;

        ProductForm.PrepareForEdit(freshProduct);
        IsEditingProduct = true;
        IsAddModalOpen = true;
    }

    public void DeleteProduct(Product product)
    {
        _inventoryService.RemoveProduct(product);
        LoadProducts();
    }

    public void ShowBatchesModal(Product product)
    {
        SelectedProduct = product;
        IsBatchesModalOpen = true;
    }

    public void CloseBatchesModal()
    {
        IsBatchesModalOpen = false;
        SelectedProduct = null;
    }

    public void CloseAddModal()
    {
        IsAddModalOpen = false;
        IsEditingProduct = false;
    }

    public string GetProductAlertStatus(Product? product)
    {
        switch (product)
        {
            case null:
                return "NORMAL";
            case IExpirable expirable:
                try
                {
                    if (expirable.IsExpired())
                        return "EXPIRED";

                    var daysUntilExpiry = expirable.ExpiresIn();
                    if (daysUntilExpiry is > 0 and <= 30)
                        return "EXPIRING";
                }
                catch
                {
                    // ignored
                }

                break;
        }

        if (product.Batches.Count <= 0) return product.IsStockLow() ? "LOWSTOCK" : "NORMAL";
        try
        {
            var expiredBatches = product.Batches.Where(b => b.ExpirationDate < DateTime.Today).ToList();
            if (expiredBatches.Any())
                return "EXPIRED";

            var expiringBatches = product.Batches.Where(b =>
                b.ExpirationDate >= DateTime.Today &&
                (b.ExpirationDate - DateTime.Today).Days <= 30).ToList();
            if (expiringBatches.Any())
                return "EXPIRING";
        }
        catch
        {
            // ignored
        }

        return product.IsStockLow() ? "LOWSTOCK" : "NORMAL";
    }

    public bool HasProductAlerts(Product product)
    {
        return GetProductAlertStatus(product) != "NORMAL";
    }
}