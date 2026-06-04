using Avalonia.Controls.Presenters;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Styling;
using Farmacontrol.Desktop.States;
using Farmacontrol.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Farmacontrol.Desktop.Views.Sales;

public class SaleView() : ViewBase<SaleState>(Program.ServiceProvider.GetRequiredService<SaleState>())
{
    private static readonly SolidColorBrush BackgroundPrimary = SolidColorBrush.Parse("#111827");
    private static readonly SolidColorBrush BackgroundSecondary = SolidColorBrush.Parse("#1F2937");
    private static readonly SolidColorBrush BackgroundTertiary = SolidColorBrush.Parse("#374151");
    private static readonly SolidColorBrush BackgroundHover = SolidColorBrush.Parse("#4B5563");
    private static readonly SolidColorBrush BackgroundInput = SolidColorBrush.Parse("#374151");
    private static readonly SolidColorBrush TextMuted = SolidColorBrush.Parse("#9CA3AF");
    private static readonly SolidColorBrush AccentBlue = SolidColorBrush.Parse("#2563EB");
    private static readonly SolidColorBrush AccentGreen = SolidColorBrush.Parse("#10B981");
    private static readonly SolidColorBrush DangerRed = SolidColorBrush.Parse("#EF4444");
    private static readonly SolidColorBrush WarningYellow = SolidColorBrush.Parse("#F59E0B");
    private static readonly SolidColorBrush BorderColor = SolidColorBrush.Parse("#374151");

    private Product? _draggedProduct;

