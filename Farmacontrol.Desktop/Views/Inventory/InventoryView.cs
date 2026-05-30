using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using Avalonia.Styling;
using Farmacontrol.Desktop.States;
using Farmacontrol.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Farmacontrol.Desktop.Views.Inventory;

public class InventoryView() : ViewBase<InventoryState>(Program.ServiceProvider.GetRequiredService<InventoryState>())
{
    private static readonly SolidColorBrush BackgroundPrimary = SolidColorBrush.Parse("#111827");
    private static readonly SolidColorBrush BackgroundSecondary = SolidColorBrush.Parse("#1F2937");
    private static readonly SolidColorBrush BackgroundTertiary = SolidColorBrush.Parse("#374151");
    private static readonly SolidColorBrush BackgroundHover = SolidColorBrush.Parse("#4B5563");
    private static readonly SolidColorBrush TextMuted = SolidColorBrush.Parse("#9CA3AF");
    private static readonly SolidColorBrush TextSubtle = SolidColorBrush.Parse("#64748B");
    private static readonly SolidColorBrush AccentBlue = SolidColorBrush.Parse("#2563EB");
    private static readonly SolidColorBrush AccentBlueHover = SolidColorBrush.Parse("#1D4ED8");
    private static readonly SolidColorBrush AccentGreen = SolidColorBrush.Parse("#10B981");
    private static readonly SolidColorBrush DangerRed = SolidColorBrush.Parse("#EF4444");
    private static readonly SolidColorBrush BorderColor = SolidColorBrush.Parse("#374151");
    private static readonly SolidColorBrush DividerColor = SolidColorBrush.Parse("#111827");

