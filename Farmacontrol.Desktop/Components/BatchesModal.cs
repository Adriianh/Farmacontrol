using Farmacontrol.Core.Model;

namespace Farmacontrol.Desktop.Components;

public static class BatchesModal
{
    private static readonly SolidColorBrush BackgroundOverlay = SolidColorBrush.Parse("#80000000");
    private static readonly SolidColorBrush BackgroundCard = SolidColorBrush.Parse("#1F2937");
    private static readonly SolidColorBrush BackgroundInput = SolidColorBrush.Parse("#374151");
    private static readonly SolidColorBrush BorderColor = SolidColorBrush.Parse("#4B5563");
    private static readonly SolidColorBrush TextMuted = SolidColorBrush.Parse("#9CA3AF");
    private static readonly SolidColorBrush WarningYellow = SolidColorBrush.Parse("#FBBF24");
    private static readonly SolidColorBrush SuccessGreen = SolidColorBrush.Parse("#10B981");

    public static Control Build(
        string productName,
        ICollection<Batch> batches,
        Action onClose)
    {
        var closeButton = new Button()
            .Content("✕")
            .Background(Brushes.Transparent)
            .Foreground(TextMuted)
            .FontSize(16)
            .Padding(4)
            .Col(1);

        closeButton.Click += (_, _) => onClose();

        var batchItems = new StackPanel().Spacing(8);
        
        if (batches.Count == 0)
        {
            batchItems.Children.Add(
                new TextBlock()
                    .Text("No hay lotes agregados para este producto")
                    .Foreground(TextMuted)
                    .FontSize(14)
                    .TextAlignment(TextAlignment.Center)
                    .Margin(0, 20)
            );
        }
        else
        {
            foreach (var batch in batches)
            {
                batchItems.Children.Add(BuildBatchItem(batch));
            }
        }

        return new Grid()
            .Background(BackgroundOverlay)
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .VerticalAlignment(VerticalAlignment.Stretch)
            .Children(
                new Border()
                    .Width(500)
                    .MaxHeight(600)
                    .Background(BackgroundCard)
                    .BorderBrush(BorderColor)
                    .BorderThickness(1)
                    .CornerRadius(12)
                    .Padding(24)
                    .HorizontalAlignment(HorizontalAlignment.Center)
                    .VerticalAlignment(VerticalAlignment.Center)
                    .Child(
                        new Grid().Rows("Auto, *")
                            .Children(
                                new Grid().Cols("*, Auto").Row(0)
                                    .Children(
                                        new TextBlock()
                                            .Text($"Lotes - {productName}")
                                            .FontSize(20)
                                            .FontWeight(FontWeight.Bold)
                                            .Foreground(Brushes.White)
                                            .VerticalAlignment(VerticalAlignment.Center),
                                        closeButton
                                    ),
                                new ScrollViewer().Row(1).Margin(0, 16)
                                    .Content(batchItems)
                            )
                    )
            );
    }

    private static Control BuildBatchItem(Batch batch)
    {
        var isExpired = batch.ExpirationDate < DateTime.Today;
        var daysUntilExpiry = (batch.ExpirationDate - DateTime.Today).Days;
        var isExpiringOnly = daysUntilExpiry is <= 30 and > 0;

        var statusColor = isExpired ? SolidColorBrush.Parse("#EF4444") :
            isExpiringOnly ? WarningYellow : SuccessGreen;
        
        var statusText = isExpired ? "VENCIDO" :
            isExpiringOnly ? $"Vence en {daysUntilExpiry}d" : "Disponible";

        return new Border()
            .Background(BackgroundInput)
            .BorderBrush(BorderColor)
            .BorderThickness(1)
            .CornerRadius(8)
            .Padding(16)
            .Child(
                new Grid().Rows("Auto, Auto")
                    .Children(
                        new Grid().Cols("*, Auto").Row(0)
                            .Children(
                                new StackPanel()
                                    .Children(
                                        new TextBlock()
                                            .Text($"Lote: {batch.LotCode}")
                                            .FontSize(14)
                                            .FontWeight(FontWeight.SemiBold)
                                            .Foreground(Brushes.White),
                                        new TextBlock()
                                            .Text($"Stock: {batch.Quantity} unidades")
                                            .FontSize(12)
                                            .Foreground(TextMuted)
                                            .Margin(0, 4, 0, 0)
                                    ),
                                new Border()
                                    .Background(statusColor)
                                    .CornerRadius(4)
                                    .Padding(8, 4)
                                    .Col(1)
                                    .Child(
                                        new TextBlock()
                                            .Text(statusText)
                                            .FontSize(11)
                                            .FontWeight(FontWeight.Bold)
                                            .Foreground(isExpired ? Brushes.White :
                                                isExpiringOnly ? SolidColorBrush.Parse("#000000") : Brushes.White)
                                    )
                            ),
                        new Grid().Cols("*, *").Row(1).Margin(0, 8, 0, 0)
                            .Children(
                                new StackPanel()
                                    .Children(
                                        new TextBlock()
                                            .Text("Fabricación")
                                            .FontSize(10)
                                            .Foreground(TextMuted),
                                        new TextBlock()
                                            .Text(batch.ManufacturingDate.ToString("yyyy-MM-dd"))
                                            .FontSize(12)
                                            .Foreground(Brushes.White)
                                            .Margin(0, 2, 0, 0)
                                    )
                                    .Col(0).Margin(0, 0, 8, 0),
                                new StackPanel()
                                    .Children(
                                        new TextBlock()
                                            .Text("Vencimiento")
                                            .FontSize(10)
                                            .Foreground(TextMuted),
                                        new TextBlock()
                                            .Text(batch.ExpirationDate.ToString("yyyy-MM-dd"))
                                            .FontSize(12)
                                            .Foreground(isExpired ? SolidColorBrush.Parse("#FCA5A5") : Brushes.White)
                                            .Margin(0, 2, 0, 0)
                                    )
                                    .Col(1).Margin(8, 0, 0, 0)
                            )
                    )
            );
    }
}