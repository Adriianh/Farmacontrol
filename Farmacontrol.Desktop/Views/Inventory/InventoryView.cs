namespace Farmacontrol.Desktop.Views.Inventory;

public class InventoryView : Grid
{
    public InventoryView()
    {
        this.Margin(20);
        this.Rows("Auto, *");
        
        this.Children(
            new Grid().Row(0).Cols("*, Auto")
                .Children(
                    new TextBox()
                        .PlaceholderText("Buscar medicamento...")
                        .Background(Brushes.White)
                        .Foreground(Brushes.Black)
                        .BorderBrush(Brushes.LightGray)
                        .Margin(right: 12, left: 0, top: 0, bottom: 0),
                        
                    new Button().Col(1)
                        .Content("➕ Agregar Producto")
                        .Background(SolidColorBrush.Parse("#2B579A"))
                        .Foreground(Brushes.White)
                        .Padding(horizontal: 16, vertical: 8)
                ),

            new Border().Row(1).Margin(top: 20, bottom: 0, left: 0, right: 0)
                .Background(Brushes.White)
                .BorderBrush(SolidColorBrush.Parse("#E0E0E0"))
                .BorderThickness(1)
                .CornerRadius(6)
                .Padding(15)
                .Child(
                    new TextBlock()
                        .Text("Tabla de Medicamentos (a implementar)")
                        .Foreground(Brushes.DarkGray)
                        .HorizontalAlignment(HorizontalAlignment.Center)
                        .VerticalAlignment(VerticalAlignment.Center)
                )
        );
    }
}