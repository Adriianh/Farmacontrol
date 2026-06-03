using Avalonia.Input;
using Farmacontrol.Desktop.States;
using Farmacontrol.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Farmacontrol.Desktop.Views.Inventory;

public class SearchProductView() : ViewBase<SearchProductState>(Program.ServiceProvider.GetRequiredService<SearchProductState>())
{
    private static readonly SolidColorBrush BackgroundPrimary = SolidColorBrush.Parse("#111827");
    private static readonly SolidColorBrush BackgroundSecondary = SolidColorBrush.Parse("#1F2937");
    private static readonly SolidColorBrush BackgroundTertiary = SolidColorBrush.Parse("#374151");
    private static readonly SolidColorBrush TextMuted = SolidColorBrush.Parse("#9CA3AF");
    private static readonly SolidColorBrush AccentBlue = SolidColorBrush.Parse("#2563EB");
    private static readonly SolidColorBrush AccentGreen = SolidColorBrush.Parse("#10B981");
    private static readonly SolidColorBrush DangerRed = SolidColorBrush.Parse("#EF4444");
    private static readonly SolidColorBrush BorderColor = SolidColorBrush.Parse("#374151");

    protected override object Build(SearchProductState state) =>
        new Grid()
            .Children(
                new Border()
                    .Background(BackgroundPrimary)
                    .CornerRadius(12)
                    .Padding(20)
                    .Child(
                        new Grid().Rows("Auto, Auto, *")
                            .Children(
                                BuildHeader().Row(0),
                                BuildSearchBox(state).Row(1).IsVisible(state, x => !x.IsEditing),
                                BuildResultContainer(state).Row(2)
                            )
                    )
            );

    private Control BuildHeader() =>
        new StackPanel().Margin(0, 0, 0, 20)
            .Children(
                new TextBlock().Text("Inspector y Editor en Vivo").FontSize(24).FontWeight(FontWeight.Bold)
                    .Foreground(Brushes.White),
                new TextBlock()
                    .Text("Busque un medicamento por criterio y modifique sus valores directamente en este panel")
                    .FontSize(13).Foreground(TextMuted)
            );

    private Control BuildSearchBox(SearchProductState state)
    {
        var searchTextBox = new TextBox()
            .PlaceholderText("🔍 Busque por nombre o código único del medicamento...")
            .Background(BackgroundSecondary)
            .Foreground(Brushes.White)
            .BorderBrush(BorderColor)
            .CornerRadius(8)
            .Padding(14, 12)
            .Text(state, x => x.SearchQuery, BindingMode.TwoWay);

        searchTextBox.KeyDown += (s, e) =>
        {
            if (e.Key == Key.Enter) state.ExecuteSearch();
        };
        return searchTextBox;
    }

    private Control BuildResultContainer(SearchProductState state) =>
        new ContentControl()
            .Content(state, x =>
                x.IsEditing ? BuildInlineEditorForm(state) :
                x.HasSimilarResults ? BuildSimilarResultsList(state) :
                x.HasSearched ? BuildNotFoundState() :
                BuildEmptyState()
            );

