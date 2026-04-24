using Farmacontrol.Services;
using Farmacontrol.Model.ProductEntity;
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
            }
        }

        private void AddMedicine()
        {
            ConsoleHelper.ShowTitle("Agregar Medicamento");
            var supplierCode = GetSupplierCodeOrReturn();
            if (supplierCode == null) return;

            string name = ReadCommonProductField("Nombre (o 'fin' para cancelar): ");
            if (name.ToLower() == "fin") return;
            string code = ReadCommonProductField("Código (o 'fin' para cancelar): ");
            if (code.ToLower() == "fin") return;

            var medicine = new Medicine
            {
                Name = name,
                Code = code,
                Price = ReadCommonProductDecimal("Precio: "),
                Stock = ReadCommonProductInt("Stock inicial: "),
                MinimumStock = ReadCommonProductInt("Stock mínimo: "),
                ActivePrinciple = ConsoleHelper.ReadText("Principio activo: "),
                ExpirationDate = ConsoleHelper.ReadDate("Fecha de vencimiento (dd/MM/yyyy): "),
                RequiresPrescription = ConsoleHelper.Confirm("¿Requiere receta?"),
                SupplierCode = supplierCode
            };
            inventory.AddProduct(medicine);
            Console.WriteLine("Medicamento agregado correctamente.");
            ConsoleHelper.Pause();
        }

        private void AddCosmetic()
        {
            ConsoleHelper.ShowTitle("Agregar Producto de Belleza");
            var supplierCode = GetSupplierCodeOrReturn();
            if (supplierCode == null) return;

            string name = ReadCommonProductField("Nombre (o 'fin' para cancelar): ");
            if (name.ToLower() == "fin") return;
            string code = ReadCommonProductField("Código (o 'fin' para cancelar): ");
            if (code.ToLower() == "fin") return;

            var cosmetic = new Cosmetic
            {
                Name = name,
                Code = code,
                Price = ReadCommonProductDecimal("Precio: "),
                Stock = ReadCommonProductInt("Stock inicial: "),
                MinimumStock = ReadCommonProductInt("Stock mínimo: "),
                Brand = ConsoleHelper.ReadText("Marca: "),
                Type = ConsoleHelper.ReadText("Tipo (shampoo, crema, etc.): "),
                ExpirationDate = ConsoleHelper.ReadDate("Fecha de vencimiento (dd/MM/yyyy): "),
                SupplierCode = supplierCode
            };
            inventory.AddProduct(cosmetic);
            Console.WriteLine("Producto de belleza agregado correctamente.");
            ConsoleHelper.Pause();
        }

        private void AddSupplement()
        {
            ConsoleHelper.ShowTitle("Agregar Suplemento");
            var supplierCode = GetSupplierCodeOrReturn();
            if (supplierCode == null) return;

            string name = ReadCommonProductField("Nombre (o 'fin' para cancelar): ");
            if (name.ToLower() == "fin") return;
            string code = ReadCommonProductField("Código (o 'fin' para cancelar): ");
            if (code.ToLower() == "fin") return;

            var supplement = new Supplement
            {
                Name = name,
                Code = code,
                Price = ReadCommonProductDecimal("Precio: "),
                Stock = ReadCommonProductInt("Stock inicial: "),
                MinimumStock = ReadCommonProductInt("Stock mínimo: "),
                ActivePrinciple = ConsoleHelper.ReadText("Principio activo: "),
                Type = ConsoleHelper.ReadText("Tipo: "),
                Format = GetSupplementFormat(),
                ExpirationDate = ConsoleHelper.ReadDate("Fecha de vencimiento (dd/MM/yyyy): "),
                SupplierCode = supplierCode
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
            var supplierCode = GetSupplierCodeOrReturn();
            if (supplierCode == null) return;

            string name = ReadCommonProductField("Nombre (o 'fin' para cancelar): ");
            if (name.ToLower() == "fin") return;
            string code = ReadCommonProductField("Código (o 'fin' para cancelar): ");
            if (code.ToLower() == "fin") return;

            var supply = new Supply
            {
                Name = name,
                Code = code,
                Price = ReadCommonProductDecimal("Precio: "),
                Stock = ReadCommonProductInt("Stock inicial: "),
                MinimumStock = ReadCommonProductInt("Stock mínimo: "),
                Brand = ConsoleHelper.ReadText("Marca: "),
                Type = ConsoleHelper.ReadText("Tipo: "),
                Size = ConsoleHelper.ReadText("Tamaño (opcional, enter para omitir): "),
                Material = ConsoleHelper.ReadText("Material (opcional, enter para omitir): "),
                ExpirationDate = ConsoleHelper.ReadDate("Fecha de vencimiento (dd/MM/yyyy): "),
                SupplierCode = supplierCode
            };
            inventory.AddProduct(supply);
            Console.WriteLine("Suministro agregado correctamente.");
            ConsoleHelper.Pause();
        }

        private string? GetSupplierCodeOrReturn()
        {
            string supplierCode = ConsoleHelper.ReadText("Código de proveedor: ");
            if (supplierManager.SearchSupplier(supplierCode) == null)
            {
                Console.WriteLine("Proveedor no encontrado.");
                ConsoleHelper.Pause();
                return null;
            }

            return supplierCode;
        }

        private string ReadCommonProductField(string prompt) => ConsoleHelper.ReadText(prompt);
        private int ReadCommonProductInt(string prompt) => ConsoleHelper.ReadInt(prompt);
        private decimal ReadCommonProductDecimal(string prompt) => ConsoleHelper.ReadDecimal(prompt);
    }
}