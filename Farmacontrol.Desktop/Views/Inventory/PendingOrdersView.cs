using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Styling;
using Farmacontrol.Core.Model;
using Farmacontrol.Desktop.States;

namespace Farmacontrol.Desktop.Views.Inventory;

public sealed class PendingOrdersView(PendingOrdersState state) : ViewBase<PendingOrdersState>(state)
{
    private static readonly SolidColorBrush BackgroundPrimary = SolidColorBrush.Parse("#111827");
    private static readonly SolidColorBrush BackgroundSecondary = SolidColorBrush.Parse("#1F2937");
    private static readonly SolidColorBrush BackgroundTertiary = SolidColorBrush.Parse("#374151");
    private static readonly SolidColorBrush BackgroundHover = SolidColorBrush.Parse("#4B5563");
    private static readonly SolidColorBrush TextMuted = SolidColorBrush.Parse("#9CA3AF");
    private static readonly SolidColorBrush AccentBlue = SolidColorBrush.Parse("#2563EB");
    private static readonly SolidColorBrush AccentGreen = SolidColorBrush.Parse("#10B981");
    private static readonly SolidColorBrush BorderColor = SolidColorBrush.Parse("#374151");
    private static readonly SolidColorBrush WarningYellow = SolidColorBrush.Parse("#F59E0B");

