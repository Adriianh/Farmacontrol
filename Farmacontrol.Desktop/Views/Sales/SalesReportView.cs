using System.Linq.Expressions;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Farmacontrol.Core.Model;
using Farmacontrol.Desktop.States;
using Microsoft.Extensions.DependencyInjection;

namespace Farmacontrol.Desktop.Views.Sales;

public class SalesReportView()
    : ViewBase<SalesReportState>(Program.ServiceProvider.GetRequiredService<SalesReportState>())
{
    private static readonly SolidColorBrush BackgroundPrimary = SolidColorBrush.Parse("#111827");
    private static readonly SolidColorBrush BackgroundSecondary = SolidColorBrush.Parse("#1F2937");
    private static readonly SolidColorBrush BackgroundTertiary = SolidColorBrush.Parse("#374151");
    private static readonly SolidColorBrush TextMuted = SolidColorBrush.Parse("#9CA3AF");
    private static readonly SolidColorBrush BorderColor = SolidColorBrush.Parse("#374151");
    private static readonly SolidColorBrush SuccessGreen = SolidColorBrush.Parse("#10B981");
    private static readonly SolidColorBrush AccentBlue = SolidColorBrush.Parse("#3B82F6");
    private static readonly SolidColorBrush DangerRed = SolidColorBrush.Parse("#EF4444");

    protected override object Build(SalesReportState state)
    {
        return new Border()
            .Background(BackgroundPrimary)
            .CornerRadius(12)
            .Padding(20)
            .Child(
                new Grid().Rows("Auto, *")
                    .Children(
                        BuildToolbar(state).Row(0),
                        BuildDashboard(state).Row(1)
                    )
            );
    }

    private Control BuildToolbar(SalesReportState state)
    {
        var headerRow = new Grid()
            .Cols("*, Auto, Auto")
            .Children(
                new TextBlock()
                    .Text("Reportes de Ventas")
                    .FontSize(20)
                    .FontWeight(FontWeight.Bold)
                    .Foreground(Brushes.White)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Col(0),
                
                new Button()
                    .Content("Exportar PDF")
                    .Command(state.ExportPdfCommand)
                    .Background(DangerRed)
                    .Foreground(Brushes.White)
                    .CornerRadius(8)
                    .Padding(16, 8)
                    .FontWeight(FontWeight.SemiBold)
                    .Cursor(new Cursor(StandardCursorType.Hand))
                    .Col(1)
                    .Margin(0, 0, 8, 0),

                new Button()
                    .Content("Exportar Excel")
                    .Command(state.ExportExcelCommand)
                    .Background(SuccessGreen)
                    .Foreground(Brushes.White)
                    .CornerRadius(8)
                    .Padding(16, 8)
                    .FontWeight(FontWeight.SemiBold)
                    .Cursor(new Cursor(StandardCursorType.Hand))
                    .Col(2)
            );

        var filters = new WrapPanel()
            .HorizontalAlignment(HorizontalAlignment.Left)
            .Margin(0, 8, 0, 0)
            .Children(
                new StackPanel().Orientation(Orientation.Horizontal).Spacing(12).Children(
                    new TextBlock().Text("Desde:").VerticalAlignment(VerticalAlignment.Center).Foreground(TextMuted),
                    new CalendarDatePicker()
                        .SelectedDate(state, s => s.StartDate, BindingMode.TwoWay)
                        .VerticalAlignment(VerticalAlignment.Center)
                        .Width(130),
                        
                    new TextBlock().Text("Hasta:").VerticalAlignment(VerticalAlignment.Center).Foreground(TextMuted).Margin(8, 0, 0, 0),
                    new CalendarDatePicker()
                        .SelectedDate(state, s => s.EndDate, BindingMode.TwoWay)
                        .VerticalAlignment(VerticalAlignment.Center)
                        .Width(130),

                    new TextBlock().Text("Pago:").VerticalAlignment(VerticalAlignment.Center).Foreground(TextMuted).Margin(8, 0, 0, 0),
                    new ComboBox()
                        .ItemsSource(state.PaymentMethods)
                        .SelectedItem(state, s => s.PaymentMethodFilter, BindingMode.TwoWay)
                        .VerticalAlignment(VerticalAlignment.Center)
                        .Width(140),

                    new ToggleSwitch()
                        .OffContent("Excluir anuladas")
                        .OnContent("Incluir anuladas")
                        .IsChecked(state, s => s.IncludeVoided, BindingMode.TwoWay)
                        .VerticalAlignment(VerticalAlignment.Center)
                        .Margin(16, 0, 0, 0)
                )
            );

        var expander = new Expander()
            .Header("Filtros de Búsqueda")
            .Content(filters)
            .Background(BackgroundSecondary)
            .BorderBrush(BorderColor)
            .BorderThickness(1)
            .CornerRadius(8)
            .IsExpanded(true);

        return new Border()
            .Background(BackgroundSecondary)
            .CornerRadius(12)
            .Padding(20)
            .Margin(0, 0, 0, 20)
            .Child(
                new StackPanel()
                    .Spacing(16)
                    .Children(headerRow, expander)
            );
    }

    private Control BuildDashboard(SalesReportState state)
    {
        return new Grid()
            .Cols("3*, 1*")
            .ColumnSpacing(20)
            .Children(
                new Grid()
                    .Rows("Auto, *")
                    .Col(0)
                    .Children(
                        BuildSummaryCards(state).Row(0).Margin(0, 0, 0, 20),
                        BuildSalesList(state).Row(1)
                    ),
                BuildTopProducts(state).Col(1)
            );
    }

    private Control BuildSummaryCards(SalesReportState state)
    {
        return new Grid()
            .Cols("1*, 1*, 1*")
            .ColumnSpacing(16)
            .Children(
                BuildCard("Ventas Totales", state, s => s.FormattedTotalSalesCount, AccentBlue).Col(0),
                BuildCard("Ingresos Totales", state, s => s.FormattedTotalRevenue, SuccessGreen).Col(1),
                BuildCard("Ticket Promedio", state, s => s.FormattedAverageTicket, SolidColorBrush.Parse("#F59E0B")).Col(2)
            );
    }

    private Control BuildCard(string title, SalesReportState state,
        Expression<Func<SalesReportState, string>> binding, SolidColorBrush color)
    {
        return new Border()
            .Background(BackgroundSecondary)
            .CornerRadius(12)
            .Padding(24)
            .Child(
                new StackPanel()
                    .Spacing(8)
                    .Children(
                        new TextBlock()
                            .Text(title)
                            .Foreground(TextMuted)
                            .FontWeight(FontWeight.SemiBold),
                        new TextBlock()
                            .Text(state, binding)
                            .FontSize(28)
                            .FontWeight(FontWeight.Bold)
                            .Foreground(color)
                    )
            );
    }

    private Control BuildSalesList(SalesReportState state)
    {
        var dataGrid = new ListBox()
            .ItemsSource(state.ReportSales)
            .Background(BackgroundSecondary)
            .CornerRadius(12)
            .Padding(8)
            .ItemTemplate(new FuncDataTemplate<Sale>((sale, _) =>
            {
                var detailsButton = new Button()
                    .Content("Detalles")
                    .Background(SolidColorBrush.Parse("#334155"))
                    .Foreground(Brushes.White)
                    .CornerRadius(6)
                    .Padding(8, 4)
                    .Cursor(new Cursor(StandardCursorType.Hand));

                var flyout = new Flyout
                {
                    Placement = PlacementMode.BottomEdgeAlignedRight,
                    ShowMode = FlyoutShowMode.Standard,
                    Content = SalesHistoryView.BuildSaleDetailsPanel(sale)
                };
                detailsButton.Flyout(flyout);

                return new Border()
                    .Padding(12)
                    .BorderThickness(0, 0, 0, 1)
                    .BorderBrush(BorderColor)
                    .Child(
                        new Grid()
                            .Cols("1*, 2*, 2*, 1*, 1*, 1*, Auto")
                            .Children(
                                new TextBlock()
                                    .Text($"#{sale.Code}")
                                    .FontWeight(FontWeight.Bold)
                                    .Foreground(Brushes.White)
                                    .Col(0),
                                new TextBlock()
                                    .Text(sale.Date.ToString("dd/MM/yyyy HH:mm"))
                                    .Foreground(TextMuted)
                                    .Col(1),
                                new TextBlock()
                                    .Text(sale.ClientName ?? "Contado")
                                    .Foreground(Brushes.White)
                                    .Col(2),
                                new TextBlock()
                                    .Text(sale.PaymentMethod.ToString())
                                    .Foreground(TextMuted)
                                    .Col(3),
                                new TextBlock()
                                    .Text(sale.IsVoided ? "Anulada" : "Ok")
                                    .Foreground(sale.IsVoided ? DangerRed : SuccessGreen)
                                    .Col(4),
                                new TextBlock()
                                    .Text($"Q{sale.Total:F2}")
                                    .FontWeight(FontWeight.Bold)
                                    .Foreground(Brushes.White)
                                    .Col(5),
                                detailsButton.Col(6).Margin(12, 0, 0, 0)
                            )
                    );
            }));

        return new Border()
            .Background(BackgroundSecondary)
            .CornerRadius(12)
            .Padding(20)
            .Child(
                new Grid()
                    .Rows("Auto, Auto, *")
                    .Children(
                        new TextBlock()
                            .Text("Historial de Ventas")
                            .FontSize(18)
                            .FontWeight(FontWeight.Bold)
                            .Foreground(Brushes.White)
                            .Row(0).Margin(0, 0, 0, 16),
                        new Border()
                            .Padding(12, 0)
                            .Child(
                                new Grid()
                                    .Cols("1*, 2*, 2*, 1*, 1*, 1*, Auto")
                                    .Children(
                                        new TextBlock().Text("Ticket").Foreground(TextMuted).FontWeight(FontWeight.SemiBold).Col(0),
                                        new TextBlock().Text("Fecha").Foreground(TextMuted).FontWeight(FontWeight.SemiBold).Col(1),
                                        new TextBlock().Text("Cliente").Foreground(TextMuted).FontWeight(FontWeight.SemiBold).Col(2),
                                        new TextBlock().Text("Pago").Foreground(TextMuted).FontWeight(FontWeight.SemiBold).Col(3),
                                        new TextBlock().Text("Estado").Foreground(TextMuted).FontWeight(FontWeight.SemiBold).Col(4),
                                        new TextBlock().Text("Total").Foreground(TextMuted).FontWeight(FontWeight.SemiBold).Col(5),
                                        new TextBlock().Text("Acciones").Foreground(TextMuted).FontWeight(FontWeight.SemiBold).Col(6).Margin(12, 0, 0, 0)
                                    )
                            ).Row(1).Margin(0, 0, 0, 8),
                        dataGrid.Row(2)
                    )
            );
    }

    private Control BuildTopProducts(SalesReportState state)
    {
        var list = new ItemsControl()
            .ItemsSource(state.TopProducts)
            .ItemTemplate(new FuncDataTemplate<ProductSalesData>((p, _) =>
                new StackPanel()
                    .Spacing(8)
                    .Margin(0, 0, 0, 20)
                    .Children(
                        new Grid()
                            .Cols("*, Auto")
                            .Children(
                                new TextBlock()
                                    .Text(p.ProductName)
                                    .Foreground(Brushes.White)
                                    .FontWeight(FontWeight.SemiBold)
                                    .TextTrimming(TextTrimming.CharacterEllipsis)
                                    .Col(0),
                                new TextBlock()
                                    .Text($"x{p.QuantitySold}")
                                    .Foreground(AccentBlue)
                                    .FontWeight(FontWeight.Bold)
                                    .Col(1)
                            ),
                        new TextBlock()
                            .Text($"Q{p.TotalRevenue:F2}")
                            .Foreground(SuccessGreen)
                            .FontSize(12),
                        new Border()
                            .Height(6)
                            .CornerRadius(3)
                            .Background(BackgroundTertiary)
                            .Child(
                                new Border()
                                    .Background(AccentBlue)
                                    .CornerRadius(3)
                                    .HorizontalAlignment(HorizontalAlignment.Left)
                                    .Width(Math.Min(200, p.QuantitySold * 10))
                            )
                    )
            ));

        return new Border()
            .Background(BackgroundSecondary)
            .CornerRadius(12)
            .Padding(24)
            .Child(
                new StackPanel()
                    .Spacing(24)
                    .Children(
                        new TextBlock()
                            .Text("Top 5 Productos")
                            .FontSize(18)
                            .FontWeight(FontWeight.Bold)
                            .Foreground(Brushes.White),
                        list
                    )
            );
    }
}