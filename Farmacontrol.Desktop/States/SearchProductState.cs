using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Farmacontrol.Core.Repository;
using Farmacontrol.Model;
using Microsoft.EntityFrameworkCore;

namespace Farmacontrol.Desktop.States;

public partial class SearchProductState(AppDbContext db, ProductState productForm) : ObservableObject
{
    public ProductState ProductForm { get; } = productForm;

    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private bool _hasSearched;
    [ObservableProperty] private bool _isEditing;

    [ObservableProperty] private ObservableCollection<Product> _similarProducts = new();
    public bool HasSimilarResults => SimilarProducts.Count > 0;

    [RelayCommand]
    public void ExecuteSearch()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;

        HasSearched = true;
        IsEditing = false;
        SimilarProducts.Clear();

        var queryStr = SearchQuery.Trim();

        var exactMatch = db.Products
            .AsQueryable()
            .Include(p => p.Batches)
            .Include(p => p.Suppliers)
            .FirstOrDefault(p => p.Code == queryStr || p.Barcode == queryStr);

        if (exactMatch != null)
        {
            SetupInlineForm(exactMatch);
            return;
        }

        var matches = db.Products
            .AsQueryable()
            .Include(p => p.Batches)
            .Where(p => EF.Functions.Like(p.Name, $"%{queryStr}%"))
            .ToList();

        if (matches.Count == 1)
        {
            SetupInlineForm(matches[0]);
        }
        else if (matches.Count > 1)
        {
            SimilarProducts = new ObservableCollection<Product>(matches);
        }
    }
    
    public void SetupInlineForm(Product product)
    {
        SimilarProducts.Clear();
        
        ProductForm.PrepareForEdit(product);
        
        IsEditing = true;
    }

    public void CancelInlineEdit()
    {
        IsEditing = false;
        HasSearched = false;
        SearchQuery = string.Empty;
    }

    public void SaveInlineChanges()
    {
        ProductForm.SaveProduct();
        
        if (!string.IsNullOrEmpty(ProductForm.ErrorMessage)) return;

        IsEditing = false;
        HasSearched = false;
        SearchQuery = string.Empty;
    }
}