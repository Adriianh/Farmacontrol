using Avalonia.Controls.Templates;
using Avalonia.Input;
using Farmacontrol.Core.Model;
using Farmacontrol.Desktop.States;
using Microsoft.Extensions.DependencyInjection;

namespace Farmacontrol.Desktop.Views.Sales;

public class VoidSaleView() : ViewBase<VoidSaleState>(Program.ServiceProvider.GetRequiredService<VoidSaleState>())
{
    private static readonly SolidColorBrush BackgroundPrimary = SolidColorBrush.Parse("#111827");
    private static readonly SolidColorBrush BackgroundSecondary = SolidColorBrush.Parse("#1F2937");
    private static readonly SolidColorBrush TextMuted = SolidColorBrush.Parse("#9CA3AF");
    private static readonly SolidColorBrush BorderColor = SolidColorBrush.Parse("#374151");
    private static readonly SolidColorBrush ErrorRed = SolidColorBrush.Parse("#EF4444");
    private static readonly SolidColorBrush SuccessGreen = SolidColorBrush.Parse("#10B981");

    protected override object Build(VoidSaleState state)
    {
        return new Border()
            .Background(BackgroundPrimary)
            .Padding(20)
            .CornerRadius(12)
            .Child(
                new Grid().Cols("1*, 1*")
                    .Children(
                        BuildLeftPanel(state).Col(0),
                        BuildRightPanel(state).Col(1)
                    )
                );
    }

    private Control BuildLeftPanel(VoidSaleState state)
    {
        var searchBox = new TextBox()
            .With(t => t.PlaceholderText = "Ingrese el número de ticket (Ej. 1001)")
            .Text(state, s => s.SearchQuery, BindingMode.TwoWay)
            .FontSize(14).Padding(12).CornerRadius(8);

        var searchButton = new Button()
            .Content("Buscar")
            .Command(state.SearchSaleCommand)
            .Background(SolidColorBrush.Parse("#3B82F6"))
            .Foreground(Brushes.White).Padding(16, 10).CornerRadius(8)
            .FontWeight(FontWeight.SemiBold).Cursor(new Cursor(StandardCursorType.Hand));

        var salesListBox = new ListBox()
            .ItemsSource(state.SalesList)
            .SelectedItem(state, s => s.SelectedListSale, BindingMode.TwoWay)
            .ItemTemplate(new FuncDataTemplate<Sale>((sale, _) =>
            {
                if (sale == null) return new Border();

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

                return new Border().Padding(8).Child(
                    new Grid().Cols("Auto, *, Auto, Auto").Children(
                        new TextBlock().Text($"#{sale.Code}").FontWeight(FontWeight.Bold).Foreground(Brushes.White)
                            .Col(0).Margin(0, 0, 12, 0),
                        new TextBlock().Text(sale.ClientName ?? "Cliente de Contado").Foreground(TextMuted).Col(1),
                        new TextBlock().Text($"Q{sale.Total:F2}").Foreground(SuccessGreen).Col(2).Margin(0, 0, 12, 0),
                        detailsButton.Col(3)
                    )
                );
            }))
            .Background(BackgroundSecondary)
            .CornerRadius(8)
            .Height(200);

        return new Border().Background(BackgroundSecondary).CornerRadius(12).Padding(24).Margin(16)
            .Child(
                new StackPanel().Spacing(16).Children(
                    new TextBlock().Text("Buscar Venta").FontSize(20).FontWeight(FontWeight.Bold)
                        .Foreground(Brushes.White),
                    new TextBlock().Text("Busque la venta por su número de ticket o nombre de cliente.")
                        .Foreground(TextMuted).TextWrapping(TextWrapping.Wrap),
                    new Grid().Cols("*, Auto").Children(
                        searchBox.Col(0), searchButton.Col(1)
                    ),
                    salesListBox,
                    new TextBlock()
                        .Text(state, s => s.ErrorMessage)
                        .Foreground(ErrorRed)
                        .IsVisible(state, s => s.HasError),
                    new TextBlock()
                        .Text(state, s => s.SuccessMessage)
                        .Foreground(SuccessGreen)
                        .IsVisible(state, s => s.HasSuccess),
                    new Border().BorderBrush(BorderColor).BorderThickness(1).CornerRadius(8).Padding(16)
                        .IsVisible(state, s => s.HasSelectedSale)
                        .Child(
                            new StackPanel().Spacing(8).Children(
                                new TextBlock().Text("Detalles de la Venta").FontSize(16)
                                    .FontWeight(FontWeight.SemiBold).Foreground(Brushes.White),
                                new Grid().Cols("Auto, *").Rows("Auto, Auto, Auto, Auto").Children(
                                    new TextBlock().Text("Ticket:").Foreground(TextMuted).Row(0).Col(0)
                                        .Margin(0, 4, 12, 4),
                                    new TextBlock().Text(state, s => s.SelectedSaleCode).Foreground(Brushes.White)
                                        .Row(0).Col(1).Margin(0, 4, 0, 4),
                                    new TextBlock().Text("Fecha:").Foreground(TextMuted).Row(1).Col(0)
                                        .Margin(0, 4, 12, 4),
                                    new TextBlock().Text(state, s => s.SelectedSaleDate).Foreground(Brushes.White)
                                        .Row(1).Col(1).Margin(0, 4, 0, 4),
                                    new TextBlock().Text("Método de Pago:").Foreground(TextMuted).Row(2).Col(0)
                                        .Margin(0, 4, 12, 4),
                                    new TextBlock().Text(state, s => s.SelectedSalePayment).Foreground(Brushes.White)
                                        .Row(2).Col(1).Margin(0, 4, 0, 4),
                                    new TextBlock().Text("Total:").Foreground(TextMuted).Row(3).Col(0)
                                        .Margin(0, 4, 12, 4),
                                    new TextBlock().Text(state, s => s.SelectedSaleTotal).Foreground(SuccessGreen)
                                        .FontWeight(FontWeight.Bold).Row(3).Col(1).Margin(0, 4, 0, 4)
                                )
                            )
                        )
                )
            );
    }

