using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Styling;
using Farmacontrol.Core.Model;
using Farmacontrol.Core.Model.ProductEntity;
using Farmacontrol.Desktop.States;

namespace Farmacontrol.Desktop.Components;

public static class AddProductModal
{
    private static readonly SolidColorBrush BackgroundTertiary = SolidColorBrush.Parse("#374151");
    private static readonly SolidColorBrush BackgroundHover = SolidColorBrush.Parse("#4B5563");
    private static readonly SolidColorBrush BackgroundOverlay = SolidColorBrush.Parse("#80000000");
    private static readonly SolidColorBrush BackgroundCard = SolidColorBrush.Parse("#1F2937");
    private static readonly SolidColorBrush BackgroundInput = SolidColorBrush.Parse("#374151");
    private static readonly SolidColorBrush BorderColor = SolidColorBrush.Parse("#4B5563");
    private static readonly SolidColorBrush AccentBlue = SolidColorBrush.Parse("#2563EB");
    private static readonly SolidColorBrush TextMuted = SolidColorBrush.Parse("#9CA3AF");
    private static readonly SolidColorBrush ErrorBackground = SolidColorBrush.Parse("#7F1D1D");
    private static readonly SolidColorBrush ErrorBorder = SolidColorBrush.Parse("#DC2626");
    private static readonly SolidColorBrush ErrorText = SolidColorBrush.Parse("#FCA5A5");

