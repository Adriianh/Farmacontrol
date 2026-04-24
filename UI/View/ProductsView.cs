using Farmacontrol.Model;
using Farmacontrol.Services;
using Farmacontrol.UI.Helper;

namespace Farmacontrol.UI.View
{
    public class ProductsView(Inventory inventory)
    {
        public void SearchProduct()
        {
            ConsoleHelper.ShowTitle("Buscar Producto");

            if (!inventory.GetProducts.Any())
            {
                Console.WriteLine("No hay productos en inventario.");
                ConsoleHelper.Pause();
                return;
            }

            string input = ConsoleHelper.ReadText("Nombre o código del producto (o 'fin' para cancelar): ");
            if (input.ToLower() == "fin") return;

            Product? product = inventory.SearchProduct(input);

            if (product == null)
                Console.WriteLine("Producto no encontrado.");
            else
                product.ShowInformation();

            ConsoleHelper.Pause();
        }

        public void ShowExpiredProducts()
        {
            ConsoleHelper.ShowTitle("Productos Vencidos");

            if (!inventory.GetProducts.Any())
            {
                Console.WriteLine("No hay productos en inventario.");
                ConsoleHelper.Pause();
                return;
            }

            inventory.GetExpiredProducts();
            ConsoleHelper.Pause();
        }
    }
}