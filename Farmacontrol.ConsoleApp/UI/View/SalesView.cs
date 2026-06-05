using Farmacontrol.ConsoleApp.UI.Helper;
using Farmacontrol.Core.Exception;
using Farmacontrol.Core.Model;
using Farmacontrol.Core.Model.ProductEntity;
using Farmacontrol.Core.Services;
using Farmacontrol.Model;

namespace Farmacontrol.ConsoleApp.UI.View
{
    public class SalesView(InventoryService inventoryService, SalesService salesService)
    {
        public void RegisterSale()
        {
            int salesCounter = salesService.GetSalesCount() + 1;
            var sale = new Sale(salesCounter);
            
            decimal discountPercent = 0m;
            bool applyTax = false;
            
            bool running = true;
            while (running)
            {
                Farmacontrol.ConsoleApp.UI.Component.Sales.CartPrinterComponent.PrintCart(sale, discountPercent, applyTax);
                
                Console.WriteLine("Opciones:");
                Console.WriteLine("1. 🔍 Buscar y añadir producto");
                Console.WriteLine("2. 🏷️ Ajustar descuento (%)");
                Console.WriteLine("3. 📝 Alternar Impuestos (IVA)");
                Console.WriteLine("4. 🗑️ Vaciar carrito");
                Console.WriteLine("5. ✅ Proceder al Pago (Checkout)");
                Console.WriteLine("0. ❌ Cancelar Venta y volver");
                
                string option = ConsoleHelper.ReadText("\nSeleccione una opción: ");
                switch (option)
                {
                    case "1":
                        AddProductToCart(sale);
                        break;
                    case "2":
                        discountPercent = ConsoleHelper.ReadDecimal("Porcentaje de Descuento (0-100): ");
                        if (discountPercent < 0 || discountPercent > 100) discountPercent = 0;
                        break;
                    case "3":
                        applyTax = !applyTax;
                        break;
                    case "4":
                        sale.Details.Clear();
                        break;
                    case "5":
                        if (Farmacontrol.ConsoleApp.UI.Component.Sales.CheckoutComponent.ProcessCheckout(sale, discountPercent, applyTax))
                        {
                            try
                            {
                                salesService.RegisterSale(sale);
                                ConsoleHelper.ShowTitle("¡Venta Exitosa!");
                                Console.WriteLine($"Venta #{sale.Code} procesada y registrada.\n");
                                ConsoleHelper.Pause();
                                running = false;
                            }
                            catch (System.Exception ex)
                            {
                                Console.WriteLine($"\n[Error] No se pudo guardar la venta: {ex.Message}");
                                ConsoleHelper.Pause();
                            }
                        }
                        break;
                    case "0":
                        if (ConsoleHelper.Confirm("¿Seguro que desea cancelar esta venta y perder el carrito?"))
                            running = false;
                        break;
                }
            }
        }

        private void AddProductToCart(Sale sale)
        {
            string input = ConsoleHelper.ReadText("\nBuscar producto por nombre o código (o Enter para volver): ", allowEmpty: true);
            if (string.IsNullOrWhiteSpace(input)) return;

            var matches = inventoryService.SearchProducts(input);
            if (matches.Count == 0)
            {
                Console.WriteLine("\n[AVISO] Producto no encontrado.");
                ConsoleHelper.Pause();
                return;
            }

            Farmacontrol.Model.Product? product = null;

            if (matches.Count == 1)
            {
                product = matches[0];
            }
            else
            {
                var topMatches = matches.Take(10).ToList();
                Console.WriteLine("\nResultados de búsqueda:");
                Console.WriteLine($"{"Nº",-3} | {"Código",-8} | {"Producto",-25} | {"Stock",-5} | {"Precio"}");
                Console.WriteLine(new string('-', 60));
                for (int i = 0; i < topMatches.Count; i++)
                {
                    string name = topMatches[i].Name.Length > 23 ? topMatches[i].Name.Substring(0, 23) + ".." : topMatches[i].Name;
                    Console.WriteLine($"{i + 1,-3} | {topMatches[i].Code,-8} | {name,-25} | {topMatches[i].Stock,-5} | Q{topMatches[i].Price:F2}");
                }
                
                string sel = ConsoleHelper.ReadText("\nIngrese el número del producto (o '0' para cancelar): ");
                if (int.TryParse(sel, out int index) && index > 0 && index <= topMatches.Count)
                {
                    product = topMatches[index - 1];
                }
                else
                {
                    return;
                }
            }

            if (product == null) return;

            int alreadyInCart = sale.Details.Where(d => d.ProductCode == product.Code).Sum(d => d.Quantity);
            int availableStock = product.Stock - alreadyInCart;

            if (availableStock <= 0)
            {
                Console.WriteLine($"\n[AVISO] No hay stock disponible para agregar más unidades de {product.Name}.");
                ConsoleHelper.Pause();
                return;
            }

            product.ShowInformation();
            Console.WriteLine($"Stock máximo disponible para agregar: {availableStock}");

            int quantity = ConsoleHelper.ReadInt("Cantidad a añadir al carrito: ");
            if (quantity <= 0) return;

            if (quantity > availableStock)
            {
                Console.WriteLine($"\n[AVISO] No puedes añadir {quantity}, solo quedan {availableStock} disponibles.");
                ConsoleHelper.Pause();
                return;
            }

            if (product is Farmacontrol.Core.Model.ProductEntity.Medicine med && med.IsControlled)
            {
                if (string.IsNullOrEmpty(sale.DoctorLicense))
                {
                    Console.WriteLine($"\n[ATENCIÓN] El producto '{med.Name}' es un MEDICAMENTO CONTROLADO.");
                    string license = ConsoleHelper.ReadText("Ingrese la Cédula Profesional del médico (obligatorio para proceder, o deje en blanco para cancelar): ", allowEmpty: true);
                    if (string.IsNullOrWhiteSpace(license))
                    {
                        Console.WriteLine("Operación cancelada. No se puede vender este producto sin receta médica.");
                        ConsoleHelper.Pause();
                        return;
                    }

                    string presDocName = ConsoleHelper.ReadText("Nombre del médico: ", allowEmpty: true);
                    string presPatient = ConsoleHelper.ReadText("Nombre del paciente: ", allowEmpty: true);
                    string presDateInput = ConsoleHelper.ReadText("Fecha de emisión (dd/MM/yyyy): ", allowEmpty: true);
                    if (!DateTime.TryParseExact(presDateInput, "dd/MM/yyyy", null, System.Globalization.DateTimeStyles.None, out DateTime presDate))
                    {
                        presDate = DateTime.Now;
                    }
                    string presFolio = ConsoleHelper.ReadText("Referencias / Folio: ", allowEmpty: true);

                    sale.DoctorLicense = license;
                    sale.Prescription = new Prescription(sale.Code, license, presDocName, (string.IsNullOrEmpty(presPatient) ? sale.ClientName : presPatient), presDate, presFolio);
                }
            }

            try
            {
                sale.AddDetail(product, quantity);
                Console.WriteLine("\n[Éxito] Producto añadido al carrito.");
            }
            catch (InsufficientStockException ex)
            {
                Console.WriteLine($"\n[Error] {ex.Message}");
            }
            ConsoleHelper.Pause();
        }

