using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Farmacontrol.Core.Model;
using Farmacontrol.Core.Services;

namespace Farmacontrol.Desktop.States;

public partial class SalesHistoryState : ObservableObject
{
    private readonly SalesService _salesService;

    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private DateTime? _startDate = DateTime.Today.AddDays(-7);
    [ObservableProperty] private DateTime? _endDate = DateTime.Today;

    public ObservableCollection<Sale> SalesList { get; } = new();
    private List<Sale> _allSales = new();

    public SalesHistoryState(SalesService salesService)
    {
        _salesService = salesService;
        LoadSales();
    }

    [RelayCommand]
    public void LoadSales()
    {
        _allSales = _salesService.GetAllSales().OrderByDescending(s => s.Date).ToList();
        FilterSales();
    }

    private void FilterSales()
    {
        SalesList.Clear();
        var query = SearchQuery?.Trim().ToLowerInvariant();
        
        IEnumerable<Sale> filtered = _allSales;
        
        var start = StartDate?.Date ?? DateTime.MinValue;
        var end = EndDate?.Date.AddDays(1).AddTicks(-1) ?? DateTime.MaxValue;

        filtered = filtered.Where(s => s.Date >= start && s.Date <= end);

        if (!string.IsNullOrEmpty(query))
        {
            filtered = filtered.Where(s => 
                s.Code.ToString().Contains(query) || 
                (s.ClientName != null && s.ClientName.ToLowerInvariant().Contains(query))
            );
        }

        foreach (var sale in filtered.Take(100))
        {
            SalesList.Add(sale);
        }
    }

    partial void OnSearchQueryChanged(string value) => FilterSales();
    partial void OnStartDateChanged(DateTime? value) => FilterSales();
    partial void OnEndDateChanged(DateTime? value) => FilterSales();
}
