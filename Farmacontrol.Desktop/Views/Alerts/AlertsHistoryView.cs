using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Layout;
using Avalonia.Media;
using Farmacontrol.Core.Model;
using Farmacontrol.Desktop.States;
using Microsoft.Extensions.DependencyInjection;

namespace Farmacontrol.Desktop.Views.Alerts;

public class AlertsHistoryView()
    : ViewBase<AlertsHistoryState>(Program.ServiceProvider.GetRequiredService<AlertsHistoryState>())
{
    private static readonly ISolidColorBrush BackgroundPrimary = SolidColorBrush.Parse("#0F172A");
    private static readonly ISolidColorBrush BackgroundSecondary = SolidColorBrush.Parse("#1E293B");
    private static readonly ISolidColorBrush TextMuted = SolidColorBrush.Parse("#94A3B8");
    private static readonly ISolidColorBrush ErrorRed = SolidColorBrush.Parse("#EF4444");
    private static readonly ISolidColorBrush WarningYellow = SolidColorBrush.Parse("#F59E0B");
    private static readonly ISolidColorBrush AccentBlue = SolidColorBrush.Parse("#3B82F6");

    private static readonly ISolidColorBrush WarningYellowBg = SolidColorBrush.Parse("#45301A");
    private static readonly ISolidColorBrush ErrorRedBg = SolidColorBrush.Parse("#451A1A");
    private static readonly ISolidColorBrush InfoBlueBg = SolidColorBrush.Parse("#1A3045");

    protected override object Build(AlertsHistoryState state)
    {
        return new Border()
            .Background(BackgroundPrimary)
            .CornerRadius(12)
            .Padding(20)
            .Child(
                new Grid()
                    .Rows("Auto, Auto, *")
                    .Children(
                        BuildHeader().Row(0),
                        BuildToolbar(state).Row(1),
                        BuildAlertsList(state).Row(2)
                    )
            );
    }

    private Control BuildHeader()
    {
        return new Grid().Cols("*, Auto")
            .Children(
                new StackPanel().VerticalAlignment(VerticalAlignment.Center)
                    .Children(
                        new TextBlock()
                            .Text("Historial de Alertas")
                            .FontSize(26)
                            .FontWeight(FontWeight.Bold)
                            .Foreground(Brushes.White),
                        new TextBlock()
                            .Text("Registro histórico de incidencias de inventario")
                            .FontSize(13)
                            .Foreground(TextMuted)
                            .Margin(0, 4, 0, 0)
                    )
            ).Margin(0, 0, 0, 24);
    }

    private Control BuildToolbar(AlertsHistoryState state)
    {
        var headerRow = new Grid()
            .Cols("*, Auto, Auto")
            .Children(
                new TextBox()
                    .With(c => c.PlaceholderText = "Buscar por producto o tipo de alerta...")
                    .Text(state, s => s.SearchQuery, BindingMode.TwoWay)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .CornerRadius(8)
                    .Col(0),
                new CalendarDatePicker()
                    .SelectedDate(state, s => s.StartDate, BindingMode.TwoWay)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .With(c => c.PlaceholderText = "Fecha Inicio")
                    .Width(130)
                    .Margin(12, 0, 0, 0)
                    .Col(1),
                new CalendarDatePicker()
                    .SelectedDate(state, s => s.EndDate, BindingMode.TwoWay)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .With(c => c.PlaceholderText = "Fecha Fin")
                    .Width(130)
                    .Margin(12, 0, 0, 0)
                    .Col(2)
            ).Margin(0, 0, 0, 16);

        return headerRow;
    }

    private Control BuildAlertsList(AlertsHistoryState state)
    {
        var list = new ListBox()
            .ItemsSource(state.AlertsList)
            .ItemTemplate(new FuncDataTemplate<Alert>((alert, _) =>
            {
                var alertColor = GetAlertColor(alert.Type);
                var alertBgColor = GetAlertBgColor(alert.Type);

                return new Border().Background(BackgroundSecondary).CornerRadius(8).Padding(16).Margin(0, 0, 0, 8)
                    .Child(
                        new Grid().Cols("Auto, *, Auto")
                            .Children(
                                new StackPanel().VerticalAlignment(VerticalAlignment.Center).Col(0).Margin(0, 0, 16, 0)
                                    .Children(
                                        new TextBlock().Text(alert.Date.ToString("dd/MM/yyyy"))
                                            .FontWeight(FontWeight.Bold).Foreground(Brushes.White),
                                        new TextBlock().Text(alert.Date.ToString("HH:mm")).FontSize(12)
                                            .Foreground(TextMuted)
                                    ),
                                new StackPanel().VerticalAlignment(VerticalAlignment.Center).Col(1)
                                    .Children(
                                        new TextBlock().Text(alert.ProductName).FontWeight(FontWeight.SemiBold)
                                            .Foreground(Brushes.White),
                                        new TextBlock().Text(alert.Description).FontSize(13).Foreground(TextMuted)
                                            .TextWrapping(TextWrapping.Wrap)
                                    ),
                                new Border().Background(alertBgColor).CornerRadius(16).Padding(12, 4).Col(2)
                                    .VerticalAlignment(VerticalAlignment.Center)
                                    .Child(
                                        new TextBlock().Text(alert.Type).Foreground(alertColor).FontSize(12)
                                            .FontWeight(FontWeight.Bold)
                                    )
                            )
                    );
            }))
            .Background(Brushes.Transparent)
            .BorderBrush(Brushes.Transparent);

        return list;
    }

    private static ISolidColorBrush GetAlertColor(string type)
    {
        if (type.Contains("VENCIDO")) return ErrorRed;
        if (type.Contains("PRÓXIMO")) return WarningYellow;
        return AccentBlue;
    }

    private static ISolidColorBrush GetAlertBgColor(string type)
    {
        if (type.Contains("VENCIDO")) return ErrorRedBg;
        if (type.Contains("PRÓXIMO")) return WarningYellowBg;
        return InfoBlueBg;
    }
}