    protected override object Build(SaleState state)
    {
        state.LoadCatalog();
        return new Grid()
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
                                new Setter(Border.BackgroundProperty, BackgroundTertiary),
                                new Setter(Border.BorderBrushProperty, BorderColor)
                            }
                        },
                        new Style(x => x.OfType<ComboBoxItem>().Template().OfType<ContentPresenter>())
                            { Setters = { new Setter(ContentPresenter.BackgroundProperty, BackgroundSecondary) } },
                        new Style(x =>
                                x.OfType<ComboBoxItem>().Class(":pointerover").Template().OfType<ContentPresenter>())
                            { Setters = { new Setter(ContentPresenter.BackgroundProperty, BackgroundTertiary) } },
                        new Style(x =>
                                x.OfType<ComboBoxItem>().Class(":selected").Template().OfType<ContentPresenter>())
                            { Setters = { new Setter(ContentPresenter.BackgroundProperty, AccentBlue) } }
                    )
                    .Child(
                        new Grid().Rows("Auto, *")
                            .Children(
                                BuildHeader().Row(0),
                                new Grid().Cols("4*, 3.5*, 2.5*").Row(1).ColumnSpacing(16)
                                    .Children(
                                        BuildCatalogPanel(state).Col(0),
                                        BuildCartPanel(state).Col(1),
                                        BuildRightPanel(state).Col(2)
                                    )
                            )
                    )
            );
    }

    private Control BuildHeader() =>
        new Grid().Cols("*, Auto").Margin(0, 0, 0, 20)
            .Children(
                new StackPanel()
                    .Children(
                        new TextBlock().Text("Punto de Venta").FontSize(24).FontWeight(FontWeight.Bold)
                            .Foreground(Brushes.White),
                        new TextBlock()
                            .Text("Escanee o busque productos para agregar al carrito")
                            .FontSize(13).Foreground(TextMuted)
                    ),
                new Border()
                    .Background(BackgroundSecondary).CornerRadius(8).Padding(12, 8)
                    .Col(1).VerticalAlignment(VerticalAlignment.Center)
                    .Child(
                        new StackPanel().Orientation(Orientation.Horizontal).Spacing(8)
                            .Children(
                                new TextBlock().Text("🗓️").FontSize(14),
                                new TextBlock()
                                    .Text(DateTime.Now.ToString("dd/MM/yyyy"))
                                    .FontSize(13).Foreground(TextMuted)
                                    .VerticalAlignment(VerticalAlignment.Center)
                            )
                    )
            );

    private Control BuildCatalogPanel(SaleState state)
    {
        var searchBox = new TextBox()
            .PlaceholderText("🔍 Buscar producto en catálogo...")
            .Background(BackgroundInput).Foreground(Brushes.White)
            .BorderBrush(BorderColor).CornerRadius(8).Padding(14, 12)
            .Text(state, x => x.CatalogSearchQuery, BindingMode.TwoWay);

        var catalogGrid = new ItemsControl()
            .ItemsSource(state, x => x.FilteredCatalogProducts)
            .With(c => c.ItemsPanel =
                new FuncTemplate<Panel?>(() => new WrapPanel { Orientation = Orientation.Horizontal }))
            .ItemTemplate(new FuncDataTemplate<Product>((product, _) => BuildProductCard(product, state)));

        return new Grid().Rows("Auto, *")
            .Children(
                new StackPanel().Spacing(8).Row(0).Margin(0, 0, 0, 12).Children(
                    searchBox
                ),
                new ScrollViewer().Row(1).Content(catalogGrid)
            );
    }

    private Control BuildProductCard(Product product, SaleState state)
    {
        var card = new Border()
            .Background(BackgroundSecondary)
            .CornerRadius(10)
            .BorderBrush(BorderColor)
            .BorderThickness(1)
            .Margin(0, 0, 10, 10)
            .Width(170)
            .Padding(12)
            .Cursor(new Cursor(StandardCursorType.Hand));

        card.Tapped += (_, e) =>
        {
            e.Handled = true;
            state.AddToCart(product);
        };

        var isLowStock = product.Stock <= product.MinimumStock;
        var stockColor = isLowStock ? WarningYellow : AccentGreen;

        var expDateText = "N/A";
        var hasExp = false;
        if (product.Batches.Any())
        {
            var earliest = product.Batches.OrderBy(b => b.ExpirationDate).FirstOrDefault(b => b.Quantity > 0);
            if (earliest != null)
            {
                expDateText = earliest.ExpirationDate.ToString("MM/yy");
                hasExp = true;
            }
        }

        var expanderBtn = new Button()
            .Content("ℹ️")
            .Background(Brushes.Transparent).Foreground(TextMuted)
            .Padding(4).CornerRadius(4)
            .HorizontalAlignment(HorizontalAlignment.Right)
            .Cursor(new Cursor(StandardCursorType.Hand));

        var flyout = new Flyout
        {
            ShowMode = FlyoutShowMode.Standard, Placement = PlacementMode.RightEdgeAlignedTop,
            Content = new Border().Background(BackgroundTertiary).CornerRadius(8).Padding(12).Width(220)
                .Child(new StackPanel().Spacing(6).Children(
                    new TextBlock().Text("Detalles:").FontWeight(FontWeight.Bold),
                    new TextBlock().Text($"Categorías: {(string.Join(", ", product.Tags))}").FontSize(11)
                        .TextWrapping(TextWrapping.Wrap),
                    new TextBlock().Text($"Ingredientes: {(string.Join(", ", product.Ingredients))}").FontSize(11)
                        .TextWrapping(TextWrapping.Wrap),
                    new TextBlock().Text(product.GetDescription()).FontSize(11).TextWrapping(TextWrapping.Wrap)
                ))
        };

        expanderBtn.Flyout(flyout);

        expanderBtn.Tapped += (_, e) => e.Handled = true;

        var content = new StackPanel().Spacing(4).Children(
            new Grid().Cols("*, Auto").Children(
                new TextBlock().Text(product.Code).FontSize(10).Foreground(TextMuted)
                    .VerticalAlignment(VerticalAlignment.Center).Col(0),
                expanderBtn.Col(1)
            ),
            new TextBlock().Text(product.Name).FontSize(13).FontWeight(FontWeight.SemiBold).Foreground(Brushes.White)
                .TextWrapping(TextWrapping.Wrap).MaxHeight(36),
            new TextBlock().Text($"Q{product.Price:F2}").FontSize(14).FontWeight(FontWeight.Bold)
                .Foreground(AccentGreen).Margin(0, 4, 0, 0),
            new Grid().Cols("*, Auto").Margin(0, 4, 0, 0).Children(
                new TextBlock().Text($"Stock: {product.Stock}").FontSize(11).Foreground(stockColor)
                    .FontWeight(FontWeight.SemiBold).Col(0).VerticalAlignment(VerticalAlignment.Center),
                new TextBlock().Text(hasExp ? $"Vence: {expDateText}" : "").FontSize(10).Foreground(TextMuted).Col(1)
                    .VerticalAlignment(VerticalAlignment.Center)
            )
        );

        card.Child = content;

        Point dragStartPoint = new Point();
        bool isDragging = false;
        PointerPressedEventArgs? dragStartEvent = null;
        card.PointerPressed += (_, e) =>
        {
            dragStartPoint = e.GetPosition(card);
            isDragging = false;
            dragStartEvent = e;
        };
        card.PointerMoved += async (_, e) =>
        {
            if (!e.GetCurrentPoint(card).Properties.IsLeftButtonPressed || dragStartEvent == null) return;
            
            var pos = e.GetPosition(card);
            
            if (isDragging ||
                (!(Math.Abs(pos.X - dragStartPoint.X) > 3) && !(Math.Abs(pos.Y - dragStartPoint.Y) > 3))) return;
            
            isDragging = true;
            _draggedProduct = product;
            var data = new DataTransfer();
            await DragDrop.DoDragDropAsync(dragStartEvent, data, DragDropEffects.Copy);
            _draggedProduct = null;
            dragStartEvent = null;
        };

        card.PointerEntered += (_, _) => card.Background = BackgroundTertiary;
        card.PointerExited += (_, _) => card.Background = BackgroundSecondary;

        return card;
    }

    private Control BuildCartPanel(SaleState state)
    {
        var panel = new Grid().Rows("Auto, Auto, *")
            .Children(
                BuildSearchBox(state).Row(0),
                BuildFeedbackBanners(state).Row(1),
                BuildCart(state).Row(2)
            );

        DragDrop.SetAllowDrop(panel, true);
        panel.AddHandler(DragDrop.DropEvent, (_, e) =>
        {
            if (_draggedProduct != null)
            {
                state.AddToCart(_draggedProduct);
                e.Handled = true;
            }
        });

        return panel;
    }

    private Control BuildSearchBox(SaleState state)
    {
        var searchBox = new TextBox()
            .PlaceholderText("🔍 Buscar por nombre, código o escanear código de barras...")
            .Background(BackgroundInput).Foreground(Brushes.White)
            .BorderBrush(BorderColor).CornerRadius(8).Padding(14, 12)
            .FontSize(14)
            .Text(state, x => x.SearchQuery, BindingMode.TwoWay);

        var resultsPanel = new Border()
            .Background(BackgroundSecondary)
            .BorderBrush(BorderColor).BorderThickness(1)
            .CornerRadius(8).Margin(0, 4, 0, 0)
            .IsVisible(state, x => x.ShowSearchResults)
            .Child(
                new ScrollViewer().MaxHeight(240)
                    .Content(
                        new ItemsControl()
                            .ItemsSource(state, x => x.SearchResults)
                            .ItemTemplate(new FuncDataTemplate<Product>((product, _) =>
                                BuildSearchResultItem(product, state)))
                    )
            );

        return new StackPanel().Spacing(0).Margin(0, 0, 0, 12)
            .Children(searchBox, resultsPanel);
    }

    private Control BuildSearchResultItem(Product product, SaleState state)
    {
        var row = new Border()
            .Padding(12, 10)
            .Cursor(new Cursor(StandardCursorType.Hand));

        var isLowStock = product.Stock <= product.MinimumStock;
        var stockColor = isLowStock ? WarningYellow : AccentGreen;

        row.Child = new Grid().Cols("*, Auto")
            .Children(
                new StackPanel().Col(0)
                    .Children(
                        new TextBlock().Text(product.Name).FontSize(13).FontWeight(FontWeight.SemiBold)
                            .Foreground(Brushes.White),
                        new TextBlock().Text($"{product.Code}  ·  Q{product.Price:F2}")
                            .FontSize(11).Foreground(TextMuted)
                    ),
                new Border()
                    .Background(stockColor).CornerRadius(4).Padding(6, 2)
                    .Col(1).VerticalAlignment(VerticalAlignment.Center)
                    .Child(
                        new TextBlock().Text($"Stock: {product.Stock}")
                            .FontSize(10).FontWeight(FontWeight.Bold).Foreground(Brushes.White)
                    )
            );

        row.PointerPressed += (_, _) => state.SelectSearchResultCommand.Execute(product);

        row.PointerEntered += (_, _) => row.Background = BackgroundTertiary;
        row.PointerExited += (_, _) => row.Background = Brushes.Transparent;

        return row;
    }

    private Control BuildFeedbackBanners(SaleState state)
    {
        var errorBanner = new Border()
            .Background(SolidColorBrush.Parse("#7F1D1D"))
            .BorderBrush(SolidColorBrush.Parse("#DC2626")).BorderThickness(1)
            .CornerRadius(6).Padding(12, 8).Margin(0, 0, 0, 8)
            .IsVisible(state, x => x.HasError)
            .Child(new TextBlock().Text(state, x => x.ErrorMessage)
                .Foreground(SolidColorBrush.Parse("#FCA5A5")).FontSize(12).TextWrapping(TextWrapping.Wrap));

        var successBanner = new Border()
            .Background(SolidColorBrush.Parse("#064E3B"))
            .BorderBrush(AccentGreen).BorderThickness(1)
            .CornerRadius(6).Padding(12, 8).Margin(0, 0, 0, 8)
            .IsVisible(state, x => x.HasSuccess)
            .Child(new TextBlock().Text(state, x => x.SuccessMessage)
                .Foreground(SolidColorBrush.Parse("#6EE7B7")).FontSize(12).TextWrapping(TextWrapping.Wrap));

        return new StackPanel().Children(errorBanner, successBanner);
    }

    private Control BuildCart(SaleState state)
    {
        var emptyCart = new Border()
            .Background(BackgroundSecondary).CornerRadius(10)
            .IsVisible(state, x => x.CartIsEmpty)
            .Child(
                new StackPanel()
                    .VerticalAlignment(VerticalAlignment.Center)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .Spacing(10).Margin(0, 40)
                    .Children(
                        new TextBlock().Text("🛍️").FontSize(48)
                            .HorizontalAlignment(HorizontalAlignment.Center),
                        new TextBlock().Text("El carrito está vacío")
                            .FontSize(15).FontWeight(FontWeight.Bold).Foreground(Brushes.White)
                            .HorizontalAlignment(HorizontalAlignment.Center),
                        new TextBlock().Text("Busque un producto o escanee su código de barras")
                            .FontSize(12).Foreground(TextMuted).TextWrapping(TextWrapping.Wrap)
                            .HorizontalAlignment(HorizontalAlignment.Center)
                    )
            );

        var cartList = new Border()
            .Background(BackgroundSecondary).CornerRadius(10)
            .IsVisible(state, x => !x.CartIsEmpty)
            .Child(
                new Grid().Rows("Auto, *")
                    .Children(
                        new Border()
                            .Background(BackgroundTertiary).CornerRadius(new CornerRadius(10, 10, 0, 0))
                            .Padding(16, 10)
                            .Row(0)
                            .Child(
                                new Grid().Cols("*, 70, 90, 50")
                                    .Children(
                                        new TextBlock().Text("Producto").FontSize(11).FontWeight(FontWeight.Bold)
                                            .Foreground(TextMuted).Col(0),
                                        new TextBlock().Text("Precio").FontSize(11).FontWeight(FontWeight.Bold)
                                            .Foreground(TextMuted).Col(1)
                                            .HorizontalAlignment(HorizontalAlignment.Center),
                                        new TextBlock().Text("Cantidad").FontSize(11).FontWeight(FontWeight.Bold)
                                            .Foreground(TextMuted).Col(2)
                                            .HorizontalAlignment(HorizontalAlignment.Center),
                                        new TextBlock().Text("Sub").FontSize(11).FontWeight(FontWeight.Bold)
                                            .Foreground(TextMuted).Col(3)
                                            .HorizontalAlignment(HorizontalAlignment.Right)
                                    )
                            ),
                        new ScrollViewer().Row(1)
                            .Content(
                                new ItemsControl()
                                    .ItemsSource(state, x => x.CartItems)
                                    .ItemTemplate(new FuncDataTemplate<CartItemState>((item, _) =>
                                        BuildCartItem(item, state)))
                            )
                    )
            );

        return new Panel().Children(emptyCart, cartList);
    }

    private Control BuildCartItem(CartItemState item, SaleState state)
    {
        var qtyLabel = new TextBlock()
            .FontSize(13).FontWeight(FontWeight.Bold).Foreground(Brushes.White)
            .VerticalAlignment(VerticalAlignment.Center)
            .HorizontalAlignment(HorizontalAlignment.Center);
        qtyLabel.Text = item.Quantity.ToString();

        var subtotalLabel = new TextBlock()
            .FontSize(12).Foreground(Brushes.White)
            .VerticalAlignment(VerticalAlignment.Center)
            .HorizontalAlignment(HorizontalAlignment.Right);
        subtotalLabel.Text = $"Q{item.Subtotal:F2}";

        item.PropertyChanged += (_, _) =>
        {
            qtyLabel.Text = item.Quantity.ToString();
            subtotalLabel.Text = $"Q{item.Subtotal:F2}";
        };

        var decrementBtn = new Button()
            .Content("−").Background(BackgroundTertiary).Foreground(Brushes.White)
            .Width(26).Height(26).Padding(0).CornerRadius(4)
            .HorizontalContentAlignment(HorizontalAlignment.Center)
            .Cursor(new Cursor(StandardCursorType.Hand));
        decrementBtn.Click += (_, _) => state.DecrementItemCommand.Execute(item);

        var incrementBtn = new Button()
            .Content("+").Background(AccentBlue).Foreground(Brushes.White)
            .Width(26).Height(26).Padding(0).CornerRadius(4)
            .HorizontalContentAlignment(HorizontalAlignment.Center)
            .Cursor(new Cursor(StandardCursorType.Hand));
        incrementBtn.Click += (_, _) => state.IncrementItemCommand.Execute(item);

        var removeBtn = new Button()
            .Content("🗑️").Background(Brushes.Transparent).Foreground(DangerRed)
            .Width(28).Height(28).Padding(2).CornerRadius(4)
            .Cursor(new Cursor(StandardCursorType.Hand));
        removeBtn.Click += (_, _) => state.RemoveFromCartCommand.Execute(item);

        return new Border()
            .BorderBrush(BorderColor).BorderThickness(new Thickness(0, 0, 0, 1))
            .Padding(16, 10)
            .Child(
                new Grid().Cols("*, 70, 90, 50")
                    .Children(
                        new StackPanel().Col(0).VerticalAlignment(VerticalAlignment.Center)
                            .Children(
                                new TextBlock().Text(item.ProductName).FontSize(13)
                                    .FontWeight(FontWeight.SemiBold).Foreground(Brushes.White)
                                    .TextWrapping(TextWrapping.Wrap).MaxHeight(36),
                                new TextBlock().Text(item.ProductCode).FontSize(10).Foreground(TextMuted)
                            ),
                        new TextBlock().Text($"Q{item.UnitPrice:F2}").FontSize(12).Foreground(TextMuted)
                            .Col(1).VerticalAlignment(VerticalAlignment.Center)
                            .HorizontalAlignment(HorizontalAlignment.Center),
                        new StackPanel().Orientation(Orientation.Horizontal).Spacing(6)
                            .Col(2).HorizontalAlignment(HorizontalAlignment.Center)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Children(decrementBtn, qtyLabel, incrementBtn),
                        new StackPanel().Orientation(Orientation.Horizontal).Spacing(4)
                            .Col(3).HorizontalAlignment(HorizontalAlignment.Right)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Children(subtotalLabel, removeBtn)
                    )
            );
    }

    private Control BuildRightPanel(SaleState state) =>
        new Grid().Rows("*, Auto")
            .Children(
                new ScrollViewer().Row(0)
                    .Content(
                        new StackPanel().Spacing(12)
                            .Children(
                                BuildTotalsCard(state),
                                BuildPaymentCard(state),
                                BuildExtraDataCard(state)
                            )
                    ),
                BuildConfirmButton(state).Row(1).Margin(0, 12, 0, 0)
            );

    private Control BuildTotalsCard(SaleState state)
    {
        var subtotalLabel = MakeValueLabel(state, x => $"Q{x.Subtotal:F2}");
        var discountLabel = MakeValueLabel(state, x => $"− Q{x.DiscountAmount:F2}");
        var taxLabel = MakeValueLabel(state, x =>
        {
            Decimal.TryParse(x.TaxAmount, out var t);
            return $"+ Q{t:F2}";
        });
        var totalLabel = new TextBlock().FontSize(22).FontWeight(FontWeight.Bold)
            .Foreground(AccentGreen).HorizontalAlignment(HorizontalAlignment.Right);

        void RefreshTotal()
        {
            totalLabel.Text = $"Q{state.Total:F2}";
        }

        RefreshTotal();
        state.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(state.Total) or nameof(state.Subtotal)
                or nameof(state.DiscountAmount) or nameof(state.TaxAmount))
                RefreshTotal();
        };

        var discountBox = new TextBox()
            .PlaceholderText("0").Background(BackgroundInput).Foreground(Brushes.White)
            .BorderBrush(BorderColor).CornerRadius(6).Padding(8, 6).Width(70)
            .Text(state, x => x.DiscountPercent, BindingMode.TwoWay);

        var taxBox = new TextBox()
            .PlaceholderText("0").Background(BackgroundInput).Foreground(Brushes.White)
            .BorderBrush(BorderColor).CornerRadius(6).Padding(8, 6).Width(70)
            .Text(state, x => x.TaxAmount, BindingMode.TwoWay);

        return new Border()
            .Background(BackgroundSecondary).CornerRadius(10).Padding(16)
            .Child(
                new StackPanel().Spacing(10)
                    .Children(
                        new TextBlock().Text("Resumen").FontSize(14).FontWeight(FontWeight.Bold)
                            .Foreground(Brushes.White),
                        BuildTotalRow("Subtotal", subtotalLabel),
                        new Grid().Cols("*, Auto, Auto")
                            .Children(
                                new TextBlock().Text("Descuento (%)").FontSize(12).Foreground(TextMuted)
                                    .VerticalAlignment(VerticalAlignment.Center).Col(0),
                                discountBox.Col(1).Margin(0, 0, 8, 0),
                                discountLabel.Col(2).VerticalAlignment(VerticalAlignment.Center)
                            ),
                        new Grid().Cols("*, Auto, Auto")
                            .Children(
                                new TextBlock().Text("Impuestos (Q)").FontSize(12).Foreground(TextMuted)
                                    .VerticalAlignment(VerticalAlignment.Center).Col(0),
                                taxBox.Col(1).Margin(0, 0, 8, 0),
                                taxLabel.Col(2).VerticalAlignment(VerticalAlignment.Center)
                            ),
                        new Border().Height(1).Background(BorderColor).Margin(0, 4),
                        new Grid().Cols("*, Auto")
                            .Children(
                                new TextBlock().Text("TOTAL").FontSize(16).FontWeight(FontWeight.Bold)
                                    .Foreground(Brushes.White).VerticalAlignment(VerticalAlignment.Center).Col(0),
                                totalLabel.Col(1)
                            )
                    )
            );
    }

    private Control BuildPaymentCard(SaleState state)
    {
        var paymentCombo = new ComboBox()
            .ItemsSource(state.PaymentMethods)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .Background(BackgroundTertiary)
            .Foreground(Brushes.White)
            .CornerRadius(6)
            .Padding(8, 6)
            .With(cb =>
            {
                cb.ItemTemplate = new FuncDataTemplate<PaymentMethodOption>((opt, _) =>
                    new TextBlock().Text(opt.Label).Foreground(Brushes.White).Padding(4, 2));
                cb.SelectedItem = state.PaymentMethods.First();
                cb.SelectionChanged += (_, _) =>
                {
                    if (cb.SelectedItem is PaymentMethodOption opt)
                        state.SelectedPaymentMethod = opt.Value;
                };
            });

        var cashPanel = new StackPanel().Spacing(8)
            .IsVisible(state, x => x.IsCashPayment);

        state.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(state.IsCashPayment))
                cashPanel.IsVisible = state.IsCashPayment;
        };

        var tenderedBox = new TextBox()
            .PlaceholderText("Monto recibido...")
            .Background(BackgroundInput).Foreground(Brushes.White)
            .BorderBrush(BorderColor).CornerRadius(6).Padding(10, 8)
            .Text(state, x => x.AmountTendered, BindingMode.TwoWay);

        var changeLabel = new TextBlock().FontSize(18).FontWeight(FontWeight.Bold)
            .Foreground(AccentGreen).HorizontalAlignment(HorizontalAlignment.Right);

        void RefreshChange()
        {
            changeLabel.Text = $"Vuelto: Q{state.ChangeAmount:F2}";
            changeLabel.IsVisible = state.HasChange;
        }

        RefreshChange();
        state.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(state.ChangeAmount) or nameof(state.HasChange))
                RefreshChange();
        };

        cashPanel.Children.Add(CreateSideField("Monto Recibido (Q)", tenderedBox));
        cashPanel.Children.Add(changeLabel);

        return new Border()
            .Background(BackgroundSecondary).CornerRadius(10).Padding(16)
            .Child(
                new StackPanel().Spacing(10)
                    .Children(
                        new TextBlock().Text("Pago").FontSize(14).FontWeight(FontWeight.Bold)
                            .Foreground(Brushes.White),
                        CreateSideField("Método de Pago", paymentCombo),
                        cashPanel
                    )
            );
    }

    private Control BuildExtraDataCard(SaleState state)
    {
        var toggleBtn = new Button()
            .Background(Brushes.Transparent).Foreground(TextMuted)
            .Padding(0).HorizontalAlignment(HorizontalAlignment.Stretch)
            .Cursor(new Cursor(StandardCursorType.Hand));

        var headerText = new TextBlock().FontSize(14).FontWeight(FontWeight.Bold).Foreground(Brushes.White);
        var chevron = new TextBlock().FontSize(11).Foreground(TextMuted)
            .VerticalAlignment(VerticalAlignment.Center);

        void RefreshHeader()
        {
            headerText.Text = state.ExtraDataExpanded ? "Datos Adicionales" : "➕ Datos Adicionales (opcional)";
            chevron.Text = state.ExtraDataExpanded ? "▲ Ocultar" : "▼ Mostrar";
        }

        RefreshHeader();
        state.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(state.ExtraDataExpanded)) RefreshHeader();
        };

        toggleBtn.Content = new Grid().Cols("*, Auto")
            .Children(headerText, chevron.Col(1));
        toggleBtn.Click += (_, _) => state.ToggleExtraDataCommand.Execute(null);

        var clientBox = new TextBox()
            .PlaceholderText("Nombre del cliente")
            .Background(BackgroundInput).Foreground(Brushes.White)
            .BorderBrush(BorderColor).CornerRadius(6).Padding(10, 8)
            .Text(state, x => x.ClientName, BindingMode.TwoWay);

        var doctorBox = new TextBox()
            .PlaceholderText("Cédula del médico")
            .Background(BackgroundInput).Foreground(Brushes.White)
            .BorderBrush(BorderColor).CornerRadius(6).Padding(10, 8)
            .Text(state, x => x.DoctorLicense, BindingMode.TwoWay);

        var invoiceBox = new TextBox()
            .PlaceholderText("No. de factura")
            .Background(BackgroundInput).Foreground(Brushes.White)
            .BorderBrush(BorderColor).CornerRadius(6).Padding(10, 8)
            .Text(state, x => x.InvoiceNumber, BindingMode.TwoWay);

        var notesBox = new TextBox()
            .PlaceholderText("Notas u observaciones...")
            .Background(BackgroundInput).Foreground(Brushes.White)
            .BorderBrush(BorderColor).CornerRadius(6).Padding(10, 8)
            .Height(64)
            .AcceptsReturn(true)
            .Text(state, x => x.Notes, BindingMode.TwoWay);

        var prescriptionToggleBtn = new Button()
            .Background(Brushes.Transparent).Padding(0)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .Cursor(new Cursor(StandardCursorType.Hand));

        var prescLabel = new TextBlock().FontSize(12).FontWeight(FontWeight.SemiBold);

        void RefreshPrescLabel()
        {
            prescLabel.Text = state.HasPrescription ? "📋 Receta adjunta ✅" : "📋 Adjuntar Receta Médica";
            prescLabel.Foreground = state.HasPrescription ? AccentGreen : TextMuted;
        }

        RefreshPrescLabel();
        state.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(state.HasPrescription)) RefreshPrescLabel();
        };
        prescriptionToggleBtn.Content = prescLabel;
        prescriptionToggleBtn.Click += (_, _) => state.TogglePrescriptionCommand.Execute(null);

        var prescriptionFields = new StackPanel().Spacing(8)
            .Margin(0, 8, 0, 0)
            .IsVisible(state, x => x.HasPrescription);

        state.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(state.HasPrescription))
                prescriptionFields.IsVisible = state.HasPrescription;
        };

        var prescDocNameBox = new TextBox()
            .PlaceholderText("Nombre del médico")
            .Background(BackgroundInput).Foreground(Brushes.White)
            .BorderBrush(BorderColor).CornerRadius(6).Padding(10, 8)
            .Text(state, x => x.PrescriptionDoctorName, BindingMode.TwoWay);

        var prescPatientBox = new TextBox()
            .PlaceholderText("Nombre del paciente")
            .Background(BackgroundInput).Foreground(Brushes.White)
            .BorderBrush(BorderColor).CornerRadius(6).Padding(10, 8)
            .Text(state, x => x.PrescriptionPatientName, BindingMode.TwoWay);

        var prescFolioBox = new TextBox()
            .PlaceholderText("Folio o referencia")
            .Background(BackgroundInput).Foreground(Brushes.White)
            .BorderBrush(BorderColor).CornerRadius(6).Padding(10, 8)
            .Text(state, x => x.PrescriptionFolio, BindingMode.TwoWay);

        var prescDatePicker = new DatePicker()
            .Background(BackgroundInput).Foreground(Brushes.White)
            .BorderBrush(BorderColor).CornerRadius(6).Padding(10, 8)
            .HorizontalAlignment(HorizontalAlignment.Stretch);
        prescDatePicker.Bind(DatePicker.SelectedDateProperty,
            new Binding(nameof(state.PrescriptionIssuedDate)) { Source = state, Mode = BindingMode.TwoWay });

        prescriptionFields.Children.Add(CreateSideField("Médico", prescDocNameBox));
        prescriptionFields.Children.Add(CreateSideField("Paciente", prescPatientBox));
        prescriptionFields.Children.Add(CreateSideField("Folio / Referencia", prescFolioBox));
        prescriptionFields.Children.Add(CreateSideDateField("Fecha de Emisión", prescDatePicker));

        var body = new StackPanel().Spacing(8).Margin(0, 12, 0, 0)
            .IsVisible(state, x => x.ExtraDataExpanded);

        state.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(state.ExtraDataExpanded))
                body.IsVisible = state.ExtraDataExpanded;
        };

        body.Children.Add(CreateSideField("Cliente", clientBox));
        body.Children.Add(CreateSideField("Cédula del Médico", doctorBox));
        body.Children.Add(CreateSideField("No. Factura", invoiceBox));
        body.Children.Add(CreateSideField("Notas", notesBox));
        body.Children.Add(new Border().Height(1).Background(BorderColor).Margin(0, 4));
        body.Children.Add(prescriptionToggleBtn);
        body.Children.Add(prescriptionFields);

        return new Border()
            .Background(BackgroundSecondary).CornerRadius(10).Padding(16)
            .Child(new StackPanel().Children(toggleBtn, body));
    }

    private Control BuildConfirmButton(SaleState state)
    {
        var btn = new Button()
            .Content("✅ Confirmar Venta")
            .Background(AccentGreen).Foreground(Brushes.White)
            .FontSize(16).FontWeight(FontWeight.Bold)
            .Padding(0, 14).CornerRadius(10)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .HorizontalContentAlignment(HorizontalAlignment.Center)
            .Cursor(new Cursor(StandardCursorType.Hand))
            .IsEnabled(state.CanConfirmSale);

        state.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName is nameof(state.CanConfirmSale))
                btn.IsEnabled = state.CanConfirmSale;
        };

        btn.Click += (_, _) => state.ConfirmSaleCommand.Execute(null);

        var clearBtn = new Button()
            .Content("🗑️ Limpiar Carrito")
            .Background(Brushes.Transparent).Foreground(DangerRed)
            .FontSize(12).Padding(0, 8).CornerRadius(6)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .HorizontalContentAlignment(HorizontalAlignment.Center)
            .Cursor(new Cursor(StandardCursorType.Hand))
            .IsVisible(state, x => !x.CartIsEmpty);

        state.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(state.CartIsEmpty))
                clearBtn.IsVisible = !state.CartIsEmpty;
        };

        clearBtn.Click += (_, _) => state.ClearCartCommand.Execute(null);

        return new StackPanel().Spacing(6).Children(btn, clearBtn);
    }

    private static Control BuildTotalRow(string label, Control valueControl) =>
        new Grid().Cols("*, Auto")
            .Children(
                new TextBlock().Text(label).FontSize(12).Foreground(TextMuted)
                    .VerticalAlignment(VerticalAlignment.Center).Col(0),
                valueControl.Col(1)
            );

    private static TextBlock MakeValueLabel(SaleState state, Func<SaleState, string> selector)
    {
        var tb = new TextBlock().FontSize(12).Foreground(Brushes.White)
            .HorizontalAlignment(HorizontalAlignment.Right);
        tb.Text = selector(state);
        state.PropertyChanged += (_, _) => tb.Text = selector(state);
        return tb;
    }

    private static Control CreateSideField(string label, Control input) =>
        new StackPanel().Spacing(4)
            .Children(
                new TextBlock().Text(label).FontSize(11).Foreground(TextMuted),
                input
            );

    private static Control CreateSideDateField(string label, DatePicker picker) =>
        new StackPanel().Spacing(4)
            .Children(
                new TextBlock().Text(label).FontSize(11).Foreground(TextMuted),
                picker
            );
}