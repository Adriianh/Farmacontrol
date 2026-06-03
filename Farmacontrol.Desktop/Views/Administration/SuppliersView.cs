using Avalonia.Controls.Presenters;
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
                        }
                    )
                    .Child(
                        new Grid().Rows("Auto, Auto, *")
                            .Children(
                                BuildHeader(state).Row(0).Margin(20),
                                BuildSearchBar(state).Row(1),
                                BuildSupplierList(state).Row(2)
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
        removeButton.Click += (_, _) => state.DeleteSupplier(supplier);

        return new Border()
            .Background(BackgroundSecondary)
            .CornerRadius(10)
            .Margin(0, 0, 0, 10)
            .Padding(16)
            .BorderBrush(BorderColor)
            .BorderThickness(1)
            .Child(
                new Grid().Cols("Auto, *, Auto, Auto")
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
                                        new TextBlock().Text(supplier.Name).FontSize(16).FontWeight(FontWeight.SemiBold)
                                            .Foreground(Brushes.White),
                                        new Border().Background(supplier.IsActive ? AccentGreen : DangerRed)
                                            .CornerRadius(4).Padding(6, 2)
                                            .Child(new TextBlock().Text(supplier.IsActive ? "Activo" : "Inactivo")
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
                                new TextBlock().Text($"📞 {supplier.PhoneNumber}  |  ✉️ {supplier.Email}").FontSize(12)
                                    .Foreground(TextMuted).Margin(0, 4, 0, 0),
                                string.IsNullOrEmpty(supplier.Address)
                                    ? new Panel()
                                    : new TextBlock().Text($"📍 {supplier.Address}").FontSize(11).Foreground(TextSubtle)
                                        .Margin(0, 2, 0, 0)
                            ).Col(1),
                        editButton.Col(3),
                        removeButton.Col(3)
                    )
            );
    }

    private static Control BuildBadge(string text) =>
        new Border()
            .Background(BackgroundTertiary)
            .CornerRadius(4)
            .Padding(6, 3)
            .Margin(0, 0, 6, 4)
            .Child(new TextBlock().Text(text).FontSize(11).Foreground(SolidColorBrush.Parse("#9CA3AF")));
}