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

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSimilarResults))]
    [NotifyPropertyChangedFor(nameof(ShowEmpty))]
    [NotifyPropertyChangedFor(nameof(ShowSimilarResults))]
    [NotifyPropertyChangedFor(nameof(ShowEditing))]
    private bool _isEditing;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ShowEmpty))]
    [NotifyPropertyChangedFor(nameof(ShowSimilarResults))]
    private bool _hasSearched;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSimilarResults))]
    [NotifyPropertyChangedFor(nameof(SimilarResultsLabel))]
    [NotifyPropertyChangedFor(nameof(ShowEmpty))]
    [NotifyPropertyChangedFor(nameof(ShowSimilarResults))]
    private ObservableCollection<Product> _similarProducts = new();

    public bool HasSimilarResults => SimilarProducts.Count > 0;

    public string SimilarResultsLabel => HasSimilarResults
        ? $"🔍 Coincidencias encontradas ({SimilarProducts.Count})"
        : "⚠️ No se encontró ningún producto con ese criterio.";

    public bool ShowEmpty => !IsEditing && !HasSimilarResults && !HasSearched;
    public bool ShowSimilarResults => !IsEditing && (HasSimilarResults || HasSearched);
    public bool ShowEditing => IsEditing;

    [RelayCommand]
    public void ExecuteSearch()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery)) return;

        HasSearched = true;
        IsEditing = false;
        SimilarProducts = new ObservableCollection<Product>();

        var queryStr = SearchQuery.Trim();

        var exactMatch = db.Products
            .Include(p => p.Batches)
            .FirstOrDefault(p => (p.Code == queryStr || p.Barcode == queryStr) && p.IsActive);

        if (exactMatch != null)
        {
            SetupInlineForm(exactMatch);
            return;
        }

        var inactiveMatch = db.Products
            .Include(p => p.Batches)
            .FirstOrDefault(p => (p.Code == queryStr || p.Barcode == queryStr) && !p.IsActive);

        if (inactiveMatch != null)
        {
            SimilarProducts = [];
            ProductForm.SetInactiveProductWarning(inactiveMatch);
            IsEditing = true;
            return;
        }

        var matches = db.Products
            .Include(p => p.Batches)
            .Where(p => p.Name.Contains(queryStr) && p.IsActive)
            .Take(10)
            .ToList();

        if (matches.Count == 1)
        {
            SetupInlineForm(matches[0]);
            return;
        }

        if (matches.Count > 1)
        {
            SimilarProducts = new ObservableCollection<Product>(matches);
            return;
        }

        var words = queryStr
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(w => w.Length > 2)
            .ToList();

        if (words.Count == 0) return;

        var seen = new HashSet<string>();
        var wordMatches = new List<Product>();

        foreach (var word in words)
        {
            var wordResults = db.Products
                .Include(p => p.Batches)
                .Where(p => p.Name.Contains(word) && p.IsActive)
                .Take(10)
                .ToList();

            foreach (var product in wordResults)
            {
                if (seen.Add(product.Code))
                    wordMatches.Add(product);
            }

            if (wordMatches.Count >= 10) break;
        }

        SimilarProducts = new ObservableCollection<Product>(wordMatches.Take(10));
    }

    public void SetupInlineForm(Product product)
    {
        SimilarProducts = new ObservableCollection<Product>();
        ProductForm.PrepareForEdit(product);
        IsEditing = true;
    }

    public void CancelInlineEdit()
    {
        IsEditing = false;
        HasSearched = false;
        SearchQuery = string.Empty;
        SimilarProducts = new ObservableCollection<Product>();
    }

    public void SaveInlineChanges()
    {
        ProductForm.SaveProduct();
        if (!string.IsNullOrEmpty(ProductForm.ErrorMessage)) return;

        IsEditing = false;
        HasSearched = false;
        SearchQuery = string.Empty;
        SimilarProducts = new ObservableCollection<Product>();
    }
}