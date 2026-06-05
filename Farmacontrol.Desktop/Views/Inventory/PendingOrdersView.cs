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
    private static readonly SolidColorBrush BackgroundInput = SolidColorBrush.Parse("#374151");
    private static readonly SolidColorBrush BackgroundCard = SolidColorBrush.Parse("#1F2937");
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
                        new Style(x => x.OfType<TabItem>().Class(":pointerover"))
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
            .IsVisible(state, x => !x.HasSuppliers);

        var supplierCombo = new ComboBox()
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .Background(BackgroundSecondary)
            .BorderBrush(BorderColor)
            .Foreground(Brushes.White)
            .PlaceholderText("Seleccione Proveedor...")
            .ItemsSource(state, x => x.Suppliers)
            .SelectedItem(state, x => x.SelectedSupplier, BindingMode.TwoWay)
            .IsEnabled(state, x => x.HasSuppliers)
            .ItemTemplate(new FuncDataTemplate<Supplier>((s, _) =>
            {
                var displayText = s?.Name ?? "Proveedor no disponible";

                return new TextBlock()
                    .Text(displayText)
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
            .IsEnabled(state, x => x.HasSuppliers);

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
                            .IsVisible(state, x => x.HasSuppliers)
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
        var emptyStateView = BuildEmptyPendingPurchases()
            .IsVisible(state, x => x.ShowEmptyPendingPurchases);

        var ordersListView = new Grid().Rows("Auto, *")
            .IsVisible(state, x => x.ShowPendingPurchasesList)
            .Children(
                new StackPanel().Row(0).Margin(0, 0, 0, 16)
                    .Children(
                        new TextBlock()
                            .Text("Órdenes en Tránsito")
                            .FontSize(24)
                            .FontWeight(FontWeight.Bold)
                            .Foreground(Brushes.White),
                        new TextBlock()
                            .Text(state, x => x.PendingPurchasesSubtitle)
                            .FontSize(13)
                            .Foreground(TextMuted)
                            .Margin(0, 4, 0, 0)
                    ),
                BuildPendingPurchasesList(state).Row(1)
            );

        var receiveOrderHost = new ContentControl()
            .IsVisible(state, x => x.IsReceivingOrder);

        state.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(state.ReceivePurchaseState))
            {
                receiveOrderHost.Content = state.ReceivePurchaseState != null
                    ? BuildReceiveOrderView(state, state.ReceivePurchaseState)
                    : null;
            }
        };

        return new Panel()
            .Children(
                emptyStateView,
                ordersListView,
                receiveOrderHost
            );
    }

    private Control BuildReceiveOrderView(PendingOrdersState state, ReceivePurchaseState receiveState) =>
        new Grid().Rows("Auto, Auto, Auto, *")
            .Children(
                BuildReceiveHeader(state, receiveState).Row(0),
                BuildPurchaseInfo(receiveState).Row(1),
                BuildReceiveSummary(receiveState).Row(2),
                new Grid().Cols("*, 340").Row(3)
                    .Children(
                        BuildProductsForReceiveList(receiveState).Col(0),
                        new Grid().Rows("*, Auto").Col(1).Margin(16, 0, 0, 0)
                            .Children(
                                new ScrollViewer()
                                    .Row(0)
                                    .Content(BuildReceiveBatchPanel(receiveState)),
                                BuildReceiveActions(state, receiveState).Row(1).Margin(0, 12, 0, 0)
                            )
                    )
            );

    private Control BuildReceiveHeader(PendingOrdersState state, ReceivePurchaseState receiveState)
    {
        var backButton = new Button()
            .Content("← Volver a lista de pedidos")
            .Background(Brushes.Transparent)
            .Foreground(AccentBlue)
            .FontSize(13)
            .Cursor(new Cursor(StandardCursorType.Hand));
        backButton.Click += (_, _) => state.BackToOrdersListCommand.Execute(null);

        return new Grid().Cols("Auto, *")
            .Margin(0, 0, 0, 16)
            .Children(
                backButton.Col(0),
                new StackPanel().Col(1).HorizontalAlignment(HorizontalAlignment.Center)
                    .Children(
                        new TextBlock()
                            .Text($"Recepción de Pedido: {receiveState.PurchaseInfo}")
                            .FontSize(20)
                            .FontWeight(FontWeight.Bold)
                            .Foreground(Brushes.White)
                            .HorizontalAlignment(HorizontalAlignment.Center),
                        new TextBlock()
                            .Text(receiveState, x => x.ProgressText)
                            .FontSize(13)
                            .Foreground(TextMuted)
                            .HorizontalAlignment(HorizontalAlignment.Center)
                            .Margin(0, 4, 0, 0)
                    )
            );
    }

    private Control BuildReceiveActions(PendingOrdersState pendingState, ReceivePurchaseState receiveState)
    {
        var cancelButton = new Button()
            .Content("Cancelar")
            .Background(Brushes.Transparent)
            .Foreground(TextMuted)
            .Padding(20, 10)
            .CornerRadius(6)
            .Cursor(new Cursor(StandardCursorType.Hand));
        cancelButton.Click += (_, _) => pendingState.BackToOrdersListCommand.Execute(null);

        var confirmButton = new Button()
            .Content("✅ Confirmar Recepción")
            .Background(AccentGreen)
            .Foreground(Brushes.White)
            .FontWeight(FontWeight.SemiBold)
            .Padding(20, 10)
            .CornerRadius(6)
            .Cursor(new Cursor(StandardCursorType.Hand))
            .IsEnabled(receiveState.CanComplete);

        receiveState.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(receiveState.CanComplete))
            {
                confirmButton.IsEnabled = receiveState.CanComplete;
            }
        };

        confirmButton.Click += (_, _) =>
        {
            receiveState.CompleteReception(() =>
            {
                pendingState.LoadDashboardData();
                pendingState.LoadPendingPurchases();
                pendingState.BackToOrdersListCommand.Execute(null);
            });
        };

        return new StackPanel()
            .Orientation(Orientation.Horizontal)
            .Spacing(12)
            .HorizontalAlignment(HorizontalAlignment.Right)
            .Children(cancelButton, confirmButton);
    }

    private Control BuildPurchaseInfo(ReceivePurchaseState receiveState) =>
        new Border()
            .Background(BackgroundSecondary)
            .CornerRadius(10)
            .Padding(16)
            .Margin(0, 0, 0, 16)
            .Child(
                new StackPanel()
                    .Children(
                        new TextBlock()
                            .Text($"Proveedor: {receiveState.SupplierName}")
                            .FontSize(14)
                            .Foreground(Brushes.White),
                        new TextBlock()
                            .Text($"Fecha de generación: {receiveState.PurchaseDate:dd/MM/yyyy HH:mm}")
                            .FontSize(12)
                            .Foreground(TextMuted)
                            .Margin(0, 4, 0, 0)
                    )
            );

    private Control BuildReceiveSummary(ReceivePurchaseState receiveState) =>
        new Border()
            .Background(BackgroundSecondary)
            .CornerRadius(10)
            .Padding(16)
            .Margin(0, 0, 0, 20)
            .Child(
                new Grid().Cols("*, *, *, *")
                    .Children(
                        BuildSummaryCard("📦", "Productos", receiveState.TotalProductsCount.ToString(), 0),
                        BuildSummaryCard("✅", "Completados", receiveState.FullyReceivedCount.ToString(), 1),
                        BuildSummaryCard("⏳", "Pendientes", receiveState.PendingCount.ToString(), 2),
                        BuildSummaryCard("📊", "Progreso", receiveState.ProgressText, 3)
                    )
            );

    private Control BuildSummaryCard(string icon, string label, string value, int column) =>
        new StackPanel()
            .HorizontalAlignment(HorizontalAlignment.Center)
            .Col(column)
            .Children(
                new TextBlock().Text(icon).FontSize(24).HorizontalAlignment(HorizontalAlignment.Center),
                new TextBlock().Text(label).FontSize(11).Foreground(TextMuted)
                    .HorizontalAlignment(HorizontalAlignment.Center),
                new TextBlock().Text(value).FontSize(16).FontWeight(FontWeight.Bold).Foreground(AccentGreen)
                    .HorizontalAlignment(HorizontalAlignment.Center).Margin(0, 4, 0, 0)
            );

    private Control BuildProductsForReceiveList(ReceivePurchaseState receiveState) =>
        new ScrollViewer()
            .Content(
                new ItemsControl()
                    .ItemsSource(receiveState.ProductItems)
                    .ItemTemplate(new FuncDataTemplate<PurchaseProductState>((item, _) =>
                        BuildReceiveProductItem(item, receiveState)))
            );

    private Control BuildReceiveProductItem(PurchaseProductState item, ReceivePurchaseState receiveState)
    {
        var statusIcon = new TextBlock()
            .FontSize(20)
            .VerticalAlignment(VerticalAlignment.Center)
            .Col(0)
            .Margin(0, 0, 16, 0);

        var progressBar = new ProgressBar()
            .Width(80).Height(6).Col(2)
            .Margin(0, 0, 16, 0)
            .VerticalAlignment(VerticalAlignment.Center);

        var progressBadge = new Border()
            .CornerRadius(4).Padding(8, 4).Col(3).Margin(0, 0, 16, 0);
        var progressBadgeText = new TextBlock()
            .FontSize(11).FontWeight(FontWeight.Bold).Foreground(Brushes.White);
        progressBadge.Child = progressBadgeText;

        var selectBtn = new Button()
            .Foreground(Brushes.White)
            .Padding(12, 6).CornerRadius(6).FontSize(12)
            .Cursor(new Cursor(StandardCursorType.Hand))
            .Col(4);
        selectBtn.Click += (_, _) => receiveState.SelectProductCommand.Execute(item);

        void UpdateHeader()
        {
            var complete = item.IsComplete;
            var pct = item.TotalQuantity > 0
                ? (double)item.ReceivedQuantity / item.TotalQuantity * 100
                : 0;

            statusIcon.Text = complete ? "✅" : "📦";
            progressBar.Value = pct;
            progressBadge.Background = complete ? AccentGreen : WarningYellow;
            progressBadgeText.Text = $"{item.ReceivedQuantity}/{item.TotalQuantity}";
            selectBtn.Content = complete ? "✅ Ver" : "Seleccionar";
            selectBtn.Background = complete ? AccentGreen
                : receiveState.SelectedProduct == item ? AccentGreen : AccentBlue;
        }

        UpdateHeader();

        item.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(item.IsComplete)
                or nameof(item.ReceivedQuantity)
                or nameof(item.PendingQuantity))
                UpdateHeader();
        };

        receiveState.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(receiveState.SelectedProduct))
                UpdateHeader();
        };

        var expander = new Expander()
            .IsExpanded(true)
            .Header(
                new Grid().Cols("Auto, *, Auto, Auto, 100")
                    .Margin(0, 8)
                    .Children(
                        statusIcon,
                        new StackPanel().Col(1)
                            .Children(
                                new TextBlock()
                                    .Text(item.ProductName)
                                    .FontSize(14)
                                    .FontWeight(FontWeight.SemiBold)
                                    .Foreground(Brushes.White),
                                new TextBlock()
                                    .Text($"Código: {item.ProductCode}")
                                    .FontSize(11)
                                    .Foreground(TextMuted)
                            ),
                        progressBar,
                        progressBadge,
                        selectBtn
                    )
            )
            .Content(BuildReceivedBatchesList(item, receiveState));

        return new Border()
            .Background(BackgroundSecondary)
            .CornerRadius(10)
            .Margin(0, 0, 0, 12)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .Styles(
                new Style(x => x.OfType<Expander>())
                {
                    Setters =
                    {
                        new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent),
                        new Setter(TemplatedControl.BorderBrushProperty, Brushes.Transparent),
                        new Setter(HorizontalAlignmentProperty, HorizontalAlignment.Stretch)
                    }
                },
                new Style(x => x.OfType<Expander>().Template().OfType<ToggleButton>())
                {
                    Setters = { new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent) }
                },
                new Style(x =>
                    x.OfType<Expander>().Template().OfType<ToggleButton>().Class(":pointerover").Template()
                        .OfType<Border>())
                {
                    Setters = { new Setter(Border.BackgroundProperty, Brushes.Transparent) }
                },
                new Style(x =>
                    x.OfType<Expander>().Template().OfType<ToggleButton>().Class(":checked").Template()
                        .OfType<Border>())
                {
                    Setters = { new Setter(Border.BackgroundProperty, Brushes.Transparent) }
                }
            )
            .Child(expander);
    }

    private Control BuildReceivedBatchesList(PurchaseProductState item, ReceivePurchaseState receiveState)
    {
        var container = new StackPanel().Spacing(8).Margin(16, 12, 16, 8);

        void Rebuild()
        {
            container.Children.Clear();
            if (item.ReceivedBatches.Count == 0)
            {
                container.Children.Add(
                    new TextBlock()
                        .Text("📭 No hay lotes recibidos aún")
                        .FontSize(12)
                        .Foreground(TextMuted)
                        .HorizontalAlignment(HorizontalAlignment.Center)
                        .Margin(0, 8)
                );
            }
            else
            {
                foreach (var batch in item.ReceivedBatches)
                    container.Children.Add(BuildReceivedBatchItem(item, batch, receiveState));
            }
        }

        Rebuild();
        item.ReceivedBatches.CollectionChanged += (_, _) => Rebuild();

        return container;
    }

    private Control BuildReceivedBatchItem(PurchaseProductState item, ReceivedBatchState batch,
        ReceivePurchaseState receiveState)
    {
        Control statusBadge;

        if (batch.IsManualStock)
        {
            statusBadge = new Border()
                .Background(SolidColorBrush.Parse("#1D4ED8"))
                .CornerRadius(4).Padding(6, 3).Col(2).Margin(0, 0, 12, 0)
                .Child(new TextBlock().Text("MANUAL").FontSize(10).FontWeight(FontWeight.Bold)
                    .Foreground(Brushes.White));
        }
        else
        {
            var daysUntilExpiry = (batch.ExpirationDate - DateTime.Today).Days;
            var isExpired = daysUntilExpiry < 0;
            var isExpiringSoon = daysUntilExpiry is <= 30 and >= 0;
            var statusColor = isExpired ? SolidColorBrush.Parse("#EF4444") :
                isExpiringSoon ? WarningYellow : AccentGreen;
            var statusText = isExpired ? "VENCIDO" :
                isExpiringSoon ? $"Vence en {daysUntilExpiry}d" : "Válido";
            statusBadge = new Border()
                .Background(statusColor)
                .CornerRadius(4).Padding(6, 3).Col(2).Margin(0, 0, 12, 0)
                .Child(new TextBlock().Text(statusText).FontSize(10).FontWeight(FontWeight.Bold)
                    .Foreground(isExpiringSoon ? SolidColorBrush.Parse("#000000") : Brushes.White));
        }

        var removeButton = new Button()
            .Content("🗑️")
            .Background(Brushes.Transparent)
            .Foreground(SolidColorBrush.Parse("#EF4444"))
            .Padding(4)
            .CornerRadius(4)
            .Cursor(new Cursor(StandardCursorType.Hand));
        removeButton.Click += (_, _) => receiveState.RemoveBatch(item, batch);

        var batchLabel = batch.IsManualStock
            ? "Stock Manual"
            : $"Lote: {batch.LotCode}";

        return new Border()
            .Background(BackgroundCard)
            .CornerRadius(8)
            .Padding(12)
            .Margin(0, 0, 0, 8)
            .Child(
                new Grid().Cols("Auto, *, Auto, Auto")
                    .Children(
                        new TextBlock().Text(batch.IsManualStock ? "📊" : "📋").FontSize(14).Col(0).Margin(0, 0, 12, 0),
                        new StackPanel().Col(1)
                            .Children(
                                new TextBlock()
                                    .Text(batchLabel)
                                    .FontSize(12)
                                    .FontWeight(FontWeight.SemiBold)
                                    .Foreground(Brushes.White),
                                new TextBlock()
                                    .Text($"Cantidad: {batch.Quantity} unidades")
                                    .FontSize(11)
                                    .Foreground(TextMuted)
                            ),
                        statusBadge,
                        removeButton.Col(3)
                    )
            );
    }

    private Control BuildReceiveBatchPanel(ReceivePurchaseState receiveState)
    {
        var quantityBox = new TextBox()
            .PlaceholderText("Cantidad a recibir")
            .Background(BackgroundInput).Foreground(Brushes.White)
            .BorderBrush(BorderColor).CornerRadius(6).Padding(10, 8)
            .Text(receiveState, x => x.Quantity, BindingMode.TwoWay);

        var unitCostBox = new TextBox()
            .PlaceholderText("Opcional")
            .Background(BackgroundInput).Foreground(Brushes.White)
            .BorderBrush(BorderColor).CornerRadius(6).Padding(10, 8)
            .Text(receiveState, x => x.UnitCost, BindingMode.TwoWay);

        var lotCodeBox = new TextBox()
            .PlaceholderText("Ej: LOT-001")
            .Background(BackgroundInput).Foreground(Brushes.White)
            .BorderBrush(BorderColor).CornerRadius(6).Padding(10, 8)
            .Text(receiveState, x => x.LotCode, BindingMode.TwoWay);

        var expirationPicker = new DatePicker()
            .Background(BackgroundInput).Foreground(Brushes.White)
            .BorderBrush(BorderColor).CornerRadius(6).Padding(10, 8)
            .HorizontalAlignment(HorizontalAlignment.Stretch);
        expirationPicker.Bind(DatePicker.SelectedDateProperty,
            new Binding(nameof(receiveState.ExpirationDate)) { Source = receiveState, Mode = BindingMode.TwoWay });

        var manufacturingPicker = new DatePicker()
            .Background(BackgroundInput).Foreground(Brushes.White)
            .BorderBrush(BorderColor).CornerRadius(6).Padding(10, 8)
            .HorizontalAlignment(HorizontalAlignment.Stretch);
        manufacturingPicker.Bind(DatePicker.SelectedDateProperty,
            new Binding(nameof(receiveState.ManufacturingDate)) { Source = receiveState, Mode = BindingMode.TwoWay });

        var lotFields = new StackPanel().Spacing(0)
            .IsVisible(receiveState, x => x.IsLotMode)
            .Children(
                CreateReceiveField("Número de Lote *", lotCodeBox),
                CreateReceiveDatePickerField("Fecha de Expiración *", expirationPicker),
                CreateReceiveDatePickerField("Fecha de Fabricación", manufacturingPicker)
            );

        var quantityWarningPanel = new Border()
            .Background(SolidColorBrush.Parse("#78350F"))
            .BorderBrush(SolidColorBrush.Parse("#F59E0B")).BorderThickness(1)
            .CornerRadius(6).Padding(10, 8).Margin(0, 0, 0, 12)
            .IsVisible(receiveState, x => x.HasQuantityWarning)
            .Child(new TextBlock().Text(receiveState, x => x.QuantityWarning)
                .Foreground(SolidColorBrush.Parse("#FDE68A")).FontSize(11).TextWrapping(TextWrapping.Wrap));

        var errorPanel = new Border()
            .Background(SolidColorBrush.Parse("#7F1D1D"))
            .BorderBrush(SolidColorBrush.Parse("#DC2626")).BorderThickness(1)
            .CornerRadius(6).Padding(10, 8).Margin(0, 0, 0, 12)
            .IsVisible(receiveState, x => x.HasError)
            .Child(new TextBlock().Text(receiveState, x => x.ErrorMessage)
                .Foreground(SolidColorBrush.Parse("#FCA5A5")).FontSize(11).TextWrapping(TextWrapping.Wrap));

        var productNameLabel = new TextBlock()
            .FontSize(15).FontWeight(FontWeight.Bold).Foreground(Brushes.White)
            .Margin(0, 0, 0, 12);

        var modeBadge = new Border()
            .CornerRadius(6).Padding(10, 6).Margin(0, 0, 0, 14);

        void UpdateModeBadge()
        {
            if (receiveState.IsManualStockMode)
            {
                modeBadge.Background = SolidColorBrush.Parse("#1D4ED8");
                ((TextBlock)modeBadge.Child!).Text = "📊 Modo: Stock Manual (sin lote)";
            }
            else
            {
                modeBadge.Background = SolidColorBrush.Parse("#065F46");
                ((TextBlock)modeBadge.Child!).Text = "🏷️ Modo: Por Lote";
            }
        }

        modeBadge.Child = new TextBlock().FontSize(11).FontWeight(FontWeight.SemiBold).Foreground(Brushes.White);
        UpdateModeBadge();

        var toggleModeBtn = new Button()
            .Content(receiveState, x => x.IsManualModeTitle)
            .Background(BackgroundTertiary)
            .Foreground(TextMuted)
            .FontSize(11)
            .Padding(10, 6)
            .CornerRadius(6)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .HorizontalContentAlignment(HorizontalAlignment.Center)
            .Margin(0, 0, 0, 16)
            .Cursor(new Cursor(StandardCursorType.Hand));
        toggleModeBtn.Click += (_, _) =>
        {
            receiveState.ToggleReceiveModeCommand.Execute(null);
            UpdateModeBadge();
        };

        var noSelectionPanel = new Border()
            .Background(BackgroundSecondary).CornerRadius(10).Padding(16)
            .Child(
                new StackPanel().VerticalAlignment(VerticalAlignment.Center)
                    .HorizontalAlignment(HorizontalAlignment.Center).Spacing(12)
                    .Children(
                        new TextBlock().Text("📦").FontSize(48).HorizontalAlignment(HorizontalAlignment.Center),
                        new TextBlock().Text("Seleccione un producto").FontSize(16).FontWeight(FontWeight.Bold)
                            .Foreground(Brushes.White).HorizontalAlignment(HorizontalAlignment.Center),
                        new TextBlock().Text("Haga clic en 'Seleccionar' en el producto que desea recibir")
                            .FontSize(12).Foreground(TextMuted).TextWrapping(TextWrapping.Wrap)
                            .HorizontalAlignment(HorizontalAlignment.Center)
                    )
            );

        var receivePanel = new Border()
            .Background(BackgroundSecondary).CornerRadius(10).Padding(16)
            .Child(
                new StackPanel().Spacing(0)
                    .Children(
                        productNameLabel,
                        modeBadge,
                        toggleModeBtn,
                        errorPanel,
                        quantityWarningPanel,
                        CreateReceiveField("Cantidad *", quantityBox),
                        lotFields,
                        CreateReceiveField("Costo Unitario", unitCostBox),
                        new Button().Content("➕ Agregar")
                            .Background(AccentBlue).Foreground(Brushes.White)
                            .FontWeight(FontWeight.SemiBold).Padding(16, 10).CornerRadius(6)
                            .Cursor(new Cursor(StandardCursorType.Hand))
                            .HorizontalAlignment(HorizontalAlignment.Stretch)
                            .HorizontalContentAlignment(HorizontalAlignment.Center)
                            .Margin(0, 16, 0, 0)
                            .With(btn => btn.Click += (_, _) => receiveState.AddBatchToSelectedCommand.Execute(null))
                    )
            );

        void UpdatePanel()
        {
            var selected = receiveState.SelectedProduct;
            var showForm = selected is { IsComplete: false };
            noSelectionPanel.IsVisible = !showForm;
            receivePanel.IsVisible = showForm;
            productNameLabel.Text = selected != null ? $"Recibir: {selected.ProductName}" : string.Empty;
        }

        UpdatePanel();
        receiveState.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(receiveState.SelectedProduct))
                UpdatePanel();
        };

        return new Panel().Children(noSelectionPanel, receivePanel);
    }

    private Control CreateReceiveDatePickerField(string label, DatePicker datePicker)
    {
        datePicker.Background(BackgroundInput)
            .Foreground(Brushes.White)
            .BorderBrush(BorderColor)
            .CornerRadius(6)
            .Padding(10, 8)
            .HorizontalAlignment(HorizontalAlignment.Stretch);

        return new StackPanel().Spacing(6).Margin(0, 0, 0, 12)
            .Children(
                new TextBlock()
                    .Text(label)
                    .FontSize(11)
                    .Foreground(TextMuted),
                datePicker
            );
    }

    private Control CreateReceiveField(string label, TextBox textBox)
    {
        return new StackPanel().Spacing(6).Margin(0, 0, 0, 12)
            .Children(
                new TextBlock()
                    .Text(label)
                    .FontSize(11)
                    .Foreground(TextMuted),
                textBox
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
                                            .Content("📦 Recibir Pedido")
                                            .Background(AccentGreen)
                                            .Foreground(Brushes.White)
                                            .FontWeight(FontWeight.SemiBold)
                                            .Padding(12, 8)
                                            .CornerRadius(6)
                                            .Cursor(new Cursor(StandardCursorType.Hand))
                                            .VerticalAlignment(VerticalAlignment.Center)
                                            .Col(2)
                                            .With(button => button.Click += (_, _) =>
                                            {
                                                state.StartReceivingOrderCommand.Execute(purchase);
                                            })
                                    )
                            )
                    ))
            );
    }
}