using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Farmacontrol.Core.Model;
using Farmacontrol.Core.Services;
using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Farmacontrol.Desktop.States;

public partial class SalesReportState : ObservableObject
{
    private readonly SalesService _salesService;

    [ObservableProperty] private DateTime? _startDate = DateTime.Today.AddDays(-30);
    [ObservableProperty] private DateTime? _endDate = DateTime.Today;
    [ObservableProperty] private string _paymentMethodFilter = "Todos";
    [ObservableProperty] private bool _includeVoided = false;

    [ObservableProperty] private decimal _totalRevenue;
    [ObservableProperty] private int _totalSalesCount;
    [ObservableProperty] private decimal _averageTicket;

    public string FormattedTotalSalesCount => TotalSalesCount.ToString();
    public string FormattedTotalRevenue => $"Q{TotalRevenue:F2}";
    public string FormattedAverageTicket => $"Q{AverageTicket:F2}";

    partial void OnTotalRevenueChanged(decimal value) => OnPropertyChanged(nameof(FormattedTotalRevenue));
    partial void OnTotalSalesCountChanged(int value) => OnPropertyChanged(nameof(FormattedTotalSalesCount));
    partial void OnAverageTicketChanged(decimal value) => OnPropertyChanged(nameof(FormattedAverageTicket));

    public ObservableCollection<Sale> ReportSales { get; } = new();
    public ObservableCollection<ProductSalesData> TopProducts { get; } = new();

    public string[] PaymentMethods { get; } = { "Todos", "Efectivo", "Tarjeta Crédito", "Tarjeta Débito", "Transferencia" };

    public SalesReportState(SalesService salesService)
    {
        _salesService = salesService;
        GenerateReport();
    }

    [RelayCommand]
    private void GenerateReport()
    {
        var allSales = _salesService.GetAllSales();
        var start = StartDate?.Date ?? DateTime.MinValue;
        var end = EndDate?.Date.AddDays(1).AddTicks(-1) ?? DateTime.MaxValue;

        var filtered = allSales.Where(s => s.Date >= start && s.Date <= end);

        if (!IncludeVoided)
        {
            filtered = filtered.Where(s => !s.IsVoided);
        }

        if (PaymentMethodFilter != "Todos")
        {
            PaymentMethod? method = PaymentMethodFilter switch
            {
                "Efectivo" => PaymentMethod.Cash,
                "Tarjeta Crédito" => PaymentMethod.CreditCard,
                "Tarjeta Débito" => PaymentMethod.DebitCard,
                "Transferencia" => PaymentMethod.Transfer,
                _ => null
            };

            if (method.HasValue)
            {
                filtered = filtered.Where(s => s.PaymentMethod == method.Value);
            }
        }

        var salesList = filtered.OrderByDescending(s => s.Date).ToList();

        ReportSales.Clear();
        foreach (var sale in salesList)
        {
            ReportSales.Add(sale);
        }

        TotalSalesCount = salesList.Count;
        TotalRevenue = salesList.Sum(s => s.Total);
        AverageTicket = TotalSalesCount > 0 ? TotalRevenue / TotalSalesCount : 0;

        CalculateTopProducts(salesList);
    }

    private void CalculateTopProducts(List<Sale> sales)
    {
        var productGroups = sales.SelectMany(s => s.Details)
            .GroupBy(d => d.ProductCode)
            .Select(g => new ProductSalesData
            {
                ProductCode = g.Key,
                ProductName = g.First().ProductName,
                QuantitySold = g.Sum(d => d.Quantity),
                TotalRevenue = g.Sum(d => d.Subtotal)
            })
            .OrderByDescending(p => p.QuantitySold)
            .Take(5)
            .ToList();

        TopProducts.Clear();
        foreach (var p in productGroups)
        {
            TopProducts.Add(p);
        }
    }

    [RelayCommand]
    private void ExportExcel()
    {
        try
        {
            var path = $"ReporteVentas_{DateTime.Now:yyyyMMddHHmmss}.xlsx";
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Ventas");

            worksheet.Cell(1, 1).Value = "Código";
            worksheet.Cell(1, 2).Value = "Fecha";
            worksheet.Cell(1, 3).Value = "Cliente";
            worksheet.Cell(1, 4).Value = "Método de Pago";
            worksheet.Cell(1, 5).Value = "Total";
            worksheet.Cell(1, 6).Value = "Estado";

            int row = 2;
            foreach (var sale in ReportSales)
            {
                worksheet.Cell(row, 1).Value = sale.Code;
                worksheet.Cell(row, 2).Value = sale.Date.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cell(row, 3).Value = sale.ClientName ?? "Contado";
                worksheet.Cell(row, 4).Value = sale.PaymentMethod.ToString();
                worksheet.Cell(row, 5).Value = sale.Total;
                worksheet.Cell(row, 6).Value = sale.IsVoided ? "Anulada" : "Completada";
                row++;
            }

            worksheet.Columns().AdjustToContents();
            workbook.SaveAs(path);
        }
        catch (Exception)
        {
            // Log or handle error
        }
    }

    [RelayCommand]
    private void ExportPdf()
    {
        try
        {
            QuestPDF.Settings.License = LicenseType.Community;
            var path = $"ReporteVentas_{DateTime.Now:yyyyMMddHHmmss}.pdf";
            
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(QuestPDF.Helpers.Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Text("Reporte de Ventas")
                        .SemiBold().FontSize(20).FontColor(QuestPDF.Helpers.Colors.Blue.Darken2);

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                    {
                        x.Item().Text($"Periodo: {StartDate?.Date:dd/MM/yyyy} - {EndDate?.Date:dd/MM/yyyy}");
                        x.Item().Text($"Total Vendido: Q{TotalRevenue:F2}");
                        x.Item().Text($"Cantidad de Ventas: {TotalSalesCount}");
                        x.Item().PaddingTop(1, Unit.Centimetre).Table(t =>
                        {
                            t.ColumnsDefinition(c =>
                            {
                                c.RelativeColumn();
                                c.RelativeColumn(2);
                                c.RelativeColumn(2);
                                c.RelativeColumn();
                                c.RelativeColumn();
                            });

                            t.Header(h =>
                            {
                                h.Cell().Text("Código").SemiBold();
                                h.Cell().Text("Fecha").SemiBold();
                                h.Cell().Text("Cliente").SemiBold();
                                h.Cell().Text("Pago").SemiBold();
                                h.Cell().Text("Total").SemiBold();
                            });

                            foreach (var sale in ReportSales)
                            {
                                t.Cell().Text(sale.Code.ToString());
                                t.Cell().Text(sale.Date.ToString("dd/MM/yyyy HH:mm"));
                                t.Cell().Text(sale.ClientName ?? "Contado");
                                t.Cell().Text(sale.PaymentMethod.ToString());
                                t.Cell().Text($"Q{sale.Total:F2}");
                            }
                        });
                    });
                });
            }).GeneratePdf(path);
        }
        catch (Exception)
        {
            // Log or handle error
        }
    }

    partial void OnStartDateChanged(DateTime? value) => GenerateReport();
    partial void OnEndDateChanged(DateTime? value) => GenerateReport();
    partial void OnPaymentMethodFilterChanged(string value) => GenerateReport();
    partial void OnIncludeVoidedChanged(bool value) => GenerateReport();
}

public class ProductSalesData
{
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal TotalRevenue { get; set; }
}