    public static Control Build(AddProductState state, Action onCancel, Action onSave)
    {
        var closeButton = new Button()
            .Content("✕")
            .Background(Brushes.Transparent)
            .Foreground(TextMuted)
            .FontSize(16)
            .Padding(4)
            .Col(1);

        closeButton.Click += (_, _) => onCancel();

        return new Grid()
            .Background(BackgroundOverlay)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .VerticalAlignment(VerticalAlignment.Stretch)
            .Children(
                new Border()
                    .Width(500)
                    .MaxHeight(650)
                    .Background(BackgroundCard)
                    .BorderBrush(BorderColor)
                    .BorderThickness(1)
                    .CornerRadius(12)
                    .Padding(24)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .VerticalAlignment(VerticalAlignment.Center)
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
                        new Grid().Rows("Auto, Auto, *, Auto")
                            .Children(
                                new Grid().Cols("*, Auto").Row(0)
                                    .Children(
                                        new TextBlock()
                                            .Text(state, x => x.Title)
                                            .FontSize(20)
                                            .FontWeight(FontWeight.Bold)
                                            .Foreground(Brushes.White)
                                            .VerticalAlignment(VerticalAlignment.Center),
                                        closeButton
                                    ),
                                BuildErrorPanel(state).Row(1),
                                new ScrollViewer().Row(2).Margin(0, 16)
                                    .Content(
                                        new StackPanel().Spacing(12)
                                            .Children(
                                                BuildLabel("Tipo de Producto"),
                                                new ComboBox()
                                                    .HorizontalAlignment(HorizontalAlignment.Stretch)
                                                    .Background(BackgroundInput)
                                                    .Foreground(Brushes.White)
                                                    .BorderBrush(BorderColor)
                                                    .CornerRadius(6)
                                                    .ItemsSource(state.ProductTypes)
                                                    .SelectedItem(state, x => x.SelectedProductType,
                                                        BindingMode.TwoWay),
                                                new Border().Height(1).Background(BorderColor).Margin(0, 4),
                                                BuildFormRow("Nombre del Producto *",
                                                    new TextBox().Text(state, x => x.Name, BindingMode.TwoWay)),
                                                BuildFormRow("Código Único *",
                                                    new TextBox().Text(state, x => x.Code, BindingMode.TwoWay)),
                                                new Grid().Cols("*, *")
                                                    .Children(
                                                        BuildFormRow("Precio (Q) *",
                                                                new TextBox().Text(state, x => x.Price,
                                                                    BindingMode.TwoWay))
                                                            .Col(0).Margin(0, 0, 6, 0),
                                                        BuildFormRow("Stock Inicial *",
                                                                new TextBox().Text(state, x => x.Stock,
                                                                    BindingMode.TwoWay))
                                                            .Col(1).Margin(6, 0, 0, 0)
                                                    ),
                                                new Grid().Cols("*, *")
                                                    .Children(
                                                        BuildFormRow("Stock Mínimo *",
                                                            new TextBox().Text(state, x => x.MinimumStock,
                                                                BindingMode.TwoWay)).Col(0).Margin(0, 0, 6, 0),
                                                        BuildFormRow("Código de Barras",
                                                            new TextBox().Text(state, x => x.Barcode,
                                                                BindingMode.TwoWay)).Col(1).Margin(6, 0, 0, 0)
                                                    ),
                                                new Grid().Cols("*, *")
                                                    .Children(
                                                        BuildFormRow("Ubicación",
                                                            new TextBox().Text(state, x => x.Location,
                                                                BindingMode.TwoWay)).Col(0).Margin(0, 0, 6, 0),
                                                        BuildFormRow("Laboratorio",
                                                            new TextBox().Text(state, x => x.Laboratory,
                                                                BindingMode.TwoWay)).Col(1).Margin(6, 0, 0, 0)
                                                    ),
                                                BuildFormRow("Subcategoría",
                                                    new TextBox().Text(state, x => x.Subcategory, BindingMode.TwoWay)),
                                                new Grid().Cols("*, *")
                                                    .Children(
                                                        BuildFormRow("Ingredientes (separados por coma)",
                                                                new TextBox().Text(state, x => x.Ingredients,
                                                                    BindingMode.TwoWay))
                                                            .Col(0).Margin(0, 0, 6, 0),
                                                        BuildFormRow("Tags (separados por coma)",
                                                                new TextBox().Text(state, x => x.Tags,
                                                                    BindingMode.TwoWay))
                                                            .Col(1).Margin(6, 0, 0, 0)
                                                    ),
                                                new Border().Height(1).Background(BorderColor).Margin(0, 4),
                                                BuildMedicinePanel(state),
                                                BuildSupplyPanel(state),
                                                BuildSupplementPanel(state),
                                                BuildCosmeticPanel(state),
                                                new Border().Height(1).Background(BorderColor).Margin(0, 4),
                                                new StackPanel().Spacing(12)
                                                    .Children(
                                                        new CheckBox().Content("Añadir Lotes").Foreground(Brushes.White)
                                                            .IsChecked(state, x => x.EnableBatches, BindingMode.TwoWay),
                                                        BuildBatchesPanel(state),
                                                        new CheckBox().Content("Asignar Proveedores")
                                                            .Foreground(Brushes.White)
                                                            .IsChecked(state, x => x.EnableSuppliers,
                                                                BindingMode.TwoWay),
                                                        BuildSuppliersPanel(state)
                                                    )
                                            )
                                    ),
                                BuildActionButtons(onCancel, onSave).Row(3)
                            )
                    )
            );
    }

    private static Control BuildFormRow(string labelText, TextBox textBox)
    {
        textBox.Background(BackgroundInput)
            .Foreground(Brushes.White)
            .BorderBrush(BorderColor)
            .CornerRadius(6)
            .Padding(10, 6);

        return new StackPanel().Spacing(4)
            .Children(
                BuildLabel(labelText),
                textBox
            );
    }

    private static Control BuildFormDatePickerRow(string labelText, CalendarDatePicker datePicker)
    {
        datePicker.Background(BackgroundInput)
            .Foreground(Brushes.White)
            .BorderBrush(BorderColor)
            .CornerRadius(6)
            .Padding(10, 6)
            .HorizontalAlignment(HorizontalAlignment.Stretch);

        return new StackPanel().Spacing(4)
            .Children(
                BuildLabel(labelText),
                datePicker
            );
    }

    private static TextBlock BuildLabel(string text) =>
        new TextBlock().Text(text).FontSize(12).FontWeight(FontWeight.SemiBold).Foreground(TextMuted);

    private static Control BuildErrorPanel(AddProductState state) =>
        new Border()
            .Background(ErrorBackground)
            .BorderBrush(ErrorBorder)
            .BorderThickness(1)
            .CornerRadius(6)
            .Padding(12, 8)
            .Margin(0, 0, 0, 12)
            .IsVisible(state, x => x.HasErrorMessage)
            .Child(
                new StackPanel().Spacing(6).Orientation(Orientation.Horizontal)
                    .Children(
                        new TextBlock()
                            .Text("⚠")
                            .FontSize(16)
                            .Foreground(ErrorText)
                            .VerticalAlignment(VerticalAlignment.Center),
                        new TextBlock()
                            .Text(state, x => x.ErrorMessage)
                            .Foreground(ErrorText)
                            .FontSize(13)
                            .TextWrapping(TextWrapping.Wrap)
                            .VerticalAlignment(VerticalAlignment.Center)
                    )
            );

    private static Control BuildMedicinePanel(AddProductState state) =>
        new StackPanel().Spacing(12)
            .Children(
                BuildFormRow("Principio Activo", new TextBox().Text(state, x => x.ActivePrinciple, BindingMode.TwoWay)),
                BuildFormRow("Concentración (ej. 500mg)",
                    new TextBox().Text(state, x => x.Concentration, BindingMode.TwoWay)),
                BuildFormRow("Presentación (ej. Caja de 20 tabletas)",
                    new TextBox().Text(state, x => x.Presentation, BindingMode.TwoWay)),
                new CheckBox().Content("Requiere Receta Médica").Foreground(Brushes.White)
                    .IsChecked(state, x => x.RequiresPrescription, BindingMode.TwoWay),
                new CheckBox().Content("Medicamento Controlado").Foreground(Brushes.White)
                    .IsChecked(state, x => x.IsControlled, BindingMode.TwoWay)
            )
            .IsVisible(state, x => x.IsMedicine);

    private static Control BuildSupplyPanel(AddProductState state) =>
        new StackPanel().Spacing(12)
            .Children(
                BuildFormRow("Marca", new TextBox().Text(state, x => x.Brand, BindingMode.TwoWay)),
                BuildFormRow("Tipo de Suministro", new TextBox().Text(state, x => x.Type, BindingMode.TwoWay)),
                BuildFormRow("Tamaño / Dimensión", new TextBox().Text(state, x => x.Size, BindingMode.TwoWay)),
                BuildFormRow("Material", new TextBox().Text(state, x => x.Material, BindingMode.TwoWay)),
                new CheckBox().Content("¿Es Estéril?").Foreground(Brushes.White)
                    .IsChecked(state, x => x.IsSterile, BindingMode.TwoWay)
            )
            .IsVisible(state, x => x.IsSupply);

    private static Control BuildSupplementPanel(AddProductState state) =>
        new StackPanel().Spacing(12)
            .Children(
                BuildFormRow("Principio Activo", new TextBox().Text(state, x => x.ActivePrinciple, BindingMode.TwoWay)),
                BuildFormRow("Tipo", new TextBox().Text(state, x => x.Type, BindingMode.TwoWay)),
                BuildFormRow("Dosis Recomendada",
                    new TextBox().Text(state, x => x.RecommendedDosage, BindingMode.TwoWay)),
                BuildLabel("Formato"),
                new ComboBox()
                    .HorizontalAlignment(HorizontalAlignment.Stretch)
                    .Background(BackgroundInput)
                    .Foreground(Brushes.White)
                    .BorderBrush(BorderColor)
                    .CornerRadius(6)
                    .ItemsSource(Enum.GetValues(typeof(SupplementFormat)).Cast<SupplementFormat>().ToList())
                    .SelectedItem(state, x => x.SelectedFormat, BindingMode.TwoWay)
            )
            .IsVisible(state, x => x.IsSupplement);

    private static Control BuildCosmeticPanel(AddProductState state) =>
        new StackPanel().Spacing(12)
            .Children(
                BuildFormRow("Marca", new TextBox().Text(state, x => x.Brand, BindingMode.TwoWay)),
                BuildFormRow("Tipo", new TextBox().Text(state, x => x.Type, BindingMode.TwoWay)),
                BuildFormRow("Presentación", new TextBox().Text(state, x => x.Presentation, BindingMode.TwoWay)),
                new CheckBox().Content("Hipoalergénico").Foreground(Brushes.White)
                    .IsChecked(state, x => x.Hypoallergenic, BindingMode.TwoWay)
            )
            .IsVisible(state, x => x.IsCosmetic);

    private static Control BuildBatchesPanel(AddProductState state)
    {
        var addBatchButton = new Button()
            .Content("Agregar Lote")
            .Background(AccentBlue)
            .Foreground(Brushes.White)
            .HorizontalAlignment(HorizontalAlignment.Right)
            .Padding(12, 6)
            .CornerRadius(6);

        addBatchButton.Click += (_, _) => state.AddBatch();

        var expirationDatePicker = new CalendarDatePicker()
            .SelectedDate(state, x => x.BatchExpirationDate, BindingMode.TwoWay);
        expirationDatePicker.DisplayDateStart = DateTime.Today;

        return new StackPanel().Spacing(12)
            .Children(
                new Grid().Cols("*, *")
                    .Children(
                        BuildFormRow("Número de Lote",
                                new TextBox().Text(state, x => x.BatchLotCode, BindingMode.TwoWay)).Col(0)
                            .Margin(0, 0, 6, 0),
                        BuildFormRow("Cantidad", new TextBox().Text(state, x => x.BatchQuantity, BindingMode.TwoWay))
                            .Col(1).Margin(6, 0, 0, 0)
                    ),
                new Grid().Cols("*, *")
                    .Children(
                        BuildFormDatePickerRow("Fecha Fabricación",
                            new CalendarDatePicker().SelectedDate(state, x => x.BatchManufacturingDate,
                                BindingMode.TwoWay)).Col(0).Margin(0, 0, 6, 0),
                        BuildFormDatePickerRow("Fecha Expiración", expirationDatePicker).Col(1).Margin(6, 0, 0, 0)
                    ),
                BuildFormRow("Costo Unitario (Opcional)",
                    new TextBox().Text(state, x => x.BatchUnitCost, BindingMode.TwoWay)),
                addBatchButton,
                new Border().Height(1).Background(BorderColor).Margin(0, 8).IsVisible(state, x => x.HasBatches),
                BuildLabel("Lotes Agregados").IsVisible(state, x => x.HasBatches),
                new ItemsControl()
                    .ItemsSource(state, x => x.Batches)
                    .ItemTemplate(
                        new FuncDataTemplate<(string LotCode, int Quantity, DateTime MfgDate, DateTime ExpDate, decimal
                            UnitCost)>((batch, _) =>
                        {
                            var removeButton = new Button()
                                .Content("✕")
                                .Background(SolidColorBrush.Parse("#EF4444"))
                                .Foreground(Brushes.White)
                                .Padding(4)
                                .FontSize(12)
                                .CornerRadius(4);

                            removeButton.Click += (_, _) =>
                            {
                                var index = state.Batches.IndexOf(batch);
                                if (index >= 0) state.RemoveBatch(index);
                            };

                            return new Border()
                                .Background(BackgroundInput)
                                .BorderBrush(BorderColor)
                                .BorderThickness(1)
                                .CornerRadius(6)
                                .Padding(12, 8)
                                .Margin(0, 0, 0, 8)
                                .Child(
                                    new Grid().Cols("*, Auto")
                                        .Children(
                                            new StackPanel().Spacing(4)
                                                .Children(
                                                    new TextBlock()
                                                        .Text($"Lote: {batch.LotCode}")
                                                        .FontSize(12)
                                                        .FontWeight(FontWeight.SemiBold)
                                                        .Foreground(Brushes.White),
                                                    new TextBlock()
                                                        .Text(
                                                            $"Stock: {batch.Quantity} unidades | Vencimiento: {batch.ExpDate:yyyy-MM-dd}")
                                                        .FontSize(11)
                                                        .Foreground(TextMuted)
                                                ),
                                            removeButton.Col(1).Margin(12, 0, 0, 0)
                                        )
                                );
                        }))
                    .IsVisible(state, x => x.HasBatches)
            )
            .IsVisible(state, x => x.EnableBatches);
    }

    private static Control BuildSuppliersPanel(AddProductState state) =>
        new StackPanel().Spacing(12)
            .Children(
                BuildLabel("Proveedores Disponibles"),
                new ItemsControl()
                    .ItemsSource(state, x => x.AvailableSuppliers)
                    .ItemTemplate(
                        new FuncDataTemplate<Supplier>((supplier,
                            _) =>
                        {
                            var cb = new CheckBox()
                                .Content(supplier.Name)
                                .Foreground(Brushes.White)
                                .Margin(0, 2);
                            cb.IsCheckedChanged += (_, _) => state.ToggleSupplier(supplier);
                            return cb;
                        }))
                    .IsVisible(state, x => x.HasSuppliers),
                new Button()
                    .Content("➕ Agregar Nuevo Proveedor")
                    .Background(AccentBlue)
                    .Foreground(Brushes.White)
                    .HorizontalAlignment(HorizontalAlignment.Stretch)
                    .Padding(12, 6)
                    .CornerRadius(6)
                    .IsVisible(state, x => !x.HasSuppliers),
                new TextBlock()
                    .Text(state, x => x.SuppliersInfo)
                    .Foreground(Brushes.White)
                    .FontSize(12)
                    .IsVisible(state, x => x.HasSuppliers)
            )
            .IsVisible(state, x => x.EnableSuppliers);

    private static Control BuildActionButtons(Action onCancel, Action onSave)
    {
        var cancelButton = new Button()
            .Content("Cancelar")
            .Background(Brushes.Transparent)
            .Foreground(Brushes.White)
            .Padding(16, 10)
            .Col(1)
            .Margin(0, 0, 8, 0);

        var saveButton = new Button()
            .Content("Guardar Producto")
            .Background(AccentBlue)
            .Foreground(Brushes.White)
            .FontWeight(FontWeight.SemiBold)
            .Padding(16, 10)
            .CornerRadius(6)
            .Col(2);

        cancelButton.Click += (_, _) => onCancel();
        saveButton.Click += (_, _) => onSave();

        return new Grid().Cols("*, Auto, Auto")
            .Children(cancelButton, saveButton);
    }
}