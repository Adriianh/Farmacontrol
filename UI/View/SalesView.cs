using Farmacontrol.Services;
using Farmacontrol.Model;
using Farmacontrol.UI.Helper;

namespace Farmacontrol.UI.View
{
    public class SalesView(Inventory inventory, List<Sale> sales, int salesCounter)
    {
        private int _salesCounter = salesCounter;

        public int SalesCounter => _salesCounter;

        public void RegisterSale()
        {
            if (!inventory.GetProducts.Any())
            {
                ConsoleHelper.ShowTitle("Registrar Venta");
                Console.WriteLine("No hay productos en inventario para vender.");
                ConsoleHelper.Pause();
                return;
            }
            _salesCounter++;
            var sale = new Sale(_salesCounter);
            bool adding = true;
            int productsAdded = 0;

            while (adding)
            {
                ConsoleHelper.ShowTitle("Registrar Venta");
                string input = ConsoleHelper.ReadText("Nombre o código del producto (o 'fin' para terminar): ");

                if (input.ToLower() == "fin")
                {
                    adding = false;
                    continue;
                }

                var product = inventory.SearchProduct(input);

                if (product == null)
                {
                    Console.WriteLine("Producto no encontrado.");
                    ConsoleHelper.Pause();
                    continue;
                }

                product.ShowInformation();
                int quantity;
                while (true)
                {
                    string qtyInput = ConsoleHelper.ReadText("Cantidad (o 'fin' para cancelar): ");
                    if (qtyInput.ToLower() == "fin")
                    {
                        quantity = 0;
                        break;
                    }
                    if (!int.TryParse(qtyInput, out quantity))
                    {
                        Console.WriteLine("Valor inválido, intente de nuevo.");
                        continue;
                    }
                    if (quantity <= 0)
                    {
                        Console.WriteLine("La cantidad debe ser mayor que cero.");
                        continue;
                    }
                    if (quantity > product.Stock)
                    {
                        Console.WriteLine($"No hay suficiente stock. Stock disponible: {product.Stock}");
                        continue;
                    }
                    break;
                }
                if (quantity == 0)
                    continue;

                try
                {
                    sale.AddDetail(product, quantity);
                    productsAdded++;
                    Console.WriteLine("Producto agregado.");
                    Console.WriteLine($"Stock restante: {product.Stock}");
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }

                ConsoleHelper.Pause();
            }

            if (productsAdded == 0)
            {
                ConsoleHelper.ShowTitle("Venta cancelada");
                Console.WriteLine("No se agregaron productos a la venta. La venta no será registrada.");
                ConsoleHelper.Pause();
                return;
            }

            sales.Add(sale);
            ConsoleHelper.ShowTitle("Resumen de Venta");
            sale.ShowResume();
            ConsoleHelper.Pause();
        }
    }
}