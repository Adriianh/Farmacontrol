using Farmacontrol.Model;
using Farmacontrol.Model.ProductEntity;
using Farmacontrol.Services;
using Farmacontrol.UI.Helper;

namespace Farmacontrol.UI.View
{
    public class InventoryView(Inventory inventory, SupplierManager supplierManager)
    {
        public void ManageInventory()
        {
            ConsoleHelper.ShowTitle("Gestionar Inventario");
            Console.WriteLine("1. Agregar medicamento");
            Console.WriteLine("2. Agregar producto de belleza");
            Console.WriteLine("3. Agregar suplemento");
            Console.WriteLine("4. Agregar suministro");
            Console.WriteLine("5. Mostrar todo el inventario");
            Console.WriteLine("6. Asociar proveedor a producto existente");
            Console.WriteLine("7. Registrar ingreso (Compras)");

            string option = ConsoleHelper.ReadText("\nSeleccione una opción (o 'fin' para cancelar): ");
            if (option.ToLower() == "fin") return;
            switch (option)
            {
                case "1": AddMedicine(); break;
                case "2": AddCosmetic(); break;
                case "3": AddSupplement(); break;
                case "4": AddSupply(); break;
                case "5":
                    if (!inventory.GetProducts.Any())
                        Console.WriteLine("No hay productos en inventario.");
                    else
                        inventory.ListProducts();
                    ConsoleHelper.Pause();
                    break;
                case "6": AssociateSupplierToProduct(); break;
                case "7": RegisterPurchase(); break;
            }
        }

        private void RegisterPurchase()
        {
            ConsoleHelper.ShowTitle("Registrar Ingreso (Compras)");
            
            string supplierCode = ConsoleHelper.ReadText("Código del proveedor (o 'fin' para cancelar): ");
            if (supplierCode.ToLower() == "fin") return;
            
            var supplier = supplierManager.SearchSupplier(supplierCode);
            if (supplier == null)
            {
                Console.WriteLine("Proveedor no encontrado.");
                ConsoleHelper.Pause();
                return;
            }

            string invoice = ConsoleHelper.ReadText("Número de Factura: ");
            var purchase = new Purchase(supplierCode, invoice);
            
            bool adding = true;
            while(adding)
            {
                ConsoleHelper.ShowTitle($"Compra de {supplier.Name} - {invoice}");
                string productCode = ConsoleHelper.ReadText("Código de producto ingresado (o 'fin' para terminar, 'nuevo' para crear uno): ");
                
                if (productCode.ToLower() == "fin")
                {
                    adding = false;
                    continue;
                }

                if (productCode.ToLower() == "nuevo")
                {
                    Console.WriteLine("Por favor regístrelo usando el menú principal de inventario (1-4).");
                    ConsoleHelper.Pause();
                    continue;
                }

                var product = inventory.SearchProduct(productCode);
                if (product == null)
                {
                    Console.WriteLine("Producto no encontrado en inventario.");
                    continue;
                }

                string lotCode = ConsoleHelper.ReadText("Código de Lote: ");
                int quantity = ConsoleHelper.ReadInt("Cantidad ingresada: ");
                decimal unitCost = ConsoleHelper.ReadDecimal("Costo Unitario: Q");
                DateTime expDate = ConsoleHelper.ReadDate("Fecha de expiración (dd/MM/yyyy): ");

                purchase.AddDetail(product, lotCode, quantity, unitCost, expDate);
                Console.WriteLine("Producto agregado al ingreso!");
            }

            if (purchase.Details.Any())
            {
                inventory.RegisterPurchase(purchase);
                Console.WriteLine($"\nIngreso registrado exitosamente. Total: Q{purchase.TotalCost:F2}");
            }
            ConsoleHelper.Pause();
        }

