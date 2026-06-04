using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Farmacontrol.Core.Model;
using Farmacontrol.Core.Services;

namespace Farmacontrol.Desktop.States;

public partial class VoidSaleState : ObservableObject
{
    private readonly SalesService _salesService;

    [ObservableProperty] private string _searchQuery = string.Empty;
    [ObservableProperty] private string _errorMessage = string.Empty;
    [ObservableProperty] private string _successMessage = string.Empty;
    [ObservableProperty] private Sale? _selectedSale;
    [ObservableProperty] private Sale? _selectedListSale;
    [ObservableProperty] private string _selectedVoidReason = string.Empty;
    [ObservableProperty] private string _voidDetails = string.Empty;

    public ObservableCollection<Sale> SalesList { get; } = new();
    private List<Sale> _allSales = new();

    public string[] VoidReasons { get; } =
    [
        "Devuelto al inventario",
        "Dado de baja",
        "Anulación sin producto"
    ];

    public VoidSaleState(SalesService salesService)
    {
        _salesService = salesService;
        LoadSales();
    }

    private void LoadSales()
    {
        _allSales = _salesService.GetAllSales().Where(s => !s.IsVoided).OrderByDescending(s => s.Date).ToList();
        FilterSales();
    }

    private void FilterSales()
    {
        SalesList.Clear();
        var query = SearchQuery.Trim().ToLowerInvariant();
        
        IEnumerable<Sale> filtered = _allSales;
        
        if (!string.IsNullOrEmpty(query))
        {
            filtered = _allSales.Where(s => 
                s.Code.ToString().Contains(query) || 
                (s.ClientName != null && s.ClientName.ToLowerInvariant().Contains(query))
            );
        }

        foreach (var sale in filtered.Take(50))
        {
            SalesList.Add(sale);
        }
    }

    public bool CanConfirmVoid => SelectedSale is { IsVoided: false } && 
                                  !string.IsNullOrWhiteSpace(SelectedVoidReason) &&
                                  !string.IsNullOrWhiteSpace(VoidDetails);

    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public bool HasSuccess => !string.IsNullOrEmpty(SuccessMessage);
    public bool HasSelectedSale => SelectedSale != null;
    public string SelectedSaleCode => SelectedSale != null ? $"#{SelectedSale.Code}" : "";
    public string SelectedSaleDate => SelectedSale != null ? SelectedSale.Date.ToString("dd/MM/yyyy HH:mm") : "";
    public string SelectedSalePayment => SelectedSale != null ? SelectedSale.PaymentMethod.ToString() : "";
    public string SelectedSaleTotal => SelectedSale != null ? $"Q{SelectedSale.Total:F2}" : "";

    partial void OnErrorMessageChanged(string value)
    {
        Console.WriteLine("ErrorMessage changed: " + value);
        OnPropertyChanged(nameof(HasError));
    }
    
    partial void OnSuccessMessageChanged(string value) => OnPropertyChanged(nameof(HasSuccess));
    
    partial void OnSearchQueryChanged(string value)
    {
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
        FilterSales();
    }

    partial void OnSelectedListSaleChanged(Sale? value)
    {
        SelectedSale = value;
        ErrorMessage = string.Empty;
        SuccessMessage = string.Empty;
    }

    partial void OnSelectedVoidReasonChanged(string value) => OnPropertyChanged(nameof(CanConfirmVoid));
    partial void OnVoidDetailsChanged(string value) => OnPropertyChanged(nameof(CanConfirmVoid));
    partial void OnSelectedSaleChanged(Sale? value) 
    {
        OnPropertyChanged(nameof(CanConfirmVoid));
        OnPropertyChanged(nameof(HasSelectedSale));
        OnPropertyChanged(nameof(SelectedSaleCode));
        OnPropertyChanged(nameof(SelectedSaleDate));
        OnPropertyChanged(nameof(SelectedSalePayment));
        OnPropertyChanged(nameof(SelectedSaleTotal));
    }

    [RelayCommand]
    private void SearchSale()
    {
        if (string.IsNullOrWhiteSpace(SearchQuery) || !int.TryParse(SearchQuery, out var code)) return;
        
        var sale = _allSales.FirstOrDefault(s => s.Code == code);
        if (sale != null)
        {
            SelectedListSale = sale;
        }
    }

    [RelayCommand]
    private void ConfirmVoid()
    {
        if (SelectedSale == null || SelectedSale.IsVoided) return;

        try 
        {
            _salesService.VoidSale(SelectedSale.Code, SelectedVoidReason, VoidDetails);
            SuccessMessage = "Venta anulada correctamente.";
            SelectedSale = null;
            SelectedListSale = null;
            SearchQuery = string.Empty;
            SelectedVoidReason = string.Empty;
            VoidDetails = string.Empty;
            LoadSales();
        }
        catch (Exception ex)
        {
            ErrorMessage = "Ocurrió un error al anular la venta: " + ex.Message + "\n" + ex.StackTrace;
        }
    }
}
