using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Input;
using Farmacontrol.Core.Model;
using Farmacontrol.Desktop.States;
using Microsoft.Extensions.DependencyInjection;

namespace Farmacontrol.Desktop.Views.Alerts;

public class AlertsView() : ViewBase<AlertsState>(Program.ServiceProvider.GetRequiredService<AlertsState>())
{
    private static readonly ISolidColorBrush BackgroundPrimary = SolidColorBrush.Parse("#0F172A");
    private static readonly ISolidColorBrush BackgroundSecondary = SolidColorBrush.Parse("#1E293B");
    private static readonly ISolidColorBrush TextMuted = SolidColorBrush.Parse("#94A3B8");
    private static readonly ISolidColorBrush TextSubtle = SolidColorBrush.Parse("#64748B");
    private static readonly ISolidColorBrush AccentBlue = SolidColorBrush.Parse("#3B82F6");
    private static readonly ISolidColorBrush ErrorRed = SolidColorBrush.Parse("#EF4444");
    private static readonly ISolidColorBrush WarningYellow = SolidColorBrush.Parse("#F59E0B");
    private static readonly ISolidColorBrush WarningYellowBg = SolidColorBrush.Parse("#45301A");
    private static readonly ISolidColorBrush ErrorRedBg = SolidColorBrush.Parse("#451A1A");
    private static readonly ISolidColorBrush InfoBlueBg = SolidColorBrush.Parse("#1A3045");

    protected override object Build(AlertsState state)
    {
        return new Border()
            .Background(BackgroundPrimary)
            .CornerRadius(12)
            .Padding(20)
            .Child(
                new Grid()
                    .Rows("Auto, *")
                    .Children(
                        BuildHeaderAndControls(state).Row(0),
                        BuildAlertsList(state).Row(1)
                    ));
    }

    private Control BuildHeaderAndControls(AlertsState state)
    {
        var scanButton = new Button()
            .Content("🔍 Escanear Inventario")
            .Background(AccentBlue)
            .Foreground(Brushes.White)
            .FontWeight(FontWeight.Bold)
            .Padding(16, 10)
            .CornerRadius(8)
            .Cursor(new Cursor(StandardCursorType.Hand))
            .Command(state.ScanInventoryCommand);

        scanButton.Bind(IsEnabledProperty, new Binding
        {
            Source = state,
            Path = nameof(state.IsScanning),
            Converter = new FuncValueConverter<bool, bool>(isScanning => !isScanning)
        });

        return new StackPanel()
            .Spacing(24)
            .Margin(0, 0, 0, 24)
            .Children(
                new Grid().Cols("*, Auto")
                    .Children(
                        new StackPanel().VerticalAlignment(VerticalAlignment.Center)
                            .Children(
                                new TextBlock()
                                    .Text("Panel de Alertas")
                                    .FontSize(26)
                                    .FontWeight(FontWeight.Bold)
                                    .Foreground(Brushes.White),
                                new TextBlock()
                                    .Text("Alertas de stock bajo y productos vencidos generadas hoy.")
                                    .FontSize(13)
                                    .Foreground(TextMuted)
                                    .Margin(0, 4, 0, 0)
                            ),
                        scanButton.Col(1).VerticalAlignment(VerticalAlignment.Center)
                    ),
                new Border()
                    .Background(BackgroundSecondary)
                    .CornerRadius(8)
                    .Padding(16)
                    .Child(
                        new Grid().Cols("Auto, *")
                            .Children(
                                new TextBlock().Text("ℹ️").FontSize(18).Col(0).Margin(0, 0, 12, 0)
                                    .VerticalAlignment(VerticalAlignment.Center),
                                new TextBlock().Text(state, s => s.ScanStatusMessage).Foreground(TextSubtle).Col(1)
                                    .VerticalAlignment(VerticalAlignment.Center).TextWrapping(TextWrapping.Wrap)
                            )
                    )
            );
    }

    private Control BuildAlertsList(AlertsState state)
    {
        var listBox = new ListBox()
            .ItemsSource(state.TodayAlerts)
            .Background(Brushes.Transparent)
            .BorderBrush(Brushes.Transparent)
            .ItemTemplate(new FuncDataTemplate<Alert>((alert, _) =>
            {
                var alertColor = GetAlertColor(alert.Type);
                var alertBgColor = GetAlertBgColor(alert.Type);
                var alertIcon = GetAlertIcon(alert.Type);

                return new Border()
                    .Background(BackgroundSecondary)
                    .CornerRadius(12)
                    .Padding(16)
                    .Margin(0, 0, 0, 12)
                    .Child(
                        new Grid().Cols("Auto, *, Auto")
                            .Children(
                                new Border()
                                    .Background(alertBgColor)
                                    .CornerRadius(8)
                                    .Padding(12)
                                    .Col(0)
                                    .Margin(0, 0, 16, 0)
                                    .VerticalAlignment(VerticalAlignment.Center)
                                    .Child(
                                        new TextBlock().Text(alertIcon).FontSize(24)
                                    ),
                                new StackPanel().VerticalAlignment(VerticalAlignment.Center).Col(1)
                                    .Children(
                                        new TextBlock().Text($"{alert.ProductName}").FontSize(16)
                                            .FontWeight(FontWeight.Bold).Foreground(Brushes.White),
                                        new TextBlock().Text($"Código: {alert.ProductCode}").FontSize(12)
                                            .Foreground(TextSubtle).Margin(0, 4, 0, 8),
                                        new TextBlock().Text(alert.Description).Foreground(TextMuted)
                                            .TextWrapping(TextWrapping.Wrap)
                                    ),
                                new StackPanel().VerticalAlignment(VerticalAlignment.Top)
                                    .HorizontalAlignment(HorizontalAlignment.Right).Col(2)
                                    .Children(
                                        new Border().Background(alertBgColor).CornerRadius(16).Padding(10, 4)
                                            .Child(
                                                new TextBlock().Text(alert.Type).Foreground(alertColor).FontSize(12)
                                                    .FontWeight(FontWeight.Bold)
                                            ),
                                        new TextBlock().Text(alert.Date.ToString("HH:mm")).Foreground(TextSubtle)
                                            .FontSize(11).HorizontalAlignment(HorizontalAlignment.Right)
                                            .Margin(0, 8, 0, 0)
                                    )
                            )
                    );
            }));

        return listBox;
    }

    private static ISolidColorBrush GetAlertColor(string type)
    {
        if (type.Contains("VENCIDO")) return ErrorRed;
        return type.Contains("PRÓXIMO") ? WarningYellow : AccentBlue;
    }

    private static ISolidColorBrush GetAlertBgColor(string type)
    {
        if (type.Contains("VENCIDO")) return ErrorRedBg;
        return type.Contains("PRÓXIMO") ? WarningYellowBg : InfoBlueBg;
    }

    private static string GetAlertIcon(string type)
    {
        if (type.Contains("VENCIDO")) return "💀";
        if (type.Contains("PRÓXIMO")) return "⏳";
        return type.Contains("STOCK BAJO") ? "📉" : "⚠️";
    }
}