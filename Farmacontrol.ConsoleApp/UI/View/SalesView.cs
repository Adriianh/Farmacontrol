using Farmacontrol.ConsoleApp.UI.Helper;
using Farmacontrol.Exception;
using Farmacontrol.Model;
using Farmacontrol.Model.ProductEntity;
using Farmacontrol.Services;

namespace Farmacontrol.ConsoleApp.UI.View
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
            string clientName = ConsoleHelper.ReadText("Nombre del cliente (opcional, presione Enter para omitir): ",
                allowEmpty: true);
            if (string.IsNullOrWhiteSpace(clientName)) clientName = "Cliente General";

            PaymentMethod paymentMethod = GetPaymentMethodInteractive();

            int salesCounter = salesManager.GetSalesCount() + 1;
            var sale = new Sale(salesCounter)
            {
                ClientName = clientName,
                PaymentMethod = paymentMethod
            };

            decimal discount;
            while (true)
            {
                discount = ConsoleHelper.ReadDecimal("Porcentaje de Descuento (ej. 10 para 10%, 0 si no aplica): ");
                if (discount is >= 0 and <= 100)
                    break;

                Console.WriteLine("El descuento debe estar entre 0 y 100.");
            }

            sale.DiscountPercentage = discount;

            bool adding = true;
            int productsAdded = 0;
            Dictionary<string, int> reservedQuantities = new();

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

                int alreadyReserved = reservedQuantities.GetValueOrDefault(product.Code, 0);
                int availableStock = product.Stock - alreadyReserved;

                if (availableStock <= 0)
                {
                    Console.WriteLine($"No hay stock disponible para agregar más unidades de {product.Name}.");
                    ConsoleHelper.Pause();
                    continue;
                }

                // Validar si es un medicamento controlado para exigir cédula médica
                if (product is Medicine med && med.IsControlled)
                {
                    if (string.IsNullOrEmpty(sale.DoctorLicense))
                    {
                        Console.WriteLine($"\n[ATENCIÓN] El producto '{med.Name}' es un MEDICAMENTO CONTROLADO.");
                        string license = ConsoleHelper.ReadText(
                            "Ingrese la Cédula Profesional del médico (obligatorio para proceder, o deje en blanco para cancelar): ",
                            allowEmpty: true);
                        if (string.IsNullOrWhiteSpace(license))
                        {
                            Console.WriteLine(
                                "Operación cancelada. No se puede vender este producto sin receta médica.");
                            ConsoleHelper.Pause();
                            continue;
                        }

                        string presDocName = ConsoleHelper.ReadText("Nombre del médico: ", allowEmpty: true);
                        string presPatient = ConsoleHelper.ReadText("Nombre del paciente: ", allowEmpty: true);
                        string presDateInput =
                            ConsoleHelper.ReadText("Fecha de emisión (dd/MM/yyyy): ", allowEmpty: true);
                        if (!DateTime.TryParseExact(presDateInput, "dd/MM/yyyy", null,
                                System.Globalization.DateTimeStyles.None, out DateTime presDate))
                        {
                            presDate = DateTime.Now;
                        }

                        string presFolio = ConsoleHelper.ReadText("Referencias / Folio: ", allowEmpty: true);

                        sale.DoctorLicense = license;
                        sale.Prescription = new Prescription(sale.Code, license, presDocName,
                            (string.IsNullOrEmpty(presPatient) ? clientName : presPatient), presDate, presFolio);
                    }
                }

                product.ShowInformation();
                Console.WriteLine($"Stock disponible para esta venta: {availableStock}");

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

                    if (quantity > availableStock)
                    {
                        Console.WriteLine(
                            $"No hay suficiente stock disponible para esta venta. Stock disponible: {availableStock}");
                        continue;
                    }

                    break;
                }

                if (quantity == 0)
                    continue;

                try
                {
                    sale.AddDetail(product, quantity);

                    reservedQuantities[product.Code] = alreadyReserved + quantity;
                    int remainingStock = product.Stock - reservedQuantities[product.Code];

                    productsAdded++;
                    Console.WriteLine("Producto agregado.");
                    Console.WriteLine($"Stock restante después de esta venta: {remainingStock}");
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

            if (ConsoleHelper.Confirm("¿Aplicar impuestos (IVA)?"))
            {
                decimal subtotal = sale.Details.Sum(d => d.Subtotal);
                decimal discountAmt = subtotal * (sale.DiscountPercentage / 100);
                sale.TaxAmount = (subtotal - discountAmt) * 0.12m; // Asumiendo un 12% de IVA
            }

            sale.RecalculateTotal();

            salesManager.RegisterSale(sale);
            ConsoleHelper.ShowTitle("Resumen de Venta");
            sale.ShowResume();
            ConsoleHelper.Pause();
        }

        public void VoidSale()
        {
            ConsoleHelper.ShowTitle("Anular Venta");
            int saleCode = ConsoleHelper.ReadInt("Ingrese el código de la venta a anular: ");

            try
            {
                salesManager.VoidSale(saleCode);
                Console.WriteLine($"Venta #{saleCode} anulada exitosamente (si existía).");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"Error al intentar anular: {ex.Message}");
            }

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