using Farmacontrol.Core.Model;

namespace Farmacontrol.ConsoleApp.UI.Component.Sales
{
    public class CartPrinterComponent
    {
        public static void PrintCart(Sale sale, decimal discountPercent, bool applyTax)
        {
            Console.Clear();
            Console.WriteLine("============================================================================");
            Console.WriteLine("                            🛒 CARRITO DE COMPRAS                         ");
            Console.WriteLine("============================================================================");
            Console.WriteLine($"{"Cod",-8} | {"Producto",-30} | {"Precio",-10} | {"Cant",-5} | {"Subtotal",-10}");
            Console.WriteLine(new string('-', 76));

            if (sale.Details.Count == 0)
            {
                Console.WriteLine(" El carrito está vacío. Añade productos para comenzar.");
            }
            else
            {
                foreach (var detail in sale.Details)
                {
                    string pName = detail.ProductName.Length > 28 ? detail.ProductName.Substring(0, 28) + ".." : detail.ProductName;
                    Console.WriteLine($"{detail.ProductCode,-8} | {pName,-30} | Q{detail.UnitPrice,8:F2} | {detail.Quantity,4} | Q{detail.Subtotal,8:F2}");
                }
            }

            Console.WriteLine("============================================================================");
            
            decimal subtotal = sale.Details.Sum(d => d.Subtotal);
            decimal discountAmt = subtotal * (discountPercent / 100);
            decimal taxAmt = applyTax ? (subtotal - discountAmt) * 0.12m : 0;
            decimal total = subtotal - discountAmt + taxAmt;

            Console.WriteLine($" Subtotal:    Q{subtotal,10:F2}");
            if (discountPercent > 0)
                Console.WriteLine($" Descuento:  -Q{discountAmt,10:F2} ({discountPercent}%)");
            
            Console.WriteLine($" IVA (12%):  +Q{taxAmt,10:F2} " + (applyTax ? "[Activado]" : "[Desactivado]"));
            Console.WriteLine("----------------------------------------------------------------------------");
            Console.WriteLine($" TOTAL:       Q{total,10:F2}");
            Console.WriteLine("============================================================================\n");
        }
    }
}
