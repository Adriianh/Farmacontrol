namespace Farmacontrol.Model.UserEntity
{
    public class Administrator(string name, string username, string password) : User(name, username, password)
    {
        public override string Role => "Administrador";

        public override List<string> GetAllowedActions() => new()
        {
            "1. Registrar venta",
            "2. Gestionar inventario",
            "3. Buscar producto",
            "4. Ver alertas",
            "5. Ver reporte de ventas",
            "6. Ver medicamentos vencidos",
            "7. Gestionar usuarios",
            "8. Gestionar proveedores",
            "9. Generar pedidos pendientes",
            "0. Salir"
        };
    }
}