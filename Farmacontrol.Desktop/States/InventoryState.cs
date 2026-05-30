using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Farmacontrol.Core.Services;
using Farmacontrol.Model;

namespace Farmacontrol.Desktop.States;

public partial class InventoryState : ObservableObject
{
    private readonly InventoryService _inventoryService;
    private List<Product> _baseProducts = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilteredProducts))]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilteredProducts))]
    public partial int SortCriterionIndex { get; set; } = 0;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilteredProducts))]
    [NotifyPropertyChangedFor(nameof(SortIcon)) ]
    public partial bool AscendingOrder { get; set; } = true;

    [ObservableProperty]
    private bool _isAddModalOpen;

    public AddProductState AddProductForm { get; }

    public string SortIcon => AscendingOrder ? "🔼 Asc" : "🔽 Desc";

    public InventoryState(InventoryService inventoryService)
    {
        _inventoryService = inventoryService;
        AddProductForm = new AddProductState(inventoryService);
        LoadProducts();
    }

    public void LoadProducts()
    {
        _baseProducts = _inventoryService.GetProducts.Where(p => p.IsActive).ToList();
        OnPropertyChanged(nameof(FilteredProducts));
    }

    public void ToggleSortDirection()
    {
        AscendingOrder = !AscendingOrder;
    }

    public void PrepareAddProduct()
    {
        AddProductForm.PrepareForAdd();
        IsAddModalOpen = true;
    }

    public void PrepareEditProduct(Product product)
    {
        AddProductForm.PrepareForEdit(product);
        IsAddModalOpen = true;
    }

    public void DeleteProduct(Product product)
    {
        _inventoryService.RemoveProduct(product);
        LoadProducts();
    }

    public IEnumerable<Product> FilteredProducts
    {
        get
        {
            IEnumerable<Product> result = _baseProducts;

            if (!string.IsNullOrWhiteSpace(SearchText))
            {
                var query = SearchText.ToLower().Trim();
                result = result.Where(p =>
                    p.Name.ToLower().Contains(query) ||
                    p.Code.ToLower().Contains(query));
            }

            result = SortCriterionIndex switch
            {
                1 => AscendingOrder ? result.OrderBy(p => p.Stock) : result.OrderByDescending(p => p.Stock),
                2 => AscendingOrder ? result.OrderBy(p => p.Price) : result.OrderByDescending(p => p.Price),
                _ => AscendingOrder ? result.OrderBy(p => p.Name) : result.OrderByDescending(p => p.Name)
            };

            return result.ToList();
        }
    }
}