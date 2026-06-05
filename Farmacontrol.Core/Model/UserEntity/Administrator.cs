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
            "1. 🔍 Historial de Ventas",
            "2. 💵 Registrar Venta",
            "3. ❌ Anular Venta",
            "4. 📈 Reporte de Ventas",
            "5. 📋 Ver Stock",
            "6. 🔍 Buscar Producto",
            "7. 📝 Pedidos Pendientes",
            "8. 🚨 Alertas Activas",
            "9. 📜 Historial de Alertas",
            "10. 👥 Gestionar Usuarios",
            "11. 🚚 Proveedores",
            "0. 🚪 Salir"
        ];
    }
}