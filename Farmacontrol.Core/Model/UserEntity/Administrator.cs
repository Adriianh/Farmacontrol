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
            "2. Anular Venta",
            "3. Ver reporte de ventas",
            "4. Gestionar inventario",
            "5. Buscar producto",
            "6. Ver medicamentos vencidos",
            "7. Gestionar proveedores",
            "8. Generar pedidos pendientes",
            "9. Ver alertas",
            "10. Ver el historial de alertas",
            "11. Gestionar usuarios",
            "0. Salir"
        ];
    }
}