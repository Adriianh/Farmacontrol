using Farmacontrol.ConsoleApp.UI.Helper;
using Farmacontrol.Core.Services;

namespace Farmacontrol.ConsoleApp.UI.View
{
    public class ProductsView(InventoryService inventoryService)
    {
        public void SearchProduct()
        {
            ConsoleHelper.ShowTitle("Buscar Producto");

            if (!inventoryService.GetProducts.Any())
            {
                Console.WriteLine("No hay productos en inventario.");
                ConsoleHelper.Pause();
                return;
            }

            string input = ConsoleHelper.ReadText("Nombre o código del producto (o 'fin' para cancelar): ");
            if (input.ToLower() == "fin") return;

            var products = inventoryService.SearchProducts(input);

            if (products.Count == 0)
            {
                Console.WriteLine("Producto no encontrado.");
            }
            else
            {
                Console.WriteLine($"Se encontraron {products.Count} coincidencias:\n");
                foreach (var product in products)
                {
                    product.ShowInformation();
                    Console.WriteLine(new string('-', 20));
                }
            }

            ConsoleHelper.Pause();
        }

        public void ShowExpiredProducts()
        {
            ConsoleHelper.ShowTitle("Productos Vencidos");

            if (!inventoryService.GetProducts.Any())
            {
                Console.WriteLine("No hay productos en inventario.");
                ConsoleHelper.Pause();
                return;
            }

            var products = inventoryService.GetProducts;
            bool foundExpired = false;
            foreach (var product in products)
            {
                var expiredBatches = product.Batches.Where(b => b.Quantity > 0 && b.ExpirationDate < DateTime.Today).ToList();
                foreach (var batch in expiredBatches)
                {
                    foundExpired = true;
                    Console.WriteLine($"Lote {batch.LotCode} - {product.Name} - Venció el {batch.ExpirationDate:dd/MM/yyyy} - Cantidad: {batch.Quantity}");
                    if (ConsoleHelper.Confirm("¿Desea darlo de baja?"))
                    {
                        inventoryService.DiscardBatch(product.Code, batch.LotCode, "Vencimiento");
                        Console.WriteLine("Lote dado de baja correctamente.");
                    }
                }
            }

            if (!foundExpired)
            {
                Console.WriteLine("No se encontraron lotes vencidos con stock disponible.");
            }
            ConsoleHelper.Pause();
        }
    }
}