        public void VoidSale()
        {
            ConsoleHelper.ShowTitle("Anular Venta");

            var recentSales = salesService.GetAllSales()
                .Where(s => !s.IsVoided)
                .OrderByDescending(s => s.Date)
                .Take(15)
                .ToList();

            Console.WriteLine("Últimas ventas registradas:");
            Console.WriteLine($"{"Nº",-3} | {"ID",-5} | {"Fecha",-14} | {"Cliente",-20} | {"Total"}");
            Console.WriteLine(new string('-', 60));
            
            for (int i = 0; i < recentSales.Count; i++)
            {
                var s = recentSales[i];
                string client = s.ClientName.Length > 18 ? s.ClientName.Substring(0, 18) + ".." : s.ClientName;
                Console.WriteLine($"{i + 1,-3} | {s.Code,-5} | {s.Date:dd/MM HH:mm} | {client,-20} | Q{s.Total:F2}");
            }
            Console.WriteLine(new string('-', 60));
            
            int saleCode = -1;
            string input = ConsoleHelper.ReadText("\nIngrese el número de la lista (1-15), o 'B' para buscar por ID exacto (0 para cancelar): ");
            
            if (input == "0") return;
            
            if (input.ToUpper() == "B")
            {
                saleCode = ConsoleHelper.ReadInt("Ingrese el código exacto de la venta a anular: ");
            }
            else if (int.TryParse(input, out int index) && index > 0 && index <= recentSales.Count)
            {
                saleCode = recentSales[index - 1].Code;
            }
            else
            {
                Console.WriteLine("\n[Error] Selección inválida.");
                ConsoleHelper.Pause();
                return;
            }

            var sale = salesService.GetAllSales().FirstOrDefault(s => s.Code == saleCode);
            if (sale == null)
            {
                Console.WriteLine("\n[Error] No se encontró ninguna venta con ese código.");
                ConsoleHelper.Pause();
                return;
            }

            if (sale.IsVoided)
            {
                Console.WriteLine("\n[AVISO] Esta venta ya ha sido anulada anteriormente.");
                Console.WriteLine($"Razón: {sale.VoidReason}");
                ConsoleHelper.Pause();
                return;
            }

            Console.WriteLine("\n=================================================");
            Console.WriteLine($" DETALLE DE VENTA #{sale.Code} - {sale.Date:dd/MM/yyyy HH:mm}");
            Console.WriteLine("=================================================");
            Console.WriteLine($" Cliente: {sale.ClientName}");
            Console.WriteLine($" Método de Pago: {sale.PaymentMethod}");
            Console.WriteLine("-------------------------------------------------");
            foreach (var detail in sale.Details)
            {
                Console.WriteLine($" {detail.Quantity}x {detail.ProductName} - Q{detail.Subtotal:F2}");
            }
            Console.WriteLine("-------------------------------------------------");
            Console.WriteLine($" TOTAL: Q{sale.Total:F2}");
            Console.WriteLine("=================================================\n");

            if (!ConsoleHelper.Confirm("¿Está seguro que desea ANULAR esta venta de forma permanente?"))
            {
                Console.WriteLine("\nOperación cancelada.");
                ConsoleHelper.Pause();
                return;
            }

            Console.WriteLine("\nOpciones de anulación:");
            Console.WriteLine("1. Devuelto al inventario");
            Console.WriteLine("2. Dado de baja");
            Console.WriteLine("3. Otro motivo");
            string option = ConsoleHelper.ReadText("Seleccione una opción (1-3): ");
            
            string reason = option switch
            {
                "1" => "Devuelto al inventario",
                "2" => "Dado de baja",
                _ => "Otro motivo"
            };

            string details = ConsoleHelper.ReadText("Ingrese los detalles o justificación de la anulación: ", allowEmpty: true);

            try
            {
                salesService.VoidSale(saleCode, reason, details);
                Console.WriteLine($"\n[Éxito] Venta #{saleCode} anulada correctamente.");
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"\n[Error] Error al intentar anular: {ex.Message}");
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