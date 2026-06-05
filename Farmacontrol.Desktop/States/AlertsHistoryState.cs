using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Farmacontrol.Core.Model;
using Farmacontrol.Core.Services;

namespace Farmacontrol.Desktop.States;

public partial class AlertsHistoryState : ObservableObject
{
    private readonly HistoryService _historyService;

    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private DateTime? _startDate = DateTime.Today.AddDays(-30);
    [ObservableProperty] private DateTime? _endDate = DateTime.Today;

    public ObservableCollection<Alert> AlertsList { get; } = new();
    private List<Alert> _allAlerts = new();

    public AlertsHistoryState(HistoryService historyService)
    {
        _historyService = historyService;
        LoadAlerts();
    }

    [RelayCommand]
    private void LoadAlerts()
    {
        _allAlerts = _historyService.GetHistory().OrderByDescending(a => a.Date).ToList();
        FilterAlerts();
    }

    private void FilterAlerts()
    {
        AlertsList.Clear();
        var query = SearchQuery.Trim().ToLowerInvariant();
        
        IEnumerable<Alert> filtered = _allAlerts;
        
        var start = StartDate?.Date ?? DateTime.MinValue;
        var end = EndDate?.Date.AddDays(1).AddTicks(-1) ?? DateTime.MaxValue;

        filtered = filtered.Where(a => a.Date >= start && a.Date <= end);

        if (!string.IsNullOrEmpty(query))
        {
            filtered = filtered.Where(a => 
                a.ProductCode.ToLowerInvariant().Contains(query) || 
                a.ProductName.ToLowerInvariant().Contains(query) ||
                a.Type.ToLowerInvariant().Contains(query)
            );
        }

        foreach (var alert in filtered.Take(100))
        {
            AlertsList.Add(alert);
        }
    }

    partial void OnSearchQueryChanged(string value) => FilterAlerts();
    partial void OnStartDateChanged(DateTime? value) => FilterAlerts();
    partial void OnEndDateChanged(DateTime? value) => FilterAlerts();
}
