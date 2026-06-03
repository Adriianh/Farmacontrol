using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Farmacontrol.Core.Model;
using Farmacontrol.Desktop.Components;
using Farmacontrol.Desktop.States;
using Microsoft.Extensions.DependencyInjection;

namespace Farmacontrol.Desktop.Views.Administration;

public class SuppliersView() : ViewBase<SupplierState>(Program.ServiceProvider.GetRequiredService<SupplierState>())
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

    protected override object Build(SupplierState state) =>
        new Grid()
            .Children(
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
                    .Child(
                        new Grid().Rows("Auto, Auto, Auto, *") 
                            .Children(
                                BuildHeader(state).Row(0).Margin(20),
                                BuildSearchBar(state).Row(1),
                                BuildErrorPanel(state).Row(2),
                                BuildSupplierList(state).Row(3)
                            )
                    ),
                SupplierModal.Build(
                    state,
                    onCancel: state.CloseModal,
                    onSave: state.SaveSupplier
                ).IsVisible(state, x => x.IsModalOpen)
            );

    private Control BuildHeader(SupplierState state)
    {
        var addButton = new Button()
            .Content("🏢 Agregar Proveedor")
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

        addButton.Click += (_, _) => state.PrepareAddSupplier();

        return new Grid().Cols("*, Auto")
            .Children(
                new StackPanel().VerticalAlignment(VerticalAlignment.Center)
                    .Children(
                        new TextBlock()
                            .Text("Catálogo de Proveedores")
                            .FontSize(26)
                            .FontWeight(FontWeight.Bold)
                            .Foreground(Brushes.White),
                        new TextBlock()
                            .Text("Administración de laboratorios, distribución, tiempos de entrega y contacto")
                            .FontSize(13)
                            .Foreground(TextMuted)
                            .Margin(0, 4, 0, 0)
                    ),
                addButton.Col(1)
            );
    }

    private Control BuildSearchBar(SupplierState state) =>
        new Border()
            .Background(BackgroundSecondary)
            .CornerRadius(10)
            .Padding(16)
            .Margin(10, 0, 10, 16)
            .Child(
                new TextBox()
                    .PlaceholderText("🔍 Buscar proveedor por nombre, código o contacto...")
                    .PlaceholderForeground(TextMuted)
                    .Background(BackgroundTertiary)
                    .Foreground(Brushes.White)
                    .BorderBrush(Brushes.Transparent)
                    .CornerRadius(8)
                    .Padding(12, 8)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Text(state, x => x.SearchText, BindingMode.TwoWay)
            );

    private Control BuildSupplierList(SupplierState state) =>
        new ListBox()
            .Background(Brushes.Transparent)
            .Margin(10, 0, 10, 0)
            .ItemsSource(state, x => x.FilteredSuppliers)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .ItemTemplate<Supplier>(supplier => BuildSupplierItem(supplier, state));

    private Control BuildSupplierItem(Supplier? supplier, SupplierState state)
    {
        if (supplier == null) return new ContentControl();

        var editButton = new Button().Content("✏️").Background(BackgroundTertiary).Foreground(Brushes.White)
            .Padding(10)
            .CornerRadius(6).Margin(0, 0, 8, 0);
        editButton.Click += (_, _) => state.PrepareEditSupplier(supplier);

        var removeButton = new Button().Content("🗑️").Background(DangerRed).Foreground(Brushes.White).Padding(10)
            .CornerRadius(6);
        removeButton.Click += (_, e) =>
        {
            e.Handled = true;
            state.DeleteSupplier(supplier);
        };;

        return new Border()
            .Background(BackgroundSecondary)
            .CornerRadius(12)
            .Margin(0, 0, 0, 12)
            .BorderBrush(BorderColor)
            .BorderThickness(1)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .Child(
                new Expander()
                    .HorizontalAlignment(HorizontalAlignment.Stretch)
                    .Padding(0)
                    .Header(
                        new Border()
                            .Padding(16, 12)
                            .Child(
                                new Grid().Cols("Auto, *, Auto, Auto")
                                    .HorizontalAlignment(HorizontalAlignment.Stretch)
                                    .Children(
                                        new Border()
                                            .Background(BackgroundTertiary)
                                            .CornerRadius(8)
                                            .Width(45).Height(45)
                                            .Margin(0, 0, 16, 0)
                                            .Child(
                                                new TextBlock()
                                                    .Text(supplier.IsActive ? "🏢" : "❌")
                                                    .FontSize(20)
                                                    .HorizontalAlignment(HorizontalAlignment.Center)
                                                    .VerticalAlignment(VerticalAlignment.Center)
                                            ).Col(0),
                                        new StackPanel().VerticalAlignment(VerticalAlignment.Center)
                                            .Children(
                                                new StackPanel().Orientation(Orientation.Horizontal).Spacing(8)
                                                    .Children(
                                                        new TextBlock().Text(supplier.Name).FontSize(16)
                                                            .FontWeight(FontWeight.SemiBold)
                                                            .Foreground(Brushes.White),
                                                        new Border()
                                                            .Background(supplier.IsActive ? AccentGreen : DangerRed)
                                                            .CornerRadius(4).Padding(6, 2)
                                                            .Child(new TextBlock()
                                                                .Text(supplier.IsActive ? "Activo" : "Inactivo")
                                                                .FontSize(10).Foreground(Brushes.White))
                                                    ),
                                                new WrapPanel().Orientation(Orientation.Horizontal).Margin(0, 4, 0, 0)
                                                    .Children(
                                                        BuildBadge($"Código: {supplier.Code}"),
                                                        string.IsNullOrEmpty(supplier.TaxId)
                                                            ? new Panel()
                                                            : BuildBadge($"NIT: {supplier.TaxId}"),
                                                        string.IsNullOrEmpty(supplier.ContactName)
                                                            ? new Panel()
                                                            : BuildBadge($"Contacto: {supplier.ContactName}"),
                                                        BuildBadge($"Entrega: {supplier.LeadTimeDays} días")
                                                    ),
                                                new TextBlock()
                                                    .Text($"📞 {supplier.PhoneNumber}  |  ✉️ {supplier.Email}")
                                                    .FontSize(12)
                                                    .Foreground(TextMuted).Margin(0, 4, 0, 0),
                                                string.IsNullOrEmpty(supplier.Address)
                                                    ? new Panel()
                                                    : new TextBlock().Text($"📍 {supplier.Address}").FontSize(11)
                                                        .Foreground(TextSubtle)
                                                        .Margin(0, 2, 0, 0)
                                            ).Col(1),
                                        editButton.Col(2),
                                        removeButton.Col(3)
                                    )
                            )
                    )
                    .Content(
                        new Border()
                            .Background(
                                BackgroundPrimary)
                            .BorderBrush(BorderColor)
                            .BorderThickness(0, 1, 0, 0)
                            .CornerRadius(0, 0, 12, 12)
                            .Padding(16, 16)
                            .Child(
                                BuildSupplierProductsSummary(supplier)
                            )
                    )
            );
    }

    private static Control BuildSupplierProductsSummary(Supplier supplier)
    {
        var products = supplier.Products.ToList();

        if (products.Count == 0)
        {
            return new TextBlock()
                .Text("📦 Ningún producto asignado a este proveedor actualmente.")
                .FontSize(12)
                .FontStyle(FontStyle.Italic)
                .Foreground(TextSubtle);
        }

        var productContainer = new WrapPanel().Orientation(Orientation.Horizontal);

        foreach (var product in products)
        {
            var productBadge = new Border()
                .Background(BackgroundTertiary)
                .BorderBrush(BorderColor)
                .BorderThickness(1)
                .CornerRadius(6)
                .Padding(8, 4)
                .Margin(0, 0, 8, 6)
                .Child(
                    new StackPanel().Orientation(Orientation.Horizontal).Spacing(6)
                        .Children(
                            new TextBlock().Text("📦").FontSize(12),
                            new TextBlock().Text($"{product.Name}").FontSize(12).FontWeight(FontWeight.SemiBold)
                                .Foreground(Brushes.White),
                            new TextBlock().Text($"({product.Code})").FontSize(11).Foreground(TextMuted)
                        )
                );

            productContainer.Children.Add(productBadge);
        }

        return new StackPanel().Spacing(8)
            .Children(
                new TextBlock()
                    .Text($"🛍️ Productos Surtidos ({products.Count})")
                    .FontSize(12)
                    .FontWeight(FontWeight.Bold)
                    .Foreground(TextMuted),
                productContainer
            );
    }

    private static Control BuildBadge(string text) =>
        new Border()
            .Background(BackgroundTertiary)
            .CornerRadius(4)
            .Padding(6, 3)
            .Margin(0, 0, 6, 4)
            .Child(new TextBlock().Text(text).FontSize(11).Foreground(SolidColorBrush.Parse("#9CA3AF")));
    
    private static Control BuildErrorPanel(SupplierState state) =>
        new Border()
            .Background(SolidColorBrush.Parse("#7F1D1D"))
            .BorderBrush(SolidColorBrush.Parse("#DC2626"))
            .BorderThickness(1)
            .CornerRadius(8)
            .Padding(16, 12)
            .Margin(10, 0, 10, 16)
            .IsVisible(state, x => x.HasErrorMessage)
            .Child(
                new Grid().Cols("Auto, *")
                    .Children(
                        new TextBlock()
                            .Text("⚠️")
                            .FontSize(18)
                            .Foreground(SolidColorBrush.Parse("#FCA5A5"))
                            .Margin(0, 0, 12, 0)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Col(0),
                        new TextBlock()
                            .Text(state, x => x.ErrorMessage)
                            .Foreground(SolidColorBrush.Parse("#FCA5A5"))
                            .FontSize(13)
                            .FontWeight(FontWeight.Medium)
                            .TextWrapping(TextWrapping.Wrap)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Col(1)
                    )
            );
}