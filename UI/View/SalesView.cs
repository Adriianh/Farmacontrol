using System;
using System.Linq;
using Farmacontrol.Services;
using Farmacontrol.Model;
using Farmacontrol.Model.ProductEntity;
using Farmacontrol.UI.Helper;
using Farmacontrol.Exception;

namespace Farmacontrol.UI.View
{
    public class SalesView(Inventory inventory, SalesManager salesManager)
    {
        public void RegisterSale()
        {
            if (!inventory.GetProducts.Any())
            {
                ConsoleHelper.ShowTitle("Registrar Venta");
                Console.WriteLine("No hay productos en inventario para vender.");
                ConsoleHelper.Pause();
                return;
            }

            ConsoleHelper.ShowTitle("Registrar Venta");
            string clientName = ConsoleHelper.ReadText("Nombre del cliente (opcional, presione Enter para omitir): ");
            if (string.IsNullOrWhiteSpace(clientName)) clientName = "Cliente General";

            PaymentMethod paymentMethod = GetPaymentMethodInteractive();

            int salesCounter = salesManager.GetSalesCount() + 1;
            var sale = new Sale(salesCounter)
            {
                ClientName = clientName,
                PaymentMethod = paymentMethod
            };

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

                // Validar si es un medicamento controlado para exigir cédula médica
                if (product is Medicine med && med.IsControlled)
                {
                    if (string.IsNullOrEmpty(sale.DoctorLicense))
                    {
                        Console.WriteLine($"\n[ATENCIÓN] El producto '{med.Name}' es un MEDICAMENTO CONTROLADO.");
                        string license = ConsoleHelper.ReadText("Ingrese la Cédula Profesional del médico (obligatorio para proceder, o deje en blanco para cancelar): ");
                        if (string.IsNullOrWhiteSpace(license))
                        {
                            Console.WriteLine("Operación cancelada. No se puede vender este producto sin receta médica.");
                            ConsoleHelper.Pause();
                            continue;
                        }
                        sale.DoctorLicense = license;
                    }
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
                catch (InsufficientStockException ex)
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

            salesManager.RegisterSale(sale);
            ConsoleHelper.ShowTitle("Resumen de Venta");
            sale.ShowResume();
            ConsoleHelper.Pause();
        }

        private PaymentMethod GetPaymentMethodInteractive()
        {
            Console.WriteLine("\nSeleccione el método de pago:");
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