    protected override object Build(PendingOrdersState state) =>
        new Grid()
            .Children(
                new Border()
                    .Background(BackgroundPrimary)
                    .CornerRadius(12)
                    .Padding(20)
                    .Styles(
                        new Style(x => x.OfType<TextBox>().Class(":pointerover").Template().OfType<Border>())
                            { Setters = { new Setter(Border.BackgroundProperty, BackgroundTertiary) } },
                        new Style(x => x.OfType<TextBox>().Class(":focus").Template().OfType<Border>())
                            { Setters = { new Setter(Border.BackgroundProperty, BackgroundTertiary) } },
                        new Style(x => x.OfType<Button>().Class(":pointerover").Template().OfType<ContentPresenter>())
                        {
                            Setters =
                            {
                                new Setter(ContentPresenter.BackgroundProperty, BackgroundHover),
                                new Setter(ContentPresenter.ForegroundProperty, Brushes.White)
                            }
                        },
                        new Style(x => x.OfType<ComboBox>().Template().OfType<Border>())
                        {
                            Setters =
                            {
                                new Setter(Border.BackgroundProperty, BackgroundSecondary),
                                new Setter(Border.BorderBrushProperty, BorderColor)
                            }
                        },
                        new Style(x => x.OfType<ComboBoxItem>().Template().OfType<ContentPresenter>())
                        {
                            Setters = { new Setter(ContentPresenter.BackgroundProperty, BackgroundSecondary) }
                        },
                        new Style(x =>
                            x.OfType<ComboBoxItem>().Class(":pointerover").Template().OfType<ContentPresenter>())
                        {
                            Setters = { new Setter(ContentPresenter.BackgroundProperty, BackgroundTertiary) }
                        },
                        new Style(x =>
                            x.OfType<ComboBoxItem>().Class(":selected").Template().OfType<ContentPresenter>())
                        {
                            Setters = { new Setter(ContentPresenter.BackgroundProperty, AccentBlue) }
                        },
                        new Style(x => x.OfType<TabItem>())
                        {
                            Setters =
                            {
                                new Setter(TemplatedControl.BackgroundProperty, BackgroundSecondary),
                                new Setter(TemplatedControl.ForegroundProperty, TextMuted),
                                new Setter(TemplatedControl.PaddingProperty, new Thickness(16, 10)),
                                new Setter(TemplatedControl.FontSizeProperty, 14.0),
                                new Setter(TemplatedControl.FontWeightProperty, FontWeight.SemiBold)
                            }
                        },
                        new Style(x => x.OfType<TabItem>().Class(":selected"))
                        {
                            Setters =
                            {
                                new Setter(TemplatedControl.BackgroundProperty, AccentBlue),
                                new Setter(TemplatedControl.ForegroundProperty, Brushes.White)
                            }
                        },
                        new Style(x => x.OfType<TabItem>().Class(":pointerover").Class(":selected"))
                        {
                            Setters =
                            {
                                new Setter(TemplatedControl.BackgroundProperty, BackgroundHover),
                                new Setter(TemplatedControl.ForegroundProperty, Brushes.White)
                            }
                        },
                        new Style(x => x.OfType<TabControl>())
                        {
                            Setters =
                            {
                                new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent)
                            }
                        },
                        new Style(x => x.OfType<TabControl>().Template().OfType<TabStrip>())
                        {
                            Setters =
                            {
                                new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent),
                                new Setter(MarginProperty, new Thickness(0, 0, 0, 16))
                            }
                        }
                    )
                    .Child(
                        new TabControl()
                            .CornerRadius(12)
                            .Items(
                                new TabItem()
                                    .Header("📦 Generar Pedido")
                                    .CornerRadius(12)
                                    .Margin(2, 0, 8, 0)
                                    .Content(BuildGenerateOrderSection(state)),
                                new TabItem()
                                    .CornerRadius(12)
                                    .Header(
                                        new StackPanel().Orientation(Orientation.Horizontal).Spacing(8)
                                            .Children(
                                                new TextBlock().Text("🚚 Pedidos en Camino"),
                                                new Border()
                                                    .Background(SolidColorBrush.Parse("#EF4444"))
                                                    .CornerRadius(10)
                                                    .Padding(6, 2)
                                                    .IsVisible(state, x => x.HasPendingPurchases)
                                                    .Child(
                                                        new TextBlock()
                                                            .Text(state, x => x.PendingPurchasesCount)
                                                            .FontSize(11)
                                                            .FontWeight(FontWeight.Bold)
                                                            .Foreground(Brushes.White)
                                                    )
                                            )
                                    )
                                    .Content(BuildReceptionSection(state))
                            )
                    )
            );

    private Control BuildGenerateOrderSection(PendingOrdersState state) =>
        new Grid().Rows("Auto, *")
            .Children(
                BuildHeader().Row(0),
                new Panel().Row(1).Children(
                    BuildEmptyState().IsVisible(state, x => x.ShowEmpty),
                    BuildMainWorkspace(state).IsVisible(state, x => x.ShowSuggestions),
                    BuildSuccessState().IsVisible(state, x => x.ShowSuccess)
                )
            );

    private Control BuildHeader() =>
        new StackPanel().Margin(0, 0, 0, 20)
            .Children(
                new TextBlock().Text("Pedidos Pendientes y Reabastecimiento").FontSize(24).FontWeight(FontWeight.Bold)
                    .Foreground(Brushes.White),
                new TextBlock()
                    .Text(
                        "El sistema detecta automáticamente los productos agotados o debajo del stock mínimo de seguridad")
                    .FontSize(13).Foreground(TextMuted)
            );

    private Control BuildMainWorkspace(PendingOrdersState state) =>
        new Grid().Cols("*, 320")
            .Children(
                BuildSuggestionsTable(state).Col(0),
                BuildOrderPanel(state).Col(1).Margin(16, 0, 0, 0)
            );

    private Control BuildSuggestionsTable(PendingOrdersState state) =>
        new Grid().Rows("Auto, *")
            .Children(
                new TextBlock().Text("⚠️ Productos que requieren atención urgente").FontSize(14)
                    .FontWeight(FontWeight.SemiBold).Foreground(WarningYellow).Row(0).Margin(0, 0, 0, 10),
                new ScrollViewer().Row(1)
                    .Content(
                        new ItemsControl()
                            .ItemsSource(state, x => x.LowStockSuggestions)
                            .ItemTemplate(
                                new FuncDataTemplate<ProductOrderSuggestion>((item, _) =>
                                    new Border().Background(BackgroundSecondary).CornerRadius(8).Padding(12)
                                        .Margin(0, 0, 0, 8).BorderBrush(BorderColor).BorderThickness(1)
                                        .Child(
                                            new Grid().Cols("Auto, *, Auto, Auto")
                                                .Children(
                                                    new CheckBox().IsChecked(item.IsSelected)
                                                        .VerticalAlignment(VerticalAlignment.Center).Col(0)
                                                        .Margin(0, 0, 12, 0),
                                                    new StackPanel().Col(1).VerticalAlignment(VerticalAlignment.Center)
                                                        .Children(
                                                            new TextBlock().Text(item.ProductName).FontSize(14)
                                                                .FontWeight(FontWeight.Bold).Foreground(Brushes.White),
                                                            new TextBlock()
                                                                .Text(
                                                                    $"Stock Actual: {item.CurrentStock} u. | Mínimo Requerido: {item.MinStock} u.")
                                                                .FontSize(11).Foreground(TextMuted).Margin(0, 2, 0, 0)
                                                        ),
                                                    new Border().Background(SolidColorBrush.Parse("#78350F"))
                                                        .CornerRadius(4).Padding(6, 4).Margin(0, 0, 16, 0).Col(2)
                                                        .VerticalAlignment(VerticalAlignment.Center)
                                                        .Child(new TextBlock().Text("Bajo Mínimo").FontSize(10)
                                                            .FontWeight(FontWeight.Bold).Foreground(WarningYellow)),
                                                    new StackPanel().Orientation(Orientation.Horizontal).Col(3)
                                                        .VerticalAlignment(VerticalAlignment.Center)
                                                        .Children(
                                                            new TextBlock().Text("Pedir: ").FontSize(12)
                                                                .Foreground(TextMuted)
                                                                .VerticalAlignment(VerticalAlignment.Center)
                                                                .Margin(0, 0, 4, 0),
                                                            new TextBox().Text(item.SuggestedQuantity.ToString())
                                                                .Width(60).Padding(6, 4).Background(BackgroundPrimary)
                                                                .Foreground(Brushes.White).BorderBrush(BorderColor)
                                                                .CornerRadius(4)
                                                        )
                                                )
                                        )
                                ))
                    )
            );

    private Control BuildOrderPanel(PendingOrdersState state)
    {
        var noSuppliersWarning = new TextBlock()
            .Text(
                "⚠️ No hay proveedores activos registrados. Por favor, registre al menos un proveedor antes de generar órdenes.")
            .Foreground(SolidColorBrush.Parse("#FCA5A5"))
            .FontSize(12)
            .TextWrapping(TextWrapping.Wrap)
            .IsVisible(state, x => x.Suppliers.Count == 0);

        var supplierCombo = new ComboBox()
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .Background(BackgroundSecondary)
            .BorderBrush(BorderColor)
            .Foreground(Brushes.White)
            .PlaceholderText("Seleccione Proveedor...")
            .ItemsSource(state, x => x.Suppliers)
            .SelectedItem(state, x => x.SelectedSupplier, BindingMode.TwoWay)
            .IsEnabled(state, x => x.Suppliers.Count > 0)
            .ItemTemplate(new FuncDataTemplate<Supplier>((s, _) =>
            {
                var supplierName = s.Name;
                return new TextBlock()
                    .Text(supplierName)
                    .Foreground(Brushes.White);
            }));

        var generateOrderButton = new Button()
            .Content("📦 Generar Orden de Compra")
            .Background(AccentBlue)
            .Foreground(Brushes.White)
            .FontWeight(FontWeight.SemiBold)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .HorizontalContentAlignment(HorizontalAlignment.Center)
            .Padding(0, 12)
            .CornerRadius(6)
            .IsEnabled(state, x => x.Suppliers.Count > 0);

        generateOrderButton.Click += (_, _) => state.GeneratePurchaseOrder();

        return new Border().Background(BackgroundSecondary).CornerRadius(10).Padding(16)
            .BorderBrush(BorderColor).BorderThickness(1)
            .Child(
                new Grid().Rows("Auto, Auto, Auto, *, Auto")
                    .Children(
                        new TextBlock().Text("Resumen del Nuevo Pedido").FontSize(15).FontWeight(FontWeight.Bold)
                            .Foreground(Brushes.White).Row(0).Margin(0, 0, 0, 14),

                        noSuppliersWarning.Row(1).Margin(0, 0, 0, 16),
                        new StackPanel().Spacing(6).Row(2).Margin(0, 0, 0, 16)
                            .IsVisible(state, x => x.Suppliers.Count > 0)
                            .Children(
                                new TextBlock().Text("Destinatario (Proveedor)").FontSize(11).Foreground(TextMuted),
                                supplierCombo
                            ),
                        new Border().Background(SolidColorBrush.Parse("#7F1D1D")).CornerRadius(6).Padding(10).Row(3)
                            .IsVisible(state, x => x.HasErrorMessage)
                            .Child(new TextBlock().Text(state, x => x.ErrorMessage)
                                .Foreground(SolidColorBrush.Parse("#FCA5A5")).FontSize(11)
                                .TextWrapping(TextWrapping.Wrap)),
                        generateOrderButton.Row(4)
                    )
            );
    }

    private static Control BuildEmptyState() =>
        new StackPanel().VerticalAlignment(VerticalAlignment.Center)
            .HorizontalAlignment(HorizontalAlignment.Center).Spacing(10).Children(
                new TextBlock().Text("✨").FontSize(48).HorizontalAlignment(HorizontalAlignment.Center),
                new TextBlock().Text("¡Inventario al día!").FontSize(16).FontWeight(FontWeight.Bold)
                    .Foreground(AccentGreen).HorizontalAlignment(HorizontalAlignment.Center),
                new TextBlock().Text("No se detectaron productos por debajo del stock mínimo de seguridad.")
                    .FontSize(13).Foreground(TextMuted));

    private static Control BuildSuccessState() =>
        new StackPanel().VerticalAlignment(VerticalAlignment.Center)
            .HorizontalAlignment(HorizontalAlignment.Center).Spacing(10).Children(
                new TextBlock().Text("✅").FontSize(48).HorizontalAlignment(HorizontalAlignment.Center),
                new TextBlock().Text("¡Orden de compra generada exitosamente!").FontSize(16).FontWeight(FontWeight.Bold)
                    .Foreground(AccentGreen).HorizontalAlignment(HorizontalAlignment.Center),
                new TextBlock().Text("El registro quedó asentado y los productos se quitaron de la lista de alertas.")
                    .FontSize(13).Foreground(TextMuted));

    private Control BuildReceptionSection(PendingOrdersState state)
    {
        var pendingPurchasesText = $"Hay {state.PendingPurchasesCount} pedido(s) pendiente(s) de recepción";

        return new Grid().Rows("Auto, *")
            .Children(
                new StackPanel().Row(0).Margin(0, 0, 0, 16)
                    .Children(
                        new TextBlock()
                            .Text("Órdenes en Tránsito")
                            .FontSize(24)
                            .FontWeight(FontWeight.Bold)
                            .Foreground(Brushes.White),
                        new TextBlock()
                            .Text(pendingPurchasesText)
                            .FontSize(13)
                            .Foreground(TextMuted)
                            .Margin(0, 4, 0, 0)
                    ),
                new Border().Row(1)
                    .Child(
                        new Grid()
                            .Children(
                                BuildEmptyPendingPurchases().IsVisible(state, x => !x.HasPendingPurchases),
                                BuildPendingPurchasesList(state).IsVisible(state, x => x.HasPendingPurchases)
                            )
                    )
            );
    }

    private Control BuildEmptyPendingPurchases()
    {
        return new StackPanel()
            .VerticalAlignment(VerticalAlignment.Center)
            .HorizontalAlignment(HorizontalAlignment.Center)
            .Spacing(12)
            .Children(
                new TextBlock()
                    .Text("📭")
                    .FontSize(48)
                    .HorizontalAlignment(HorizontalAlignment.Center),
                new TextBlock()
                    .Text("No hay pedidos en camino")
                    .FontSize(16)
                    .FontWeight(FontWeight.Bold)
                    .Foreground(Brushes.White)
                    .HorizontalAlignment(HorizontalAlignment.Center),
                new TextBlock()
                    .Text("Los pedidos que generes aparecerán aquí para su confirmación de recepción.")
                    .FontSize(12)
                    .Foreground(TextMuted)
                    .TextWrapping(TextWrapping.Wrap)
                    .HorizontalAlignment(HorizontalAlignment.Center)
            );
    }

    private Control BuildPendingPurchasesList(PendingOrdersState state)
    {
        return new ScrollViewer()
            .Content(
                new ItemsControl()
                    .ItemsSource(state, x => x.PendingPurchases)
                    .ItemTemplate(new FuncDataTemplate<Purchase>((purchase, _) =>
                        new Border()
                            .Background(BackgroundSecondary)
                            .Padding(15)
                            .Margin(0, 0, 0, 10)
                            .CornerRadius(8)
                            .BorderBrush(BorderColor)
                            .BorderThickness(1)
                            .Child(
                                new Grid().Cols("Auto, *, Auto")
                                    .Children(
                                        new Border()
                                            .Background(SolidColorBrush.Parse("#F59E0B"))
                                            .CornerRadius(6)
                                            .Width(40)
                                            .Height(40)
                                            .VerticalAlignment(VerticalAlignment.Center)
                                            .Col(0)
                                            .Margin(0, 0, 12, 0)
                                            .Child(
                                                new TextBlock()
                                                    .Text("🚛")
                                                    .FontSize(20)
                                                    .HorizontalAlignment(HorizontalAlignment.Center)
                                                    .VerticalAlignment(VerticalAlignment.Center)
                                            ),
                                        new StackPanel()
                                            .Col(1)
                                            .VerticalAlignment(VerticalAlignment.Center)
                                            .Children(
                                                new TextBlock()
                                                    .Text($"📄 Pedido: {purchase.InvoiceNumber}")
                                                    .FontWeight(FontWeight.Bold)
                                                    .Foreground(Brushes.White)
                                                    .FontSize(14),
                                                new TextBlock()
                                                    .Text($"🏢 Proveedor: {purchase.SupplierCode}")
                                                    .Foreground(TextMuted)
                                                    .FontSize(12)
                                                    .Margin(0, 2, 0, 0),
                                                new TextBlock()
                                                    .Text($"📅 Generado: {purchase.Date:dd/MM/yyyy HH:mm}")
                                                    .Foreground(TextMuted)
                                                    .FontSize(11)
                                                    .Margin(0, 2, 0, 0),
                                                new TextBlock()
                                                    .Text(
                                                        $"📦 Total productos: {purchase.Details.Count}  |  💰 Total: ${purchase.TotalCost:N2}")
                                                    .Foreground(TextMuted)
                                                    .FontSize(11)
                                                    .Margin(0, 4, 0, 0)
                                            ),
                                        new Button()
                                            .Content("✅ Confirmar Recepción")
                                            .Background(AccentGreen)
                                            .Foreground(Brushes.White)
                                            .FontWeight(FontWeight.SemiBold)
                                            .Padding(12, 8)
                                            .CornerRadius(6)
                                            .Cursor(new Cursor(StandardCursorType.Hand))
                                            .VerticalAlignment(VerticalAlignment.Center)
                                            .Col(2)
                                            .With(button => button.Click += (_, _) => state.CompletePurchase(purchase))
                                    )
                            )
                    ))
            );
    }
}