        private void AddMedicine()
        {
            ConsoleHelper.ShowTitle("Agregar Medicamento");
            var suppliers = GetSuppliersInteractive();

            string name = ReadCommonProductField("Nombre (o 'fin' para cancelar): ");
            if (name.ToLower() == "fin") return;
            string code = ReadCommonProductField("Código (o 'fin' para cancelar): ");
            if (code.ToLower() == "fin") return;

            var medicine = new Medicine
            {
                Name = name,
                Code = code,
                Price = ReadCommonProductDecimal("Precio: Q"),
                Stock = ReadCommonProductInt("Stock inicial: "),
                MinimumStock = ReadCommonProductInt("Stock mínimo: "),
                Barcode = ReadOptionalField("Código de barras"),
                Location = ReadOptionalField("Ubicación en estantería"),
                Laboratory = ReadOptionalField("Laboratorio fabricante"),
                ActivePrinciple = ConsoleHelper.ReadText("Principio activo: "),
                Concentration = ReadOptionalField("Concentración (ej. 500 mg)"),
                Presentation = ReadOptionalField("Presentación (ej. Caja con 20 tabletas)"),
                ExpirationDate = ConsoleHelper.ReadDate("Fecha de vencimiento (dd/MM/yyyy): "),
                RequiresPrescription = ConsoleHelper.Confirm("¿Requiere receta médica?"),
                IsControlled = ConsoleHelper.Confirm("¿Es un medicamento controlado (requiere cédula médica y registro)?"),
                Suppliers = suppliers
            };
            inventory.AddProduct(medicine);
            Console.WriteLine("Medicamento agregado correctamente.");
            ConsoleHelper.Pause();
        }

        private void AddCosmetic()
        {
            ConsoleHelper.ShowTitle("Agregar Producto de Belleza");
            var suppliers = GetSuppliersInteractive();

            string name = ReadCommonProductField("Nombre (o 'fin' para cancelar): ");
            if (name.ToLower() == "fin") return;
            string code = ReadCommonProductField("Código (o 'fin' para cancelar): ");
            if (code.ToLower() == "fin") return;

            var cosmetic = new Cosmetic
            {
                Name = name,
                Code = code,
                Price = ReadCommonProductDecimal("Precio: Q"),
                Stock = ReadCommonProductInt("Stock inicial: "),
                MinimumStock = ReadCommonProductInt("Stock mínimo: "),
                Barcode = ReadOptionalField("Código de barras"),
                Location = ReadOptionalField("Ubicación en estantería"),
                Laboratory = ReadOptionalField("Laboratorio fabricante"),
                Brand = ConsoleHelper.ReadText("Marca: "),
                Type = ConsoleHelper.ReadText("Tipo (shampoo, crema, etc.): "),
                Presentation = ReadOptionalField("Presentación (ej. Frasco 250 ml)"),
                Hypoallergenic = ConsoleHelper.Confirm("¿Es hipoalergénico?"),
                ExpirationDate = ConsoleHelper.ReadDate("Fecha de vencimiento (dd/MM/yyyy): "),
                Suppliers = suppliers
            };
            inventory.AddProduct(cosmetic);
            Console.WriteLine("Producto de belleza agregado correctamente.");
            ConsoleHelper.Pause();
        }

        private void AddSupplement()
        {
            ConsoleHelper.ShowTitle("Agregar Suplemento");
            var suppliers = GetSuppliersInteractive();

            string name = ReadCommonProductField("Nombre (o 'fin' para cancelar): ");
            if (name.ToLower() == "fin") return;
            string code = ReadCommonProductField("Código (o 'fin' para cancelar): ");
            if (code.ToLower() == "fin") return;

            var supplement = new Supplement
            {
                Name = name,
                Code = code,
                Price = ReadCommonProductDecimal("Precio: Q"),
                Stock = ReadCommonProductInt("Stock inicial: "),
                MinimumStock = ReadCommonProductInt("Stock mínimo: "),
                Barcode = ReadOptionalField("Código de barras"),
                Location = ReadOptionalField("Ubicación en estantería"),
                Laboratory = ReadOptionalField("Laboratorio fabricante"),
                ActivePrinciple = ConsoleHelper.ReadText("Principio activo: "),
                Type = ConsoleHelper.ReadText("Tipo: "),
                Format = GetSupplementFormat(),
                Concentration = ReadOptionalField("Concentración (ej. 1000 UI)"),
                RecommendedDosage = ReadOptionalField("Dosis recomendada (ej. 1 cápsula al día)"),
                ExpirationDate = ConsoleHelper.ReadDate("Fecha de vencimiento (dd/MM/yyyy): "),
                Suppliers = suppliers
            };
            inventory.AddProduct(supplement);
            Console.WriteLine("Suplemento agregado correctamente.");
            ConsoleHelper.Pause();
        }

