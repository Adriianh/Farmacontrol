using Farmacontrol.ConsoleApp.UI.Helper;
using Farmacontrol.Core.Model;
using Farmacontrol.Core.Model.ProductEntity;
using Farmacontrol.Core.Services;
using Farmacontrol.Model;

namespace Farmacontrol.ConsoleApp.UI.View
{
    public class InventoryView(InventoryService inventoryService, SupplierService supplierService)
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
            Console.WriteLine("8. Modificar producto");
            Console.WriteLine("9. Eliminar producto");

            string option = ConsoleHelper.ReadText("\nSeleccione una opción (o 'fin' para cancelar): ");
            if (option.ToLower() == "fin") return;
            switch (option)
            {
                case "1": AddMedicine(); break;
                case "2": AddCosmetic(); break;
                case "3": AddSupplement(); break;
                case "4": AddSupply(); break;
                case "5":
                    if (!inventoryService.GetProducts.Any())
                        Console.WriteLine("No hay productos en inventario.");
                    else
                        inventoryService.ListProducts();
                    ConsoleHelper.Pause();
                    break;
                case "6": AssociateSupplierToProduct(); break;
                case "7": RegisterPurchase(); break;
                case "8": UpdateProduct(); break;
                case "9": RemoveProduct(); break;
            }
        }

        private void RegisterPurchase()
        {
            ConsoleHelper.ShowTitle("Registrar Ingreso (Compras)");
            
            string supplierCode = ConsoleHelper.ReadText("Código del proveedor (o 'fin' para cancelar): ");
            if (supplierCode.ToLower() == "fin") return;
            
            var supplier = supplierService.SearchSupplier(supplierCode);
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

                var product = inventoryService.SearchProduct(productCode);
                if (product == null)
                {
                    Console.WriteLine("Producto no encontrado en inventario.");
                    continue;
                }

                string lotCode = ConsoleHelper.ReadText("Código de Lote: ");
                int quantity = ReadPositiveQuantity("Cantidad ingresada: ");
                decimal unitCost = ReadPositiveDecimal("Costo Unitario: Q");
                DateTime expDate = ReadValidExpirationDate("Fecha de expiración (dd/MM/yyyy): ");

                purchase.AddDetail(product, lotCode, quantity, unitCost, expDate);
                Console.WriteLine("Producto agregado al ingreso!");
            }

            if (purchase.Details.Any())
            {
                inventoryService.RegisterPurchase(purchase);
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
            if (inventoryService.SearchProduct(code) != null)
            {
                Console.WriteLine("Ya existe un producto con ese código.");
                ConsoleHelper.Pause();
                return;
            }

            int initialStock = ReadPositiveOrZeroInt("Stock inicial (ingrese 0 si no hay inventario): ");
            DateTime? expirationDate = null;
            string? lotCode = null;
            if (initialStock > 0)
            {
                expirationDate = ReadValidExpirationDate("Fecha de vencimiento (dd/MM/yyyy): ");
                lotCode = ConsoleHelper.ReadText("Código de lote inicial: ");
            }

            var medicine = new Medicine
            {
                Name = name,
                Code = code,
                Price = ReadPositiveDecimal("Precio: Q"),
                Stock = 0,
                MinimumStock = ReadPositiveOrZeroInt("Stock mínimo: "),
                Barcode = ReadOptionalField("Código de barras"),
                Location = ReadOptionalField("Ubicación en estantería"),
                Laboratory = ReadOptionalField("Laboratorio fabricante"),
                Subcategory = ReadOptionalField("Sub-categoría (ej. Analgésico)"),
                ActivePrinciple = ConsoleHelper.ReadText("Principio activo: "),
                Concentration = ReadOptionalField("Concentración (ej. 500 mg)"),
                Presentation = ReadOptionalField("Presentación (ej. Caja con 20 tabletas)"),
                RequiresPrescription = ConsoleHelper.Confirm("¿Requiere receta médica?"),
                IsControlled = ConsoleHelper.Confirm("¿Es un medicamento controlado (requiere cédula médica y registro)?"),
                Suppliers = suppliers
            };
            
            medicine.Ingredients = ReadList("Ingredientes (separados por coma, enter para omitir): ");
            medicine.Tags = ReadList("Etiquetas (separadas por coma, enter para omitir): ");

            if (initialStock > 0 && lotCode != null && expirationDate.HasValue)
                medicine.AddBatch(lotCode, initialStock, expirationDate.Value);

            inventoryService.AddProduct(medicine);
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
            if (inventoryService.SearchProduct(code) != null)
            {
                Console.WriteLine("Ya existe un producto con ese código.");
                ConsoleHelper.Pause();
                return;
            }

            int initialStock = ReadPositiveOrZeroInt("Stock inicial (ingrese 0 si no hay inventario): ");
            DateTime? expirationDate = null;
            string? lotCode = null;
            if (initialStock > 0)
            {
                expirationDate = ReadValidExpirationDate("Fecha de vencimiento (dd/MM/yyyy): ");
                lotCode = ConsoleHelper.ReadText("Código de lote inicial: ");
            }

            var cosmetic = new Cosmetic
            {
                Name = name,
                Code = code,
                Price = ReadPositiveDecimal("Precio: Q"),
                Stock = 0,
                MinimumStock = ReadPositiveOrZeroInt("Stock mínimo: "),
                Barcode = ReadOptionalField("Código de barras"),
                Location = ReadOptionalField("Ubicación en estantería"),
                Laboratory = ReadOptionalField("Laboratorio fabricante"),
                Subcategory = ReadOptionalField("Sub-categoría (ej. Cuidado facial)"),
                Brand = ConsoleHelper.ReadText("Marca: "),
                Type = ConsoleHelper.ReadText("Tipo (shampoo, crema, etc.): "),
                Presentation = ReadOptionalField("Presentación (ej. Frasco 250 ml)"),
                Hypoallergenic = ConsoleHelper.Confirm("¿Es hipoalergénico?"),
                Suppliers = suppliers
            };
            
            cosmetic.Ingredients = ReadList("Ingredientes (separados por coma, enter para omitir): ");
            cosmetic.Tags = ReadList("Etiquetas (separadas por coma, enter para omitir): ");

            if (initialStock > 0 && lotCode != null && expirationDate.HasValue)
                cosmetic.AddBatch(lotCode, initialStock, expirationDate.Value);

            inventoryService.AddProduct(cosmetic);
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
            if (inventoryService.SearchProduct(code) != null)
            {
                Console.WriteLine("Ya existe un producto con ese código.");
                ConsoleHelper.Pause();
                return;
            }

            int initialStock = ReadPositiveOrZeroInt("Stock inicial (ingrese 0 si no hay inventario): ");
            DateTime? expirationDate = null;
            string? lotCode = null;
            if (initialStock > 0)
            {
                expirationDate = ReadValidExpirationDate("Fecha de vencimiento (dd/MM/yyyy): ");
                lotCode = ConsoleHelper.ReadText("Código de lote inicial: ");
            }

            var supplement = new Supplement
            {
                Name = name,
                Code = code,
                Price = ReadPositiveDecimal("Precio: Q"),
                Stock = 0,
                MinimumStock = ReadPositiveOrZeroInt("Stock mínimo: "),
                Barcode = ReadOptionalField("Código de barras"),
                Location = ReadOptionalField("Ubicación en estantería"),
                Laboratory = ReadOptionalField("Laboratorio fabricante"),
                Subcategory = ReadOptionalField("Sub-categoría (ej. Vitamina)"),
                ActivePrinciple = ConsoleHelper.ReadText("Principio activo: "),
                Type = ConsoleHelper.ReadText("Tipo: "),
                Format = GetSupplementFormat(),
                Concentration = ReadOptionalField("Concentración (ej. 1000 UI)"),
                RecommendedDosage = ReadOptionalField("Dosis recomendada (ej. 1 cápsula al día)"),
                Suppliers = suppliers
            };
            
            supplement.Ingredients = ReadList("Ingredientes (separados por coma, enter para omitir): ");
            supplement.Tags = ReadList("Etiquetas (separadas por coma, enter para omitir): ");

            if (initialStock > 0 && lotCode != null && expirationDate.HasValue)
                supplement.AddBatch(lotCode, initialStock, expirationDate.Value);

            inventoryService.AddProduct(supplement);
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
            if (inventoryService.SearchProduct(code) != null)
            {
                Console.WriteLine("Ya existe un producto con ese código.");
                ConsoleHelper.Pause();
                return;
            }

            int initialStock = ReadPositiveOrZeroInt("Stock inicial (ingrese 0 si no hay inventario): ");
            DateTime? expirationDate = null;
            string? lotCode = null;
            if (initialStock > 0)
            {
                expirationDate = ReadValidExpirationDate("Fecha de vencimiento (dd/MM/yyyy): ");
                lotCode = ConsoleHelper.ReadText("Código de lote inicial: ");
            }

            var supply = new Supply
            {
                Name = name,
                Code = code,
                Price = ReadPositiveDecimal("Precio: Q"),
                Stock = 0,
                MinimumStock = ReadPositiveOrZeroInt("Stock mínimo: "),
                Barcode = ReadOptionalField("Código de barras"),
                Location = ReadOptionalField("Ubicación en estantería"),
                Laboratory = ReadOptionalField("Laboratorio fabricante"),
                Subcategory = ReadOptionalField("Sub-categoría (ej. Material de curación)"),
                Brand = ConsoleHelper.ReadText("Marca: "),
                Type = ConsoleHelper.ReadText("Tipo: "),
                Size = ReadOptionalField("Tamaño"),
                Material = ReadOptionalField("Material"),
                IsSterile = ConsoleHelper.Confirm("¿Es estéril (libre de bacterias)?"),
                Suppliers = suppliers
            };
            
            supply.Tags = ReadList("Etiquetas (separadas por coma, enter para omitir): ");

            if (initialStock > 0 && lotCode != null && expirationDate.HasValue)
                supply.AddBatch(lotCode, initialStock, expirationDate.Value);

            inventoryService.AddProduct(supply);
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

                var supplier = supplierService.SearchSupplier(supplierCode);
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
            var product = inventoryService.SearchProduct(productCode);
            if (product == null)
            {
                Console.WriteLine("Producto no encontrado.");
                ConsoleHelper.Pause();
                return;
            }

            string supplierCode = ConsoleHelper.ReadText("Código del proveedor a asociar: ");
            var supplier = supplierService.SearchSupplier(supplierCode);
            if (supplier == null)
            {
                Console.WriteLine("Proveedor no encontrado.");
                ConsoleHelper.Pause();
                return;
            }

            bool success = inventoryService.AssociateSupplier(productCode, supplierCode);
            Console.WriteLine(success
                ? $"[Éxito] Proveedor '{supplier.Name}' asociado correctamente al producto '{product.Name}'."
                : "El proveedor ya estaba asociado a este producto o ocurrió un error.");
            ConsoleHelper.Pause();
        }

        private void RemoveProduct()
        {
            ConsoleHelper.ShowTitle("Eliminar Producto");
            if (!inventoryService.GetProducts.Any())
            {
                Console.WriteLine("No hay productos en inventario.");
                ConsoleHelper.Pause();
                return;
            }

            string code = ConsoleHelper.ReadText("Código del producto a eliminar (o 'fin' para cancelar): ");
            if (code.ToLower() == "fin") return;

            var product = inventoryService.SearchProduct(code);
            if (product == null)
            {
                Console.WriteLine("No existe un producto con ese código.");
                ConsoleHelper.Pause();
                return;
            }

            if (!ConsoleHelper.Confirm($"\n¿Está seguro que desea eliminar '{product.Name}'?"))
                return;

            inventoryService.RemoveProduct(product);
            Console.WriteLine("Producto eliminado (borrado lógico) correctamente.");
            ConsoleHelper.Pause();
        }

        private void UpdateProduct()
        {
            ConsoleHelper.ShowTitle("Modificar Producto");
            if (!inventoryService.GetProducts.Any())
            {
                Console.WriteLine("No hay productos en inventario.");
                ConsoleHelper.Pause();
                return;
            }

            string code = ConsoleHelper.ReadText("Código del producto a modificar (o 'fin' para cancelar): ");
            if (code.ToLower() == "fin") return;

            var product = inventoryService.SearchProduct(code);
            if (product == null)
            {
                Console.WriteLine("No existe un producto con ese código.");
                ConsoleHelper.Pause();
                return;
            }

            Console.WriteLine("\nNota: Presione Enter sin escribir nada para mantener el valor actual.");
            product.Name = ConsoleHelper.ReadTextWithDefault("Nombre", product.Name);
            product.Price = ConsoleHelper.ReadDecimalWithDefault("Precio: Q", product.Price);
            product.MinimumStock = ConsoleHelper.ReadIntWithDefault("Stock mínimo", product.MinimumStock);
            product.Barcode = ConsoleHelper.ReadTextWithDefault("Código de barras", product.Barcode ?? "");
            product.Location = ConsoleHelper.ReadTextWithDefault("Ubicación en estantería", product.Location ?? "");
            product.Laboratory = ConsoleHelper.ReadTextWithDefault("Laboratorio fabricante", product.Laboratory ?? "");
            product.Subcategory = ConsoleHelper.ReadTextWithDefault("Sub-categoría", product.Subcategory ?? "");
            product.Ingredients = UpdateList("Ingredientes", product.Ingredients);
            product.Tags = UpdateList("Etiquetas", product.Tags);

            if (product is Medicine medicine)
            {
                medicine.ActivePrinciple = ConsoleHelper.ReadTextWithDefault("Principio activo", medicine.ActivePrinciple);
                medicine.Concentration = ConsoleHelper.ReadTextWithDefault("Concentración", medicine.Concentration ?? "");
                medicine.Presentation = ConsoleHelper.ReadTextWithDefault("Presentación", medicine.Presentation ?? "");
            }
            else if (product is Cosmetic cosmetic)
            {
                cosmetic.Brand = ConsoleHelper.ReadTextWithDefault("Marca", cosmetic.Brand);
                cosmetic.Type = ConsoleHelper.ReadTextWithDefault("Tipo", cosmetic.Type);
                cosmetic.Presentation = ConsoleHelper.ReadTextWithDefault("Presentación", cosmetic.Presentation ?? "");
            }
            else if (product is Supplement supplement)
            {
                supplement.ActivePrinciple = ConsoleHelper.ReadTextWithDefault("Principio activo", supplement.ActivePrinciple);
                supplement.Type = ConsoleHelper.ReadTextWithDefault("Tipo", supplement.Type);
                supplement.Concentration = ConsoleHelper.ReadTextWithDefault("Concentración", supplement.Concentration ?? "");
                supplement.RecommendedDosage = ConsoleHelper.ReadTextWithDefault("Dosis recomendada", supplement.RecommendedDosage ?? "");
            }
            else if (product is Supply supply)
            {
                supply.Brand = ConsoleHelper.ReadTextWithDefault("Marca", supply.Brand);
                supply.Type = ConsoleHelper.ReadTextWithDefault("Tipo", supply.Type);
                supply.Size = ConsoleHelper.ReadTextWithDefault("Tamaño", supply.Size ?? "");
                supply.Material = ConsoleHelper.ReadTextWithDefault("Material", supply.Material ?? "");
            }

            inventoryService.UpdateProduct(product);
            Console.WriteLine("Producto modificado correctamente.");
            ConsoleHelper.Pause();
        }

        private string? ReadOptionalField(string fieldName)
        {
            string value = ConsoleHelper.ReadText($"{fieldName} (opcional, enter para omitir): ", allowEmpty: true);
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }

        private List<string> ReadList(string prompt)
        {
            string input = ConsoleHelper.ReadText(prompt, allowEmpty: true);
            if (string.IsNullOrWhiteSpace(input)) return new List<string>();
            return input.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
        }

        private List<string> UpdateList(string prompt, List<string> currentList)
        {
            string currentStr = string.Join(", ", currentList);
            string input = ConsoleHelper.ReadTextWithDefault($"{prompt} (separados por coma)", currentStr);
            if (string.IsNullOrWhiteSpace(input)) return new List<string>();
            return input.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList();
        }

        private int ReadPositiveOrZeroInt(string prompt)
        {
            while (true)
            {
                int value = ConsoleHelper.ReadInt(prompt);
                if (value >= 0)
                    return value;

                Console.WriteLine("El valor no puede ser negativo.");
            }
        }

        private int ReadPositiveQuantity(string prompt)
        {
            while (true)
            {
                int value = ConsoleHelper.ReadInt(prompt);
                if (value > 0)
                    return value;

                Console.WriteLine("La cantidad debe ser mayor que cero.");
            }
        }

        private decimal ReadPositiveDecimal(string prompt)
        {
            while (true)
            {
                decimal value = ConsoleHelper.ReadDecimal(prompt);
                if (value > 0)
                    return value;

                Console.WriteLine("El valor debe ser mayor que cero.");
            }
        }

        private DateTime ReadValidExpirationDate(string prompt)
        {
            while (true)
            {
                DateTime date = ConsoleHelper.ReadDate(prompt);
                if (date > DateTime.Today)
                    return date;

                Console.WriteLine("La fecha de vencimiento debe ser posterior a la fecha actual.");
            }
        }

        private string ReadCommonProductField(string prompt) => ConsoleHelper.ReadText(prompt);
    }
}