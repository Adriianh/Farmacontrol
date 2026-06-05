using System.Windows.Input;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Farmacontrol.Desktop.States;
using Microsoft.Extensions.DependencyInjection;

namespace Farmacontrol.Desktop.Views;

public class DashboardView() : ViewBase<DashboardState>(Program.ServiceProvider.GetRequiredService<DashboardState>())
{
    private static readonly SolidColorBrush CardBackground = SolidColorBrush.Parse("#FFFFFF");
    private static readonly SolidColorBrush TextPrimary = SolidColorBrush.Parse("#1E293B");
    private static readonly SolidColorBrush TextSubtle = SolidColorBrush.Parse("#64748B");
    private static readonly SolidColorBrush AccentColor = SolidColorBrush.Parse("#3B82F6");
    private static readonly SolidColorBrush WarningColor = SolidColorBrush.Parse("#F59E0B");

    protected override object Build(DashboardState state)
    {
        return new ScrollViewer()
            .Content(
                new StackPanel()
                    .Margin(24)
                    .Spacing(24)
                    .Children(
                        new TextBlock()
                            .Text("Panel Principal")
                            .FontSize(24)
                            .FontWeight(FontWeight.Bold)
                            .Foreground(TextPrimary),
                        new UniformGrid()
                            .Columns(3)
                            .Rows(1)
                            .Margin(0, 8, 0, 0)
                            .Children(
                                CreateStatCard("Ventas de Hoy", state, nameof(DashboardState.TodaysSalesCount),
                                    AccentColor, "💰"),
                                CreateStatCard("Ingresos del Día", state, nameof(DashboardState.TodaysSalesTotal),
                                    AccentColor, "💵"),
                                CreateStatCard("Alertas Activas", state, nameof(DashboardState.ActiveAlertsCount),
                                    WarningColor, "⚠️")
                            ),
                        new TextBlock()
                            .Text("Accesos Rápidos")
                            .FontSize(20)
                            .FontWeight(FontWeight.SemiBold)
                            .Foreground(TextPrimary)
                            .Margin(0, 16, 0, 0),
                        new UniformGrid()
                            .Columns(3)
                            .Rows(1)
                            .Margin(0, 8, 0, 0)
                            .Children(
                                CreateQuickAccessButton("🛒 Nueva Venta", "Registra una venta rápidamente",
                                    state.NavigateToSaleCommand),
                                CreateQuickAccessButton("🔍 Buscar Producto", "Consulta el inventario y stock",
                                    state.NavigateToSearchCommand),
                                CreateQuickAccessButton("🔔 Ver Alertas", "Revisa alertas de caducidad y stock",
                                    state.NavigateToAlertsCommand)
                            )
                    )
            );
    }

    private Border CreateStatCard(string title, DashboardState state, string bindingPath, SolidColorBrush iconColor,
        string icon)
    {
        var valueTextBlock = new TextBlock()
            .FontSize(28)
            .FontWeight(FontWeight.Bold)
            .Foreground(TextPrimary)
            .Margin(36, 0, 0, 0);

        valueTextBlock.Bind(TextBlock.TextProperty, new Binding { Source = state, Path = bindingPath });

        return new Border()
            .Background(CardBackground)
            .CornerRadius(12)
            .Padding(20)
            .Margin(8)
            .BoxShadow(new BoxShadows(new BoxShadow
                { Blur = 10, Color = Color.Parse("#1A000000"), OffsetX = 0, OffsetY = 4 }))
            .Child(
                new StackPanel()
                    .Spacing(8)
                    .Children(
                        new StackPanel()
                            .Orientation(Orientation.Horizontal)
                            .Spacing(12)
                            .Children(
                                new TextBlock()
                                    .Text(icon)
                                    .FontSize(24)
                                    .Foreground(iconColor),
                                new TextBlock()
                                    .Text(title)
                                    .FontSize(14)
                                    .FontWeight(FontWeight.SemiBold)
                                    .Foreground(TextSubtle)
                                    .VerticalAlignment(VerticalAlignment.Center)
                            ),
                        valueTextBlock
                    )
            );
    }

    private Button CreateQuickAccessButton(string title, string description, ICommand command)
    {
        return new Button()
            .Command(command)
            .Margin(8)
            .Padding(20)
            .CornerRadius(12)
            .Background(CardBackground)
            .Foreground(TextPrimary)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .VerticalAlignment(VerticalAlignment.Stretch)
            .Cursor(new Cursor(StandardCursorType.Hand))
            .Content(
                new StackPanel()
                    .Spacing(8)
                    .Children(
                        new TextBlock()
                            .Text(title)
                            .FontSize(16)
                            .FontWeight(FontWeight.Bold),
                        new TextBlock()
                            .Text(description)
                            .FontSize(13)
                            .Foreground(TextSubtle)
                            .TextWrapping(TextWrapping.Wrap)
                    )
            );
    }
}