        private SupplementFormat GetSupplementFormat()
        {
            Console.WriteLine("Seleccione el formato:");
            var values = Enum.GetValues(typeof(SupplementFormat));
            int i = 1;
            foreach (var value in values)
            {
                Console.WriteLine($"{i}. {value}");
                i++;
            }

            int option;
            do
            {
                option = ConsoleHelper.ReadInt("Opción: ");
            } while (option < 1 || option > values.Length);

            return (SupplementFormat)values.GetValue(option - 1)!;
        }

        private void AddSupply()
        {
            ConsoleHelper.ShowTitle("Agregar Suministro");
            var suppliers = GetSuppliersInteractive();

            string name = ReadCommonProductField("Nombre (o 'fin' para cancelar): ");
            if (name.ToLower() == "fin") return;
            string code = ReadCommonProductField("Código (o 'fin' para cancelar): ");
            if (code.ToLower() == "fin") return;

            var supply = new Supply
            {
                Name = name,
                Code = code,
                Price = ReadCommonProductDecimal("Precio: Q"),
                Stock = ReadCommonProductInt("Stock inicial: "),
                MinimumStock = ReadCommonProductInt("Stock mínimo: "),
                Barcode = ReadOptionalField("Código de barras"),
                Location = ReadOptionalField("Ubicación en estantería"),
                Laboratory = ReadOptionalField("Laboratorio fabricante"),
                Brand = ConsoleHelper.ReadText("Marca: "),
                Type = ConsoleHelper.ReadText("Tipo: "),
                Size = ReadOptionalField("Tamaño"),
                Material = ReadOptionalField("Material"),
                IsSterile = ConsoleHelper.Confirm("¿Es estéril (libre de bacterias)?"),
                ExpirationDate = ConsoleHelper.ReadDate("Fecha de vencimiento (dd/MM/yyyy): "),
                Suppliers = suppliers
            };
            inventory.AddProduct(supply);
            Console.WriteLine("Suministro agregado correctamente.");
            ConsoleHelper.Pause();
        }

        private List<Supplier> GetSuppliersInteractive()
        {
            var selectedSuppliers = new List<Supplier>();
            while (true)
            {
                string supplierCode = ConsoleHelper.ReadText("Código de proveedor (o presione Enter/escriba 'fin' para terminar): ", allowEmpty: true);
                if (string.IsNullOrWhiteSpace(supplierCode) || supplierCode.ToLower() == "fin")
                {
                    break;
                }

                var supplier = supplierManager.SearchSupplier(supplierCode);
                if (supplier == null)
                {
                    Console.WriteLine("[Error] Proveedor no encontrado. Intente de nuevo.");
                }
                else
                {
                    if (selectedSuppliers.Any(s => s.Code == supplier.Code))
                    {
                        Console.WriteLine("Este proveedor ya ha sido agregado.");
                    }
                    else
                    {
                        selectedSuppliers.Add(supplier);
                        Console.WriteLine($"[Ok] Proveedor '{supplier.Name}' asociado.");
                    }
                }
            }
            return selectedSuppliers;
        }

        private void AssociateSupplierToProduct()
        {
            ConsoleHelper.ShowTitle("Asociar Proveedor a Producto");
            string productCode = ConsoleHelper.ReadText("Código del producto: ");
            var product = inventory.SearchProduct(productCode);
            if (product == null)
            {
                Console.WriteLine("Producto no encontrado.");
                ConsoleHelper.Pause();
                return;
            }

            string supplierCode = ConsoleHelper.ReadText("Código del proveedor a asociar: ");
            var supplier = supplierManager.SearchSupplier(supplierCode);
            if (supplier == null)
            {
                Console.WriteLine("Proveedor no encontrado.");
                ConsoleHelper.Pause();
                return;
            }

            bool success = inventory.AssociateSupplier(productCode, supplierCode);
            Console.WriteLine(success
                ? $"[Éxito] Proveedor '{supplier.Name}' asociado correctamente al producto '{product.Name}'."
                : "El proveedor ya estaba asociado a este producto o ocurrió un error.");
            ConsoleHelper.Pause();
        }

        private string? ReadOptionalField(string fieldName)
        {
            string value = ConsoleHelper.ReadText($"{fieldName} (opcional, enter para omitir): ", allowEmpty: true);
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private string ReadCommonProductField(string prompt) => ConsoleHelper.ReadText(prompt);
        private int ReadCommonProductInt(string prompt) => ConsoleHelper.ReadInt(prompt);
        private decimal ReadCommonProductDecimal(string prompt) => ConsoleHelper.ReadDecimal(prompt);
    }
}