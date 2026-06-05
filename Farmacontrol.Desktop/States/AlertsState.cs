using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Farmacontrol.Core.Model;
using Farmacontrol.Core.Services;

namespace Farmacontrol.Desktop.States;

public partial class AlertsState : ObservableObject
{
    private readonly HistoryService _historyService;
    private readonly InventoryService _inventoryService;

    public ObservableCollection<Alert> TodayAlerts { get; } = new();

    [ObservableProperty] private bool _isScanning;

    [ObservableProperty]
    private string _scanStatusMessage = "Sistema listo. Haz clic en Escanear para evaluar el inventario.";

    public AlertsState(HistoryService historyService, InventoryService inventoryService)
    {
        _historyService = historyService;
        _inventoryService = inventoryService;
        LoadTodayAlerts();
    }

    [RelayCommand]
    private void LoadTodayAlerts()
    {
        TodayAlerts.Clear();
        var allAlerts = _historyService.GetHistory();

        var todayAlerts = allAlerts
            .Where(a => a.Date.Date == DateTime.Today)
            .OrderByDescending(a => a.Date)
            .ToList();

        foreach (var alert in todayAlerts)
        {
            TodayAlerts.Add(alert);
        }
    }

    [RelayCommand]
    private async Task ScanInventoryAsync()
    {
        IsScanning = true;
        ScanStatusMessage = "Escaneando inventario y evaluando productos...";

        await Task.Run(() =>
        {
            var products = _inventoryService.GetProducts.ToList();
            _historyService.VerifyAlert(products);
        });

        LoadTodayAlerts();

        ScanStatusMessage = $"Escaneo completado. Se generaron {TodayAlerts.Count} alertas de inventario para hoy.";
        IsScanning = false;
    }
}