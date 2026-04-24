using Farmacontrol.Services;
using Farmacontrol.Model;
using Farmacontrol.UI.Helper;

namespace Farmacontrol.UI.View
{
    public class SuppliersView(SupplierManager supplierManager, Inventory inventory)
    {
        public void ManageSuppliers()
        {
            ConsoleHelper.ShowTitle("Gestionar Proveedores");
            Console.WriteLine("1. Agregar proveedor");
            Console.WriteLine("2. Eliminar proveedor");
            Console.WriteLine("3. Listar proveedores");
            Console.WriteLine("4. Generar pedido por proveedor");

            string option = ConsoleHelper.ReadText("\nSeleccione una opción (o 'fin' para cancelar): ");
            if (option.ToLower() == "fin") return;
            switch (option)
            {
                case "1": AddSupplier(); break;
                case "2": RemoveSupplier(); break;
                case "3":
                    if (!supplierManager.GetSuppliers().Any())
                        Console.WriteLine("No hay proveedores registrados.");
                    else
                        supplierManager.GetAllSuppliers();
                    ConsoleHelper.Pause();
                    break;
                case "4": GenerateOrderBySupplier(); break;
            }
        }

        public void GenerateAllSupplierOrders()
        {
            ConsoleHelper.ShowTitle("Generar Pedidos");

            if (!supplierManager.GetSuppliers().Any())
            {
                Console.WriteLine("No hay proveedores registrados.");
                ConsoleHelper.Pause();
                return;
            }

            if (!inventory.GetProducts.Any())
            {
                Console.WriteLine("No hay productos en inventario.");
                ConsoleHelper.Pause();
                return;
            }

            supplierManager.GenerateAllOrders(inventory.GetProducts.ToList());
            ConsoleHelper.Pause();
        }

        private void AddSupplier()
        {
            ConsoleHelper.ShowTitle("Agregar Proveedor");

            string code = ConsoleHelper.ReadText("Código (o 'fin' para cancelar): ");
            if (code.ToLower() == "fin") return;
            if (supplierManager.SearchSupplier(code) != null)
            {
                Console.WriteLine("Ya existe un proveedor con ese código.");
                ConsoleHelper.Pause();
                return;
            }

            string name = ConsoleHelper.ReadText("Nombre (o 'fin' para cancelar): ");
            if (name.ToLower() == "fin") return;
            string phone = ConsoleHelper.ReadText("Teléfono (o 'fin' para cancelar): ");
            if (phone.ToLower() == "fin") return;
            string email;
            while (true)
            {
                email = ConsoleHelper.ReadText("Correo (o 'fin' para cancelar): ");
                if (email.ToLower() == "fin") return;
                if (email.Contains("@") && email.Contains(".")) break;
                Console.WriteLine("Correo inválido. Debe contener '@' y dominio.");
            }

            int leadTime;
            while (true)
            {
                leadTime = ConsoleHelper.ReadInt("Días de entrega estimados (o '0' para cancelar): ");
                if (leadTime == 0) return;
                if (leadTime > 0) break;
                Console.WriteLine("Debe ser mayor que cero.");
            }

            var supplier = new Supplier(code, name, phone, email, leadTime);
            supplierManager.AddSupplier(supplier);
            Console.WriteLine("Proveedor agregado correctamente.");
            ConsoleHelper.Pause();
        }

        private void RemoveSupplier()
        {
            ConsoleHelper.ShowTitle("Eliminar Proveedor");
            if (!supplierManager.GetSuppliers().Any())
            {
                Console.WriteLine("No hay proveedores registrados.");
                ConsoleHelper.Pause();
                return;
            }

            supplierManager.GetAllSuppliers();

            if (!ConsoleHelper.Confirm("\n¿Desea eliminar un proveedor? (o 'fin' para cancelar)"))
                return;

            string code = ConsoleHelper.ReadText("Código del proveedor a eliminar (o 'fin' para cancelar): ");
            if (code.ToLower() == "fin") return;
            if (supplierManager.SearchSupplier(code) == null)
            {
                Console.WriteLine("No existe un proveedor con ese código.");
                ConsoleHelper.Pause();
                return;
            }

            supplierManager.RemoveSupplier(code);
            Console.WriteLine("Proveedor eliminado.");
            ConsoleHelper.Pause();
        }

        private void GenerateOrderBySupplier()
        {
            ConsoleHelper.ShowTitle("Generar Pedido");
            if (!supplierManager.GetSuppliers().Any())
            {
                Console.WriteLine("No hay proveedores registrados.");
                ConsoleHelper.Pause();
                return;
            }

            string input = ConsoleHelper.ReadText("Nombre o código del proveedor (o 'fin' para cancelar): ");
            if (input.ToLower() == "fin") return;
            var supplier = supplierManager.SearchSupplier(input);

            if (supplier == null)
            {
                Console.WriteLine("Proveedor no encontrado.");
                ConsoleHelper.Pause();
                return;
            }

            supplier.PlaceOrder(inventory.GetProducts.ToList());
            ConsoleHelper.Pause();
        }
    }
}