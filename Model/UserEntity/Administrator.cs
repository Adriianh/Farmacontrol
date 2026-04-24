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
            "5. Ver el historial de alertas",
            "6. Ver reporte de ventas",
            "7. Ver medicamentos vencidos",
            "8. Gestionar usuarios",
            "9. Gestionar proveedores",
            "10. Generar pedidos pendientes",
            "0. Salir"
        };
    }
}