    private Control BuildInlineEditorForm(SearchProductState state)
    {
        var form = state.ProductForm;
        
        var cancelButton = new Button().Content("Cancelar").Background(Brushes.Transparent)
            .Foreground(Brushes.White).Padding(16, 10).CornerRadius(6).Col(1)
            .Margin(0, 0, 8, 0);

        cancelButton.Click += (_, _) => state.CancelInlineEdit();
        var saveButton = new Button().Content("💾 Guardar Cambios").Background(AccentBlue)
            .Foreground(Brushes.White).FontWeight(FontWeight.SemiBold).Padding(16, 10)
            .CornerRadius(6).Col(2);
        saveButton.Click += (_, _) => state.SaveInlineChanges();

        return new ScrollViewer().Margin(0, 10, 0, 0)
            .Content(
                new StackPanel().Spacing(16)
                    .Children(
                        new Border().Background(SolidColorBrush.Parse("#7F1D1D")).BorderBrush(DangerRed)
                            .BorderThickness(1).CornerRadius(6).Padding(12).Margin(0, 0, 0, 10)
                            .IsVisible(form, x => !string.IsNullOrEmpty(x.ErrorMessage))
                            .Child(new TextBlock().Text(form, x => x.ErrorMessage)
                                .Foreground(SolidColorBrush.Parse("#FCA5A5")).FontSize(12)),
                        new TextBlock().Text("📝 Editando Datos del Producto").FontSize(16).FontWeight(FontWeight.Bold)
                            .Foreground(AccentGreen),

                        new Grid().Cols("*, *")
                            .Children(
                                BuildFormRow("Nombre Comercial *",
                                    new TextBox().Text(form, x => x.Name, BindingMode.TwoWay)).Col(0).Margin(0, 0, 8, 0),
                                BuildFormRow("Código Único *",
                                        new TextBox().Text(form, x => x.Code, BindingMode.TwoWay).IsEnabled(false)).Col(1)
                                    .Margin(8, 0, 0, 0)
                            ),

                        new Grid().Cols("*, *, *")
                            .Children(
                                BuildFormRow("Precio de Venta (Q) *",
                                    new TextBox().Text(form, x => x.Price, BindingMode.TwoWay)).Col(0).Margin(0, 0, 6, 0),
                                BuildFormRow("Stock Total",
                                    new TextBox().Text(form, x => x.Stock, BindingMode.TwoWay)
                                        .IsEnabled(form, x => !x.EnableBatches)).Col(1).Margin(6, 0, 6, 0),
                                BuildFormRow("Stock Mínimo Alerta",
                                        new TextBox().Text(form, x => x.MinimumStock, BindingMode.TwoWay)).Col(2)
                                    .Margin(6, 0, 0, 0)
                            ),

                        new Grid().Cols("*, *")
                            .Children(
                                BuildFormRow("Laboratorio Fabricante",
                                        new TextBox().Text(form, x => x.Laboratory, BindingMode.TwoWay)).Col(0)
                                    .Margin(0, 0, 8, 0),
                                BuildFormRow("Ubicación Física (Anaquel)",
                                        new TextBox().Text(form, x => x.Location, BindingMode.TwoWay)).Col(1)
                                    .Margin(8, 0, 0, 0)
                            ),

                        new Border().BorderBrush(BorderColor).BorderThickness(0, 1, 0, 0).Padding(0, 16, 0, 0)
                            .Margin(0, 12, 0, 0)
                            .Child(
                                new Grid().Cols("*, Auto, Auto")
                                    .Children(
                                        cancelButton,
                                        saveButton
                                    )
                            )
                    )
            );
    }

    private static StackPanel BuildFormRow(string label, TextBox input)
    {
        input.Background(BackgroundSecondary);
        input.Foreground(Brushes.White);
        input.BorderBrush(BorderColor);
        input.CornerRadius(6);
        input.Padding(10, 8);

        return new StackPanel().Spacing(6)
            .Children(
                new TextBlock().Text(label).FontSize(12).FontWeight(FontWeight.Medium).Foreground(TextMuted),
                input
            );
    }

    private Control BuildSimilarResultsList(SearchProductState state)
    {
        var container = new StackPanel().Spacing(12).Margin(0, 16, 0, 0);
        container.Children.Add(new TextBlock()
            .Text($"🔍 Coincidencias similares encontradas ({state.SimilarProducts.Count})").FontSize(14)
            .FontWeight(FontWeight.Bold).Foreground(AccentGreen));

        var listBox = new ListBox().Background(Brushes.Transparent).ItemsSource(state.SimilarProducts)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .ItemTemplate<Product>(product =>
            {
                var btn = new Button().Background(BackgroundSecondary).HorizontalAlignment(HorizontalAlignment.Stretch)
                    .Padding(16, 12).CornerRadius(8).BorderBrush(BorderColor).BorderThickness(1)
                    .Content(new Grid().Cols("*, Auto").Children(
                        new StackPanel().Col(0).Children(
                            new TextBlock().Text(product.Name).FontSize(15).FontWeight(FontWeight.SemiBold)
                                .Foreground(Brushes.White),
                            new TextBlock().Text($"Código: {product.Code} | Laboratorio: {product.Laboratory ?? "N/D"}")
                                .FontSize(11).Foreground(TextMuted).Margin(0, 4, 0, 0)
                        ),
                        new TextBlock().Text("Editar ➔").FontSize(12).Foreground(AccentBlue).Col(1)
                            .VerticalAlignment(VerticalAlignment.Center)
                    ));
                btn.Click += (_, _) => state.SetupInlineForm(product);
                return btn;
            });

        container.Children.Add(listBox);
        return container;
    }

    private Control BuildEmptyState() => new StackPanel().VerticalAlignment(VerticalAlignment.Center)
        .HorizontalAlignment(HorizontalAlignment.Center).Spacing(10).Children(
            new TextBlock().Text("🔍").FontSize(48).HorizontalAlignment(HorizontalAlignment.Center),
            new TextBlock().Text("Escriba un término para inspeccionar y editar...").FontSize(14)
                .Foreground(TextMuted));

    private Control BuildNotFoundState() => new StackPanel().VerticalAlignment(VerticalAlignment.Center)
        .HorizontalAlignment(HorizontalAlignment.Center).Spacing(10).Children(
            new TextBlock().Text("⚠️").FontSize(48).HorizontalAlignment(HorizontalAlignment.Center),
            new TextBlock().Text("No se encontró ningún producto.").FontSize(14).Foreground(DangerRed));
}