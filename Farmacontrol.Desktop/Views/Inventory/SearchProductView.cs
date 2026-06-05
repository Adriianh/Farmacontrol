using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Styling;
using Farmacontrol.Core.Model;
using Farmacontrol.Core.Model.ProductEntity;
using Farmacontrol.Desktop.States;
using Farmacontrol.Model;
using Microsoft.Extensions.DependencyInjection;

namespace Farmacontrol.Desktop.Views.Inventory;

public class SearchProductView()
    : ViewBase<SearchProductState>(Program.ServiceProvider.GetRequiredService<SearchProductState>())
{
    private static readonly SolidColorBrush BackgroundPrimary = SolidColorBrush.Parse("#111827");
    private static readonly SolidColorBrush BackgroundSecondary = SolidColorBrush.Parse("#1F2937");
    private static readonly SolidColorBrush BackgroundTertiary = SolidColorBrush.Parse("#374151");
    private static readonly SolidColorBrush BackgroundHover = SolidColorBrush.Parse("#4B5563");
    private static readonly SolidColorBrush TextMuted = SolidColorBrush.Parse("#9CA3AF");
    private static readonly SolidColorBrush AccentBlue = SolidColorBrush.Parse("#2563EB");
    private static readonly SolidColorBrush AccentGreen = SolidColorBrush.Parse("#10B981");
    private static readonly SolidColorBrush DangerRed = SolidColorBrush.Parse("#EF4444");
    private static readonly SolidColorBrush BorderColor = SolidColorBrush.Parse("#374151");
    private static readonly SolidColorBrush ErrorBackground = SolidColorBrush.Parse("#7F1D1D");
    private static readonly SolidColorBrush ErrorBorder = SolidColorBrush.Parse("#DC2626");
    private static readonly SolidColorBrush ErrorText = SolidColorBrush.Parse("#FCA5A5");

    protected override object Build(SearchProductState state) =>
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
                        new Style(x => x.OfType<FlyoutPresenter>())
                        {
                            Setters =
                            {
                                new Setter(TemplatedControl.BackgroundProperty, Brushes.Transparent),
                                new Setter(TemplatedControl.PaddingProperty, new Thickness(0)),
                                new Setter(TemplatedControl.BorderBrushProperty, Brushes.Transparent),
                                new Setter(TemplatedControl.BorderThicknessProperty, new Thickness(0))
                            }
                        })
                    .Child(
                        new Grid().Rows("Auto, Auto, *")
                            .Children(
                                BuildHeader().Row(0),
                                BuildSearchBox(state).Row(1).IsVisible(state, x => !x.ShowEditing),
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
        return new Border()
            .Background(BackgroundSecondary)
            .CornerRadius(10)
            .Padding(16)
            .Margin(10, 0, 10, 16)
            .Child(
                new Grid().Cols("*, Auto")
                    .Children(
                        new TextBox()
                            .PlaceholderText("🔍 Buscar por nombre, código o código de barras...")
                            .PlaceholderForeground(TextMuted)
                            .Background(BackgroundTertiary)
                            .Foreground(Brushes.White)
                            .BorderBrush(Brushes.Transparent)
                            .CornerRadius(8)
                            .Padding(12, 8)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Text(state, x => x.SearchQuery, BindingMode.TwoWay)
                            .With(textBox =>
                            {
                                textBox.KeyDown += (_, e) =>
                                {
                                    if (e.Key != Key.Enter) return;
                                    state.ExecuteSearch();
                                    e.Handled = true;
                                };
                            }),
                        new Button()
                            .Content("🔍")
                            .Background(AccentBlue)
                            .Foreground(Brushes.White)
                            .CornerRadius(8)
                            .Padding(12, 8)
                            .Margin(12, 0, 0, 0)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .With(button => button.Click += (_, _) => state.ExecuteSearch())
                            .Col(1)
                    )
            );
    }

    private Control BuildResultContainer(SearchProductState state)
    {
        var emptyState = BuildEmptyState()
            .IsVisible(state, x => x.ShowEmpty);

        var similarResults = BuildSimilarResultsList(state)
            .IsVisible(state, x => x.ShowSimilarResults);

        var editorForm = BuildFullEditorForm(state)
            .IsVisible(state, x => x.ShowEditing);

        return new Panel().Children(emptyState, similarResults, editorForm);
    }

    private Control BuildSimilarResultsList(SearchProductState state)
    {
        var listBox = new ListBox()
            .Background(Brushes.Transparent)
            .ItemsSource(state, x => x.SimilarProducts)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .ItemTemplate<Product>(product =>
            {
                var btn = new Button()
                    .Background(BackgroundSecondary)
                    .HorizontalAlignment(HorizontalAlignment.Stretch)
                    .Padding(16, 12).CornerRadius(8)
                    .BorderBrush(BorderColor).BorderThickness(1)
                    .Content(
                        new Grid().Cols("*, Auto").Children(
                            new StackPanel().Col(0).Children(
                                new TextBlock().Text(product.Name).FontSize(15)
                                    .FontWeight(FontWeight.SemiBold).Foreground(Brushes.White),
                                new TextBlock()
                                    .Text($"Código: {product.Code} | Laboratorio: {product.Laboratory ?? "N/D"}")
                                    .FontSize(11).Foreground(TextMuted).Margin(0, 4, 0, 0)
                            ),
                            new TextBlock().Text("Editar ➔").FontSize(12).Foreground(AccentBlue).Col(1)
                                .VerticalAlignment(VerticalAlignment.Center)
                        )
                    );
                btn.Click += (_, _) => state.SetupInlineForm(product);
                return btn;
            });

        return new StackPanel().Spacing(12).Margin(0, 16, 0, 0)
            .Children(
                new TextBlock()
                    .Text(state, x => x.SimilarResultsLabel)
                    .FontSize(14).FontWeight(FontWeight.Bold)
                    .Foreground(AccentGreen)
                    .IsVisible(state, x => x.HasSimilarResults),
                new TextBlock()
                    .Text(state, x => x.SimilarResultsLabel)
                    .FontSize(14).FontWeight(FontWeight.Bold)
                    .Foreground(DangerRed)
                    .IsVisible(state, x => !x.HasSimilarResults),
                listBox.IsVisible(state, x => x.HasSimilarResults)
            );
    }

    private Control BuildFullEditorForm(SearchProductState state)
    {
        var form = state.ProductForm;

        var cancelButton = new Button().Content("✕ Cancelar búsqueda")
            .Background(Brushes.Transparent).Foreground(TextMuted)
            .Padding(12, 8).CornerRadius(6);
        cancelButton.Click += (_, _) => state.CancelInlineEdit();

        var saveButton = new Button().Content("💾 Guardar Cambios")
            .Background(AccentBlue).Foreground(Brushes.White)
            .FontWeight(FontWeight.SemiBold).Padding(16, 10).CornerRadius(6);
        saveButton.Click += (_, _) => state.SaveInlineChanges();

        return new Grid().Rows("Auto, Auto, *")
            .Children(
                new Grid().Cols("*, Auto, Auto").Row(0).Margin(0, 0, 0, 12).Children(
                    new StackPanel().VerticalAlignment(VerticalAlignment.Center)
                        .Children(
                            new TextBlock().Text("📝 Editando Producto").FontSize(13)
                                .Foreground(AccentGreen).FontWeight(FontWeight.SemiBold),
                            new TextBlock().Text(form, x => x.Name).FontSize(18)
                                .FontWeight(FontWeight.Bold).Foreground(Brushes.White)
                        ),
                    cancelButton.Col(1).Margin(0, 0, 8, 0),
                    saveButton.Col(2)
                ),
                BuildInactiveProductWarning(form).Row(1).IsVisible(form, x => x.ShowInactiveWarning),
                new ScrollViewer().Row(2)
                    .Content(
                        new StackPanel().Spacing(16)
                            .Children(
                                BuildErrorPanel(form),
                                BuildSection("Tipo de Producto",
                                    new ComboBox()
                                        .HorizontalAlignment(HorizontalAlignment.Stretch)
                                        .Background(BackgroundTertiary).Foreground(Brushes.White)
                                        .BorderBrush(BorderColor).CornerRadius(6)
                                        .ItemsSource(form.ProductTypes)
                                        .SelectedItem(form, x => x.SelectedProductType, BindingMode.TwoWay)
                                        .IsEnabled(!form.ShowInactiveWarning)
                                ),
                                BuildSection("Datos Generales",
                                    new StackPanel().Spacing(12).Children(
                                        new Grid().Cols("*, *").Children(
                                            BuildFormRow("Nombre Comercial *",
                                                    new TextBox().Text(form, x => x.Name, BindingMode.TwoWay)
                                                        .IsEnabled(!form.ShowInactiveWarning))
                                                .Col(0).Margin(0, 0, 6, 0),
                                            BuildFormRow("Código Único *",
                                                    new TextBox().Text(form, x => x.Code, BindingMode.TwoWay)
                                                        .IsEnabled(false))
                                                .Col(1).Margin(6, 0, 0, 0)
                                        ),
                                        new Grid().Cols("*, *, *").Children(
                                            BuildFormRow("Precio (Q) *",
                                                    new TextBox().Text(form, x => x.Price, BindingMode.TwoWay))
                                                .Col(0).Margin(0, 0, 6, 0),
                                            BuildFormRow("Stock Total",
                                                    new TextBox().Text(form, x => x.Stock, BindingMode.TwoWay)
                                                        .IsEnabled(form, x => !x.EnableBatches))
                                                .Col(1).Margin(6, 0, 6, 0),
                                            BuildFormRow("Stock Mínimo *",
                                                    new TextBox().Text(form, x => x.MinimumStock, BindingMode.TwoWay))
                                                .Col(2).Margin(6, 0, 0, 0)
                                        ),
                                        new Grid().Cols("*, *").Children(
                                            BuildFormRow("Código de Barras",
                                                    new TextBox().Text(form, x => x.Barcode, BindingMode.TwoWay))
                                                .Col(0).Margin(0, 0, 6, 0),
                                            BuildFormRow("Subcategoría",
                                                    new TextBox().Text(form, x => x.Subcategory, BindingMode.TwoWay))
                                                .Col(1).Margin(6, 0, 0, 0)
                                        ),
                                        new Grid().Cols("*, *").Children(
                                            BuildFormRow("Laboratorio",
                                                    new TextBox().Text(form, x => x.Laboratory, BindingMode.TwoWay))
                                                .Col(0).Margin(0, 0, 6, 0),
                                            BuildFormRow("Ubicación Física",
                                                    new TextBox().Text(form, x => x.Location, BindingMode.TwoWay))
                                                .Col(1).Margin(6, 0, 0, 0)
                                        ),
                                        new Grid().Cols("*, *").Children(
                                            BuildFormRow("Ingredientes (separados por coma)",
                                                    new TextBox().Text(form, x => x.Ingredients, BindingMode.TwoWay))
                                                .Col(0).Margin(0, 0, 6, 0),
                                            BuildFormRow("Tags (separados por coma)",
                                                    new TextBox().Text(form, x => x.Tags, BindingMode.TwoWay))
                                                .Col(1).Margin(6, 0, 0, 0)
                                        )
                                    )
                                ),
                                BuildSection("Especificaciones",
                                    new Panel().Children(
                                        BuildMedicinePanel(form),
                                        BuildSupplyPanel(form),
                                        BuildSupplementPanel(form),
                                        BuildCosmeticPanel(form)
                                    )
                                ),
                                BuildSection("Lotes (FEFO)",
                                    new StackPanel().Spacing(12).Children(
                                        new CheckBox().Content("Gestionar lotes para este producto")
                                            .Foreground(Brushes.White)
                                            .IsChecked(form, x => x.EnableBatches, BindingMode.TwoWay),
                                        BuildBatchesPanel(form)
                                    )
                                ),
                                BuildSection("Proveedores",
                                    new StackPanel().Spacing(12).Children(
                                        new CheckBox().Content("Asignar proveedores a este producto")
                                            .Foreground(Brushes.White)
                                            .IsChecked(form, x => x.EnableSuppliers, BindingMode.TwoWay),
                                        BuildSuppliersPanel(form)
                                    )
                                )
                            )
                    )
            );
    }

    private Control BuildMedicinePanel(ProductState form) =>
        new StackPanel().Spacing(12)
            .Children(
                BuildFormRow("Principio Activo",
                    new TextBox().Text(form, x => x.ActivePrinciple, BindingMode.TwoWay)),
                BuildFormRow("Concentración (ej. 500mg)",
                    new TextBox().Text(form, x => x.Concentration, BindingMode.TwoWay)),
                BuildFormRow("Presentación (ej. Caja de 20 tabletas)",
                    new TextBox().Text(form, x => x.Presentation, BindingMode.TwoWay)),
                new CheckBox().Content("Requiere Receta Médica").Foreground(Brushes.White)
                    .IsChecked(form, x => x.RequiresPrescription, BindingMode.TwoWay),
                new CheckBox().Content("Medicamento Controlado").Foreground(Brushes.White)
                    .IsChecked(form, x => x.IsControlled, BindingMode.TwoWay)
            )
            .IsVisible(form, x => x.IsMedicine);

    private Control BuildSupplyPanel(ProductState form) =>
        new StackPanel().Spacing(12)
            .Children(
                BuildFormRow("Marca", new TextBox().Text(form, x => x.Brand, BindingMode.TwoWay)),
                BuildFormRow("Tipo de Suministro", new TextBox().Text(form, x => x.Type, BindingMode.TwoWay)),
                BuildFormRow("Tamaño / Dimensión", new TextBox().Text(form, x => x.Size, BindingMode.TwoWay)),
                BuildFormRow("Material", new TextBox().Text(form, x => x.Material, BindingMode.TwoWay)),
                new CheckBox().Content("¿Es Estéril?").Foreground(Brushes.White)
                    .IsChecked(form, x => x.IsSterile, BindingMode.TwoWay)
            )
            .IsVisible(form, x => x.IsSupply);

    private Control BuildSupplementPanel(ProductState form) =>
        new StackPanel().Spacing(12)
            .Children(
                BuildFormRow("Principio Activo",
                    new TextBox().Text(form, x => x.ActivePrinciple, BindingMode.TwoWay)),
                BuildFormRow("Tipo", new TextBox().Text(form, x => x.Type, BindingMode.TwoWay)),
                BuildFormRow("Dosis Recomendada",
                    new TextBox().Text(form, x => x.RecommendedDosage, BindingMode.TwoWay)),
                BuildLabel("Formato"),
                new ComboBox()
                    .HorizontalAlignment(HorizontalAlignment.Stretch)
                    .Background(BackgroundTertiary).Foreground(Brushes.White)
                    .BorderBrush(BorderColor).CornerRadius(6)
                    .ItemsSource(Enum.GetValues(typeof(SupplementFormat)).Cast<SupplementFormat>().ToList())
                    .SelectedItem(form, x => x.SelectedFormat, BindingMode.TwoWay)
            )
            .IsVisible(form, x => x.IsSupplement);

    private Control BuildCosmeticPanel(ProductState form) =>
        new StackPanel().Spacing(12)
            .Children(
                BuildFormRow("Marca", new TextBox().Text(form, x => x.Brand, BindingMode.TwoWay)),
                BuildFormRow("Tipo", new TextBox().Text(form, x => x.Type, BindingMode.TwoWay)),
                BuildFormRow("Presentación", new TextBox().Text(form, x => x.Presentation, BindingMode.TwoWay)),
                new CheckBox().Content("Hipoalergénico").Foreground(Brushes.White)
                    .IsChecked(form, x => x.Hypoallergenic, BindingMode.TwoWay)
            )
            .IsVisible(form, x => x.IsCosmetic);

    private Control BuildBatchesPanel(ProductState form)
    {
        var addBatchButton = new Button().Content("+ Agregar Lote")
            .Background(AccentBlue).Foreground(Brushes.White)
            .HorizontalAlignment(HorizontalAlignment.Right)
            .Padding(12, 6).CornerRadius(6);
        addBatchButton.Click += (_, _) => form.AddBatch();

        var expirationDatePicker = new CalendarDatePicker()
            .SelectedDate(form, x => x.BatchExpirationDate, BindingMode.TwoWay);
        expirationDatePicker.DisplayDateStart = DateTime.Today;

        return new StackPanel().Spacing(12)
            .Children(
                new Grid().Cols("*, *").Children(
                    BuildFormRow("Número de Lote",
                            new TextBox().Text(form, x => x.BatchLotCode, BindingMode.TwoWay))
                        .Col(0).Margin(0, 0, 6, 0),
                    BuildFormRow("Cantidad",
                            new TextBox().Text(form, x => x.BatchQuantity, BindingMode.TwoWay))
                        .Col(1).Margin(6, 0, 0, 0)
                ),
                new Grid().Cols("*, *").Children(
                    BuildFormDatePickerRow("Fecha Fabricación",
                            new CalendarDatePicker().SelectedDate(form, x => x.BatchManufacturingDate,
                                BindingMode.TwoWay))
                        .Col(0).Margin(0, 0, 6, 0),
                    BuildFormDatePickerRow("Fecha Expiración", expirationDatePicker)
                        .Col(1).Margin(6, 0, 0, 0)
                ),
                BuildFormRow("Costo Unitario (Opcional)",
                    new TextBox().Text(form, x => x.BatchUnitCost, BindingMode.TwoWay)),
                addBatchButton,
                new Border().Height(1).Background(BorderColor).Margin(0, 8)
                    .IsVisible(form, x => x.HasBatches),
                BuildLabel("Lotes Registrados").IsVisible(form, x => x.HasBatches),
                new ItemsControl()
                    .ItemsSource(form, x => x.Batches)
                    .ItemTemplate(
                        new FuncDataTemplate<(string LotCode, int Quantity, DateTime MfgDate, DateTime ExpDate,
                            decimal UnitCost)>((batch, _) =>
                        {
                            var removeBtn = new Button().Content("✕")
                                .Background(DangerRed).Foreground(Brushes.White)
                                .Padding(4).FontSize(12).CornerRadius(4);
                            removeBtn.Click += (_, _) =>
                            {
                                var idx = form.Batches.IndexOf(batch);
                                if (idx >= 0) form.RemoveBatch(idx);
                            };

                            return new Border()
                                .Background(BackgroundTertiary).BorderBrush(BorderColor)
                                .BorderThickness(1).CornerRadius(6).Padding(12, 8).Margin(0, 0, 0, 8)
                                .Child(
                                    new Grid().Cols("*, Auto").Children(
                                        new StackPanel().Spacing(4).Children(
                                            new TextBlock().Text($"Lote: {batch.LotCode}").FontSize(12)
                                                .FontWeight(FontWeight.SemiBold).Foreground(Brushes.White),
                                            new TextBlock()
                                                .Text(
                                                    $"Stock: {batch.Quantity} u  |  Vence: {batch.ExpDate:yyyy-MM-dd}  |  Costo: Q{batch.UnitCost:F2}")
                                                .FontSize(11).Foreground(TextMuted)
                                        ),
                                        removeBtn.Col(1).Margin(12, 0, 0, 0)
                                    )
                                );
                        }))
                    .IsVisible(form, x => x.HasBatches)
            )
            .IsVisible(form, x => x.EnableBatches);
    }

    private Control BuildSuppliersPanel(ProductState form) =>
        new StackPanel().Spacing(12)
            .Children(
                BuildLabel("Proveedores Disponibles"),
                new ItemsControl()
                    .ItemsSource(form, x => x.AvailableSuppliers)
                    .ItemTemplate(
                        new FuncDataTemplate<Supplier>((supplier, _) =>
                        {
                            var initiallyChecked = form.SelectedSuppliers.Any(s => s.Code == supplier.Code);
                            var cb = new CheckBox().Content(supplier.Name).Foreground(Brushes.White)
                                .IsChecked(initiallyChecked).Margin(0, 2);
                            cb.Click += (_, _) => form.ToggleSupplier(supplier);
                            return cb;
                        }))
                    .IsVisible(form, x => x.AnySupplierAvailable),
                new TextBlock()
                    .Text("⚠️ No hay proveedores activos registrados en el sistema.")
                    .Foreground(TextMuted).FontSize(12).FontStyle(FontStyle.Italic)
                    .IsVisible(form, x => x.NoSuppliersAvailable),
                new TextBlock().Text(form, x => x.SuppliersInfo)
                    .Foreground(AccentGreen).FontSize(12)
                    .IsVisible(form, x => x.HasSuppliers)
            )
            .IsVisible(form, x => x.EnableSuppliers);

    private Control BuildSection(string title, Control content) =>
        new Border()
            .Background(BackgroundSecondary)
            .BorderBrush(BorderColor).BorderThickness(1)
            .CornerRadius(10).Padding(16).Margin(0, 0, 0, 4)
            .Child(
                new StackPanel().Spacing(12).Children(
                    new TextBlock().Text(title).FontSize(13).FontWeight(FontWeight.Bold)
                        .Foreground(TextMuted),
                    new Border().Height(1).Background(BorderColor),
                    content
                )
            );

    private Control BuildErrorPanel(ProductState form) =>
        new Border()
            .Background(ErrorBackground).BorderBrush(ErrorBorder).BorderThickness(1)
            .CornerRadius(6).Padding(12, 8)
            .IsVisible(form, x => x.HasErrorMessage)
            .Child(
                new StackPanel().Spacing(6).Orientation(Orientation.Horizontal).Children(
                    new TextBlock().Text("⚠").FontSize(16).Foreground(ErrorText)
                        .VerticalAlignment(VerticalAlignment.Center),
                    new TextBlock().Text(form, x => x.ErrorMessage).Foreground(ErrorText)
                        .FontSize(13).TextWrapping(TextWrapping.Wrap)
                        .VerticalAlignment(VerticalAlignment.Center)
                )
            );

    private StackPanel BuildFormRow(string label, TextBox input)
    {
        input.Background(BackgroundTertiary).Foreground(Brushes.White)
            .BorderBrush(BorderColor).CornerRadius(6).Padding(10, 8);
        return new StackPanel().Spacing(6).Children(BuildLabel(label), input);
    }

    private StackPanel BuildFormDatePickerRow(string label, CalendarDatePicker picker)
    {
        picker.Background(BackgroundTertiary).Foreground(Brushes.White)
            .BorderBrush(BorderColor).CornerRadius(6).Padding(10, 6)
            .HorizontalAlignment(HorizontalAlignment.Stretch);
        return new StackPanel().Spacing(6).Children(BuildLabel(label), picker);
    }

    private static TextBlock BuildLabel(string text) =>
        new TextBlock().Text(text).FontSize(12).FontWeight(FontWeight.SemiBold).Foreground(TextMuted);

    private Control BuildEmptyState() =>
        new StackPanel().VerticalAlignment(VerticalAlignment.Center)
            .HorizontalAlignment(HorizontalAlignment.Center).Spacing(10).Children(
                new TextBlock().Text("🔍").FontSize(48).HorizontalAlignment(HorizontalAlignment.Center),
                new TextBlock().Text("Escriba un término para inspeccionar y editar...")
                    .FontSize(14).Foreground(TextMuted)
                    .HorizontalAlignment(HorizontalAlignment.Center)
            );

    private Control BuildInactiveProductWarning(ProductState state)
    {
        return new Border()
            .Background(SolidColorBrush.Parse("#7F1D1D"))
            .BorderBrush(SolidColorBrush.Parse("#EF4444"))
            .BorderThickness(1)
            .CornerRadius(8)
            .Padding(16, 12)
            .Margin(0, 0, 0, 16)
            .Child(
                new Grid().Cols("Auto, *, Auto")
                    .Children(
                        new Border()
                            .Background(SolidColorBrush.Parse("#DC2626"))
                            .CornerRadius(20)
                            .Width(36)
                            .Height(36)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Col(0)
                            .Child(
                                new TextBlock()
                                    .Text("⚠️")
                                    .FontSize(18)
                                    .HorizontalAlignment(HorizontalAlignment.Center)
                                    .VerticalAlignment(VerticalAlignment.Center)
                            ),
                        new StackPanel()
                            .Col(1)
                            .Margin(12, 0)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Children(
                                new TextBlock()
                                    .Text("Producto Inactivo")
                                    .FontSize(14)
                                    .FontWeight(FontWeight.Bold)
                                    .Foreground(SolidColorBrush.Parse("#FCA5A5")),
                                new TextBlock()
                                    .Text(state, x => x.InactiveProductWarning)
                                    .Foreground(SolidColorBrush.Parse("#FED7AA"))
                                    .FontSize(12)
                                    .TextWrapping(TextWrapping.Wrap)
                            ),
                        new StackPanel()
                            .Orientation(Orientation.Horizontal)
                            .Spacing(8)
                            .Col(2)
                            .VerticalAlignment(VerticalAlignment.Center)
                            .Children(
                                new Button()
                                    .Content("Cancelar")
                                    .Background(Brushes.Transparent)
                                    .Foreground(SolidColorBrush.Parse("#FCA5A5"))
                                    .BorderBrush(SolidColorBrush.Parse("#DC2626"))
                                    .BorderThickness(1)
                                    .CornerRadius(6)
                                    .Padding(12, 6)
                                    .Cursor(new Cursor(StandardCursorType.Hand))
                                    .With(btn => btn.Click += (_, _) => state.CancelInactiveEdit()),
                                new Button()
                                    .Content("🔄 Reactivar Producto")
                                    .Background(SolidColorBrush.Parse("#10B981"))
                                    .Foreground(Brushes.White)
                                    .FontWeight(FontWeight.SemiBold)
                                    .CornerRadius(6)
                                    .Padding(12, 6)
                                    .Cursor(new Cursor(StandardCursorType.Hand))
                                    .With(btn => btn.Click += (_, _) => state.ReactivateProduct())
                            )
                    )
            );
    }
}