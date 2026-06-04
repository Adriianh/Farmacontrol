using Avalonia.Controls.Templates;
using Avalonia.Input;
using Farmacontrol.Core.Model;
using Farmacontrol.Desktop.States;
using Microsoft.Extensions.DependencyInjection;

namespace Farmacontrol.Desktop.Views.Sales;

public class SalesHistoryView()
    : ViewBase<SalesHistoryState>(Program.ServiceProvider.GetRequiredService<SalesHistoryState>())
{
    private static readonly ISolidColorBrush BackgroundPrimary = SolidColorBrush.Parse("#0F172A");
    private static readonly ISolidColorBrush BackgroundSecondary = SolidColorBrush.Parse("#1E293B");
    private static readonly ISolidColorBrush BackgroundTertiary = SolidColorBrush.Parse("#334155");
    private static readonly ISolidColorBrush TextMuted = SolidColorBrush.Parse("#94A3B8");
    private static readonly ISolidColorBrush TextSubtle = SolidColorBrush.Parse("#64748B");
    private static readonly ISolidColorBrush SuccessGreen = SolidColorBrush.Parse("#10B981");
    private static readonly ISolidColorBrush ErrorRed = SolidColorBrush.Parse("#EF4444");

    protected override object Build(SalesHistoryState state)
    {
        return new Border()
            .Background(BackgroundPrimary)
            .Padding(20)
            .CornerRadius(12)
            .Child(
                new Grid()
                    .Rows("Auto, Auto, *")
                    .Background(BackgroundPrimary)
                    .Children(
                        BuildHeader().Row(0),
                        BuildToolbar(state).Row(1),
                        BuildSalesList(state).Row(2)
                    )
                );
    }

    private Control BuildHeader()
    {
        return new Grid().Cols("*, Auto")
            .Children(
                new StackPanel().VerticalAlignment(VerticalAlignment.Center)
                    .Children(
                        new TextBlock()
                            .Text("Historial de Ventas")
                            .FontSize(26)
                            .FontWeight(FontWeight.Bold)
                            .Foreground(Brushes.White),
                        new TextBlock()
                            .Text("Consulta rápida de todas las transacciones")
                            .FontSize(13)
                            .Foreground(TextMuted)
                            .Margin(0, 4, 0, 0)
                    )
            ).Margin(0, 0, 0, 24);
    }

    private Control BuildToolbar(SalesHistoryState state)
    {
        var headerRow = new Grid()
            .Cols("*, Auto, Auto")
            .Children(
                new TextBox()
                    .PlaceholderText("Buscar por código o cliente...")
                    .Text(state, s => s.SearchQuery, BindingMode.TwoWay)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .CornerRadius(8)
                    .Col(0),
                new CalendarDatePicker()
                    .SelectedDate(state, s => s.StartDate, BindingMode.TwoWay)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .PlaceholderText("Fecha Inicio")
                    .Width(130)
                    .Margin(12, 0, 0, 0)
                    .Col(1),
                new CalendarDatePicker()
                    .SelectedDate(state, s => s.EndDate, BindingMode.TwoWay)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .PlaceholderText("Fecha Fin")
                    .Width(130)
                    .Margin(12, 0, 0, 0)
                    .Col(2)
            ).Margin(0, 0, 0, 16);

        return headerRow;
    }

    private Control BuildSalesList(SalesHistoryState state)
    {
        var list = new ListBox()
            .ItemsSource(state.SalesList)
            .ItemTemplate(new FuncDataTemplate<Sale>((sale, _) =>
            {
                var detailsButton = new Button()
                    .Content("Detalles")
                    .Background(BackgroundTertiary)
                    .Foreground(Brushes.White)
                    .CornerRadius(6)
                    .Padding(8, 4)
                    .Cursor(new Cursor(StandardCursorType.Hand));

                var flyout = new Flyout
                {
                    Placement = PlacementMode.BottomEdgeAlignedRight,
                    ShowMode = FlyoutShowMode.Standard,
                    Content = BuildSaleDetailsPanel(sale)
                };

                detailsButton.Flyout(flyout);

                return new Border().Background(BackgroundSecondary).CornerRadius(8).Padding(16).Margin(0, 0, 0, 8)
                    .Child(
                        new Grid().Cols("Auto, *, Auto, Auto, Auto")
                            .Children(
                                new StackPanel().VerticalAlignment(VerticalAlignment.Center).Col(0).Margin(0, 0, 16, 0)
                                    .Children(
                                        new TextBlock().Text($"#{sale.Code}").FontWeight(FontWeight.Bold)
                                            .Foreground(Brushes.White),
                                        new TextBlock().Text(sale.Date.ToString("dd/MM/yyyy HH:mm")).FontSize(12)
                                            .Foreground(TextMuted)
                                    ),
                                new StackPanel().VerticalAlignment(VerticalAlignment.Center).Col(1)
                                    .Children(
                                        new TextBlock().Text(sale.ClientName ?? "Cliente de Contado")
                                            .FontWeight(FontWeight.SemiBold).Foreground(Brushes.White),
                                        new TextBlock().Text($"Total: Q{sale.Total:F2}").FontSize(14)
                                            .Foreground(SuccessGreen).FontWeight(FontWeight.Bold)
                                    ),
                                new Border()
                                    .Background(sale.IsVoided
                                        ? SolidColorBrush.Parse("#451A1A")
                                        : SolidColorBrush.Parse("#1A4526")).CornerRadius(16).Padding(8, 4).Col(2)
                                    .VerticalAlignment(VerticalAlignment.Center).Margin(0, 0, 16, 0)
                                    .Child(new TextBlock().Text(sale.IsVoided ? "Anulada" : "Completada")
                                        .Foreground(sale.IsVoided ? ErrorRed : SuccessGreen).FontSize(12)
                                        .FontWeight(FontWeight.Bold)),
                                new TextBlock().Text(sale.PaymentMethod.ToString()).Foreground(TextSubtle)
                                    .VerticalAlignment(VerticalAlignment.Center).Col(3).Margin(0, 0, 16, 0),
                                detailsButton.Col(4).VerticalAlignment(VerticalAlignment.Center)
                            )
                    );
            }))
            .Background(Brushes.Transparent)
            .BorderBrush(Brushes.Transparent);

        return list;
    }

    public static Control BuildSaleDetailsPanel(Sale sale)
    {
        var itemsPanel = new StackPanel().Spacing(8);

        itemsPanel.Children.Add(
            new Grid().Cols("*, Auto, Auto").Children(
                new TextBlock().Text("Producto").Foreground(TextMuted).FontWeight(FontWeight.Bold).Col(0),
                new TextBlock().Text("Cant.").Foreground(TextMuted).FontWeight(FontWeight.Bold).Col(1).Margin(16, 0),
                new TextBlock().Text("Subtotal").Foreground(TextMuted).FontWeight(FontWeight.Bold).Col(2)
            )
        );

        itemsPanel.Children.Add(new Separator { Background = SolidColorBrush.Parse("#475569"), Height = 1 });

        foreach (var detail in sale.Details)
        {
            itemsPanel.Children.Add(
                new Grid().Cols("*, Auto, Auto").Children(
                    new TextBlock().Text(detail.ProductName).Foreground(Brushes.White).Col(0),
                    new TextBlock().Text(detail.Quantity.ToString()).Foreground(Brushes.White).Col(1).Margin(16, 0),
                    new TextBlock().Text($"Q{detail.Subtotal:F2}").Foreground(SuccessGreen).Col(2)
                )
            );
        }

        return new Border()
            .Background(BackgroundSecondary)
            .BorderBrush(SolidColorBrush.Parse("#475569"))
            .BorderThickness(1)
            .CornerRadius(8)
            .Padding(16)
            .MinWidth(300)
            .MaxWidth(400)
            .Child(
                new StackPanel().Spacing(12).Children(
                    new TextBlock().Text($"Detalles de Venta #{sale.Code}").FontSize(16).FontWeight(FontWeight.Bold)
                        .Foreground(Brushes.White),
                    itemsPanel,
                    new Separator { Background = SolidColorBrush.Parse("#475569"), Height = 1 },
                    new Grid().Cols("*, Auto").Children(
                        new TextBlock().Text("Total:").FontWeight(FontWeight.Bold).Foreground(Brushes.White).Col(0),
                        new TextBlock().Text($"Q{sale.Total:F2}").FontWeight(FontWeight.Bold).Foreground(SuccessGreen)
                            .Col(1)
                    )
                )
            );
    }
}