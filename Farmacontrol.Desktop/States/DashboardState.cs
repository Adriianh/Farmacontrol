using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Farmacontrol.Core.Services;
using Farmacontrol.Desktop.Views;

namespace Farmacontrol.Desktop.States;

public partial class DashboardState : ObservableObject
{
    private readonly SalesService _salesService;
    private readonly HistoryService _historyService;
    private readonly InventoryService _inventoryService;

    [ObservableProperty] private int _todaysSalesCount;
    [ObservableProperty] private decimal _todaysSalesTotal;
    [ObservableProperty] private int _activeAlertsCount;
    [ObservableProperty] private int _lowStockProductsCount;

    public DashboardState(SalesService salesService, HistoryService historyService, InventoryService inventoryService)
    {
        _salesService = salesService;
        _historyService = historyService;
        _inventoryService = inventoryService;

        LoadData();
    }

    private void LoadData()
    {
        var today = DateTime.Today;

        var sales = _salesService.GetAllSales().Where(s => s.Date.Date == today && !s.IsVoided).ToList();
        TodaysSalesCount = sales.Count;
        TodaysSalesTotal = sales.Sum(s => s.Total);

        ActiveAlertsCount = _historyService.GetHistory().Count(a => a.Date.Date == today);
        LowStockProductsCount = _inventoryService.GetProducts.Count(p => p.Stock <= 5);
    }

    [RelayCommand]
    private void NavigateToSale()
    {
        var mainState =
            Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions
                .GetRequiredService<MainView.State>(Program.ServiceProvider);
        mainState.ExpandedCategory = "Sales";
        mainState.CurrentContent = new Views.Sales.SaleView();
    }

    [RelayCommand]
    private void NavigateToAlerts()
    {
        var mainState =
            Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions
                .GetRequiredService<MainView.State>(Program.ServiceProvider);
        mainState.ExpandedCategory = "Alerts";
        mainState.CurrentContent = new Views.Alerts.AlertsView();
    }

    [RelayCommand]
    private void NavigateToSearch()
    {
        var mainState =
            Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions
                .GetRequiredService<MainView.State>(Program.ServiceProvider);
        mainState.ExpandedCategory = "Inventory";
        mainState.CurrentContent = new Views.Inventory.SearchProductView();
    }
}