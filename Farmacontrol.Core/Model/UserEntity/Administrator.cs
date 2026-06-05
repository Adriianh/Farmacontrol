using Farmacontrol.Model;

namespace Farmacontrol.Core.Model.UserEntity
{
    public class Administrator : User
    {
        public override string Role => "Administrador";
        
        public Administrator(string name, string username, string password) : base(name, username, password) { }
        private Administrator() : base() { }
        
        public override List<string> GetAllowedActions() =>
        [
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
            "11. Anular Venta",
            "0. Salir"
        ];
    }
}