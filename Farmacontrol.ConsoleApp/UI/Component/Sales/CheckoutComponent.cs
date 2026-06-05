using Farmacontrol.ConsoleApp.UI.Helper;
using Farmacontrol.Core.Model;
using Farmacontrol.Core.Model.ProductEntity;
using Farmacontrol.Model;

namespace Farmacontrol.ConsoleApp.UI.Component.Sales
{
    public class CheckoutComponent
    {
        public static bool ProcessCheckout(Sale sale, decimal discountPercent, bool applyTax)
        {
            ConsoleHelper.ShowTitle("Checkout y Pago");

            if (sale.Details.Count == 0)
            {
                Console.WriteLine("El carrito está vacío.");
                ConsoleHelper.Pause();
                return false;
            }

            // Datos Extra de la Venta
            string clientName = ConsoleHelper.ReadText("Nombre del Cliente (Enter para omitir): ", allowEmpty: true);
            sale.ClientName = string.IsNullOrWhiteSpace(clientName) ? "Cliente General" : clientName;

            // Recalcular Totales
            decimal subtotal = sale.Details.Sum(d => d.Subtotal);
            decimal discountAmt = subtotal * (discountPercent / 100);
            sale.DiscountPercentage = discountPercent;
            
            if (applyTax)
            {
                sale.TaxAmount = (subtotal - discountAmt) * 0.12m; // Asumiendo 12% IVA
            }

            sale.RecalculateTotal();

            // Método de Pago
            sale.PaymentMethod = GetPaymentMethodInteractive();

            // Si es Efectivo, pedir monto recibido
            if (sale.PaymentMethod == PaymentMethod.Cash)
            {
                while (true)
                {
                    decimal amountTendered = ConsoleHelper.ReadDecimal($"Monto Recibido (Q): ");
                    if (amountTendered >= sale.Total)
                    {
                        decimal change = amountTendered - sale.Total;
                        Console.WriteLine($"\n[INFO] Vuelto a entregar: Q{change:F2}");
                        break;
                    }
                    Console.WriteLine($"El monto debe ser al menos Q{sale.Total:F2}");
                }
            }

            return true; // Continuar con el guardado
        }

        private static PaymentMethod GetPaymentMethodInteractive()
        {
            Console.WriteLine("\nSeleccione el Método de Pago:");
            Console.WriteLine("1. Efectivo");
            Console.WriteLine("2. Tarjeta de Crédito");
            Console.WriteLine("3. Tarjeta de Débito");
            Console.WriteLine("4. Transferencia Bancaria");

            while (true)
            {
                string opt = ConsoleHelper.ReadText("Opción (1-4): ");
                switch (opt)
                {
                    case "1": return PaymentMethod.Cash;
                    case "2": return PaymentMethod.CreditCard;
                    case "3": return PaymentMethod.DebitCard;
                    case "4": return PaymentMethod.Transfer;
                    default:
                        Console.WriteLine("Opción no válida.");
                        break;
                }
            }
        }
    }
}