    protected override object Build(InventoryState state) =>
        new Border().RowSpan(2)
            .Background(BackgroundPrimary)
            .CornerRadius(12)
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
                new Style(x => x.OfType<FlyoutPresenter>())
                {
                    Setters =
                    {
                        new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent),
                        new Setter(TemplatedControl.PaddingProperty, new Thickness(0)),
                        new Setter(TemplatedControl.BorderBrushProperty, Brushes.Transparent),
                        new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(0))
                    }
                }
            )
            .Child(
                new Grid().Rows("Auto, Auto, *")
                    .Children(
                        BuildHeader().Row(0).Margin(20),
                        BuildSearchBar(state).Row(1),
                        BuildProductList(state).Row(2)
                    )
            );

    private Control BuildHeader()
    {
        var addButton = new Button()
            .Content("➕ Agregar Medicamento")
            .Background(AccentBlue)
            .Foreground(Brushes.White)
            .FontWeight(FontWeight.SemiBold)
            .Padding(16, 10)
            .CornerRadius(8);

        addButton.Styles(
            new Style(x => x.OfType<Button>().Class(":pointerover").Template().OfType<ContentPresenter>())
            {
                Setters =
                {
                    new Setter(ContentPresenter.BackgroundProperty, AccentBlueHover),
                    new Setter(ContentPresenter.ForegroundProperty, Brushes.White)
                }
            }
        );

        return new Grid().Cols("*, Auto")
            .Children(
                new StackPanel().VerticalAlignment(VerticalAlignment.Center)
                    .Children(
                        new TextBlock()
                            .Text("Gestión de Inventario")
                            .FontSize(26)
                            .FontWeight(FontWeight.Bold)
                            .Foreground(Brushes.White),
                        new TextBlock()
                            .Text("Control total de medicamentos y stock en tiempo real")
                            .FontSize(13)
                            .Foreground(TextMuted)
                            .Margin(0, 4, 0, 0)
                    ),
                addButton.Col(1)
            );
    }

    private Control BuildSearchBar(InventoryState state) =>
        new Border()
            .Background(BackgroundSecondary)
            .CornerRadius(10)
            .Padding(16)
            .Margin(10, 0, 10, 16)
            .Child(
                new Grid().Cols("*, Auto")
                    .Children(
                        new TextBox()
                            .PlaceholderText("🔍 Buscar por nombre o código...")
                            .PlaceholderForeground(TextMuted)
                            .Background(BackgroundTertiary)
                            .Foreground(Brushes.White)
                            .BorderBrush(Brushes.Transparent)
                            .CornerRadius(8)
                            .Padding(12, 8)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Text(state, x => x.SearchText, BindingMode.TwoWay),
                        BuildFilterDropdown(state).Col(1).Margin(12, 0, 0, 0)
                    )
            );

    private Control BuildFilterDropdown(InventoryState state)
    {
        var filterMenuButton = new Button()
            .Background(BackgroundTertiary)
            .Foreground(Brushes.White)
            .CornerRadius(8)
            .Padding(16, 8)
            .VerticalAlignment(VerticalAlignment.Center)
            .Content("Filtrar y Ordenar ▾");

        var flyout = new Flyout
        {
            Placement = PlacementMode.BottomEdgeAlignedRight,
            ShowMode = FlyoutShowMode.Standard
        };

        var menuContainer = new StackPanel();

        var dropdownContent = new Border()
            .Background(BackgroundSecondary)
            .BorderBrush(BorderColor)
            .BorderThickness(1)
            .CornerRadius(8)
            .Padding(4)
            .MinWidth(180)
            .Child(menuContainer);

        flyout.Content = dropdownContent;

        flyout.Opened += (_, _) =>
        {
            menuContainer.Children.Clear();
            menuContainer.Children.AddRange(new List<Control>
            {
                BuildDropdownButton("Nombre", state.SortCriterionIndex == 0, () => state.SortCriterionIndex = 0,
                    flyout),
                BuildDropdownButton("Stock", state.SortCriterionIndex == 1, () => state.SortCriterionIndex = 1, flyout),
                BuildDropdownButton("Precio", state.SortCriterionIndex == 2, () => state.SortCriterionIndex = 2,
                    flyout),
                new Separator()
                {
                    Margin = new Thickness(0, 4, 0, 4),
                    Background = DividerColor,
                    Height = 1
                },
                BuildDropdownButton("Ascendente", state.AscendingOrder, () =>
                {
                    if (!state.AscendingOrder) state.ToggleSortDirection();
                }, flyout),
                BuildDropdownButton("Descendente", !state.AscendingOrder, () =>
                {
                    if (state.AscendingOrder) state.ToggleSortDirection();
                }, flyout)
            });
        };

        filterMenuButton.Flyout(flyout);
        return filterMenuButton;
    }

    private Control BuildDropdownButton(string text, bool isActive, System.Action onClickAction, Flyout flyout)
    {
        var textBlock = new TextBlock()
            .Text(text)
            .VerticalAlignment(VerticalAlignment.Center)
            .Foreground(isActive ? AccentBlue : Brushes.White);

        var contentGrid = new Grid().Cols("*, Auto")
            .Children(textBlock.Col(0));

        if (isActive)
        {
            var checkBlock = new TextBlock()
                .Text("✓")
                .FontWeight(FontWeight.Bold)
                .Foreground(AccentBlue)
                .VerticalAlignment(VerticalAlignment.Center)
                .Margin(8, 0, 4, 0);

            contentGrid.Children(checkBlock.Col(1));
        }

        var button = new Button()
            .Background(Brushes.Transparent)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .HorizontalContentAlignment(HorizontalAlignment.Left)
            .Padding(12, 8)
            .CornerRadius(4)
            .Content(contentGrid);

        button.Styles(
            new Style(x => x.OfType<Button>().Class(":pointerover").Template().OfType<ContentPresenter>())
            {
                Setters =
                {
                    new Setter(ContentPresenter.BackgroundProperty, BackgroundHover),
                    new Setter(ContentPresenter.ForegroundProperty, Brushes.White)
                }
            }
        );

        button.Click += (_, _) =>
        {
            onClickAction();
            flyout.Hide();
        };

        return button;
    }

    private Control BuildProductList(InventoryState state) =>
        new ListBox()
            .Background(Brushes.Transparent)
            .Margin(0, 12, 0, 0)
            .ItemsSource(state, x => x.FilteredProducts)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .ItemTemplate<Product>(BuildProductItem);

    private Control BuildProductItem(Product product) =>
        new Border()
            .Background(BackgroundSecondary)
            .CornerRadius(12)
            .Margin(0, 0, 0, 12)
            .Padding(20, 16)
            .BorderBrush(BorderColor)
            .BorderThickness(1)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .Child(
                new Grid().Cols("Auto, *, 120, 120, Auto")
                    .HorizontalAlignment(HorizontalAlignment.Stretch)
                    .Children(
                        new TextBlock()
                            .Text("📦")
                            .FontSize(22)
                            .Margin(0, 0, 16, 0)
                            .VerticalAlignment(VerticalAlignment.Center),
                        BuildProductInfo(product).Col(1),
                        BuildStockColumn(product).Col(2),
                        BuildPriceColumn(product).Col(3),
                        BuildActionButtons().Col(4)
                    )
            );

    private Control BuildProductInfo(Product product) =>
        new StackPanel().VerticalAlignment(VerticalAlignment.Center)
            .Children(
                new TextBlock()
                    .Text(product, p => p.Name)
                    .FontSize(16)
                    .FontWeight(FontWeight.SemiBold)
                    .Foreground(Brushes.White),
                new StackPanel()
                    .Orientation(Orientation.Horizontal)
                    .Margin(0, 4, 0, 0)
                    .Children(
                        new TextBlock()
                            .Text("Código: ")
                            .FontSize(12)
                            .Foreground(TextMuted),
                        new TextBlock()
                            .Text(product, p => p.Code)
                            .FontSize(12)
                            .Foreground(TextSubtle)
                    )
            );

    private Control BuildStockColumn(Product product) =>
        new StackPanel()
            .VerticalAlignment(VerticalAlignment.Center)
            .HorizontalAlignment(HorizontalAlignment.Center)
            .Children(
                new TextBlock()
                    .Text("STOCK")
                    .FontSize(10)
                    .FontWeight(FontWeight.Bold)
                    .Foreground(TextMuted)
                    .HorizontalAlignment(HorizontalAlignment.Center),
                new TextBlock()
                    .Text(product, p => p.Stock)
                    .FontSize(16)
                    .FontWeight(FontWeight.Bold)
                    .Foreground(Brushes.White)
                    .Margin(0, 4, 0, 0)
                    .HorizontalAlignment(HorizontalAlignment.Center)
            );

    private Control BuildPriceColumn(Product product) =>
        new StackPanel()
            .VerticalAlignment(VerticalAlignment.Center)
            .HorizontalAlignment(HorizontalAlignment.Center)
            .Children(
                new TextBlock()
                    .Text("PRECIO")
                    .FontSize(10)
                    .FontWeight(FontWeight.Bold)
                    .Foreground(TextMuted)
                    .HorizontalAlignment(HorizontalAlignment.Center),
                new StackPanel()
                    .Orientation(Orientation.Horizontal)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .Margin(0, 4, 0, 0)
                    .Children(
                        new TextBlock()
                            .Text("Q ")
                            .FontSize(16)
                            .FontWeight(FontWeight.Bold)
                            .Foreground(AccentGreen),
                        new TextBlock()
                            .Text(product, p => p.Price)
                            .FontSize(16)
                            .FontWeight(FontWeight.Bold)
                            .Foreground(AccentGreen)
                    )
            );

    private Control BuildActionButtons() =>
        new StackPanel()
            .Orientation(Orientation.Horizontal)
            .VerticalAlignment(VerticalAlignment.Center)
            .Margin(16, 0, 0, 0)
            .Children(
                new Button()
                    .Content("✏️")
                    .Background(BackgroundTertiary)
                    .Foreground(Brushes.White)
                    .Padding(8)
                    .CornerRadius(6)
                    .Margin(0, 0, 8, 0),
                new Button()
                    .Content("🗑️")
                    .Background(DangerRed)
                    .Foreground(Brushes.White)
                    .Padding(8)
                    .CornerRadius(6)
            );
}