    private Control BuildRightPanel(VoidSaleState state)
    {
        var reasonsDropdown = new ComboBox()
            .With(c => c.PlaceholderText = "Seleccione una razón")
            .ItemsSource(state.VoidReasons)
            .SelectedItem(state, s => s.SelectedVoidReason, BindingMode.TwoWay)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .CornerRadius(8)
            .Margin(0, 0, 0, 16);

        var detailsBox = new TextBox()
            .With(t => t.PlaceholderText = "Escriba la justificación detallada de la anulación...")
            .Text(state, s => s.VoidDetails, BindingMode.TwoWay)
            .AcceptsReturn(true)
            .TextWrapping(TextWrapping.Wrap)
            .Height(120)
            .CornerRadius(8)
            .Margin(0, 0, 0, 24);

        var confirmButton = new Button()
            .Content("Confirmar Anulación")
            .Command(state.ConfirmVoidCommand)
            .Background(ErrorRed)
            .Foreground(Brushes.White).Padding(16)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .HorizontalContentAlignment(HorizontalAlignment.Center)
            .CornerRadius(8).FontWeight(FontWeight.Bold)
            .Cursor(new Cursor(StandardCursorType.Hand));

        return new Border().Background(BackgroundSecondary).CornerRadius(12).Padding(24).Margin(0, 16, 16, 16)
            .Child(
                new StackPanel().Spacing(12)
                    .IsEnabled(state, s => s.HasSelectedSale)
                    .Children(
                        new TextBlock().Text("Proceso de Anulación").FontSize(20).FontWeight(FontWeight.Bold)
                            .Foreground(Brushes.White),
                        new Border().Background(SolidColorBrush.Parse("#FEF2F2")).BorderBrush(ErrorRed)
                            .BorderThickness(1).CornerRadius(8).Padding(12).Margin(0, 0, 0, 16)
                            .Child(new TextBlock()
                                .Text(
                                    "ADVERTENCIA: Esta acción es irreversible. Se registrará la razón en la bitácora de auditoría y se alterará el inventario en base a la razón seleccionada.")
                                .Foreground(SolidColorBrush.Parse("#991B1B")).TextWrapping(TextWrapping.Wrap)
                                .FontSize(12)),
                        new TextBlock().Text("Motivo de Anulación *").Foreground(TextMuted)
                            .FontWeight(FontWeight.SemiBold),
                        reasonsDropdown,
                        new TextBlock().Text("Detalles / Justificación *").Foreground(TextMuted)
                            .FontWeight(FontWeight.SemiBold),
                        detailsBox,
                        confirmButton
                    )
            );
    }
}