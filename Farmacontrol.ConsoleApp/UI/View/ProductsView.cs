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

            var input = ConsoleHelper.ReadText("Nombre o código del producto (o 'fin' para cancelar): ");
            if (input.ToLower() == "fin") return;

            var products = inventoryService.SearchProducts(input);

            if (products.Count == 0)
            {
                Console.WriteLine("\n[AVISO] Producto no encontrado.");
            }
            else
            {
                Console.WriteLine($"\nSe encontraron {products.Count} coincidencias:\n");
                ConsoleHelper.PrintProductsTable(products);

                var infoCode = ConsoleHelper.ReadText(
                    "\nIngrese el código de un producto para ver sus detalles (o Enter para continuar): ",
                    allowEmpty: true);
                if (!string.IsNullOrWhiteSpace(infoCode))
                {
                    var prodInfo = inventoryService.SearchProduct(infoCode);
                    if (prodInfo != null)
                    {
                        Console.WriteLine();
                        prodInfo.ShowInformation();
                    }
                    else
                    {
                        Console.WriteLine("\n[AVISO] Producto no encontrado en los resultados.");
                    }
                }
            }

            ConsoleHelper.Pause();
        }
    }
}