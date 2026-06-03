using Farmacontrol.Desktop.States;

namespace Farmacontrol.Desktop.Components;

public static class SupplierModal
{
    private static readonly SolidColorBrush BackgroundOverlay = SolidColorBrush.Parse("#80000000");
    private static readonly SolidColorBrush BackgroundCard = SolidColorBrush.Parse("#1F2937");
    private static readonly SolidColorBrush BackgroundInput = SolidColorBrush.Parse("#374151");
    private static readonly SolidColorBrush BorderColor = SolidColorBrush.Parse("#4B5563");
    private static readonly SolidColorBrush AccentBlue = SolidColorBrush.Parse("#2563EB");
    private static readonly SolidColorBrush TextMuted = SolidColorBrush.Parse("#9CA3AF");
    private static readonly SolidColorBrush ErrorBackground = SolidColorBrush.Parse("#7F1D1D");
    private static readonly SolidColorBrush ErrorBorder = SolidColorBrush.Parse("#DC2626");
    private static readonly SolidColorBrush ErrorText = SolidColorBrush.Parse("#FCA5A5");

    public static Control Build(SupplierState state, Action onCancel, Action onSave)
    {
        var closeButton = new Button()
            .Content("✕")
            .Background(Brushes.Transparent)
            .Foreground(TextMuted)
            .FontSize(18)
            .Padding(4);

        closeButton.Click += (_, _) => onCancel();

        return new Grid()
            .Background(BackgroundOverlay)
            .Children(
                new Border()
                    .Background(BackgroundCard)
                    .CornerRadius(12)
                    .Width(500)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Padding(24)
                    .Child(
                        new Grid().Rows("Auto, Auto, *, Auto")
                            .Children(
                                new Grid().Row(0).Cols("*, Auto").Margin(0, 0, 0, 16)
                                    .Children(
                                        new TextBlock()
                                            .Text(state,
                                                x => x.ModalTitle)
                                            .FontSize(20)
                                            .FontWeight(FontWeight.Bold)
                                            .Foreground(Brushes.White),
                                        closeButton.Col(1)
                                    ),
                                BuildErrorSection(state).Row(1),
                                new ScrollViewer().Row(2).MaxHeight(450)
                                    .Content(
                                        new StackPanel().Spacing(14).Margin(0, 4, 10, 16)
                                            .Children(
                                                BuildField("Código único *",
                                                    new TextBox().Text(state, x => x.Code, BindingMode.TwoWay)
                                                        .IsEnabled(state, x => !x.IsEditing)),
                                                BuildField("Nombre Comercial / Laboratorio *",
                                                    new TextBox().Text(state, x => x.Name, BindingMode.TwoWay)),
                                                BuildField("NIT / ID Fiscal",
                                                    new TextBox().Text(state, x => x.TaxId, BindingMode.TwoWay)),
                                                new Grid().Cols("*, *")
                                                    .Children(
                                                        BuildField("Teléfono de Contacto *",
                                                            new TextBox().Text(state, x => x.PhoneNumber,
                                                                BindingMode.TwoWay)).Col(0).Margin(0, 0, 6, 0),
                                                        BuildField("Correo Electrónico *",
                                                                new TextBox().Text(state, x => x.Email,
                                                                    BindingMode.TwoWay))
                                                            .Col(1).Margin(6, 0, 0, 0)
                                                    ),
                                                BuildField("Nombre del Agente de Ventas / Contacto",
                                                    new TextBox().Text(state, x => x.ContactName, BindingMode.TwoWay)),
                                                new Grid().Cols("*, *")
                                                    .Children(
                                                        BuildField("Tiempo de Entrega (Días) *",
                                                            new TextBox().Text(state, x => x.LeadTimeDays,
                                                                BindingMode.TwoWay)).Col(0).Margin(0, 0, 6, 0),
                                                        BuildField("Términos de Pago (ej: Crédito 30 días)",
                                                            new TextBox().Text(state, x => x.PaymentTerms,
                                                                BindingMode.TwoWay)).Col(1).Margin(6, 0, 0, 0)
                                                    ),
                                                BuildField("Dirección Física / Fiscal",
                                                    new TextBox().Text(state, x => x.Address, BindingMode.TwoWay)
                                                        .Height(60).AcceptsReturn(true)),
                                                new CheckBox()
                                                    .Content("Proveedor Activo")
                                                    .Foreground(Brushes.White)
                                                    .IsChecked(state, x => x.IsActive, BindingMode.TwoWay)
                                            )
                                    ),
                                BuildActionButtons(onCancel, onSave).Row(3).Margin(0, 12, 0, 0)
                            )
                    )
            );
    }

    private static Control BuildField(string labelText, TextBox inputControl)
    {
        inputControl
            .Background(BackgroundInput)
            .Foreground(Brushes.White)
            .BorderBrush(BorderColor)
            .BorderThickness(1)
            .CornerRadius(6)
            .Padding(10, 8);

        return new StackPanel().Spacing(6)
            .Children(
                new TextBlock().Text(labelText).FontSize(12).FontWeight(FontWeight.SemiBold).Foreground(TextMuted),
                inputControl
            );
    }

    private static Control BuildErrorSection(SupplierState state) =>
        new Border()
            .Background(ErrorBackground)
            .BorderBrush(ErrorBorder)
            .BorderThickness(1)
            .CornerRadius(6)
            .Padding(12, 10)
            .Margin(0, 0, 0, 14)
            .IsVisible(state, x => x.HasErrorMessage)
            .Child(
                new TextBlock()
                    .Text(state, x => x.ErrorMessage)
                    .Foreground(ErrorText)
                    .FontSize(12)
                    .TextWrapping(TextWrapping.Wrap)
            );

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
            .Content("Guardar Proveedor")
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