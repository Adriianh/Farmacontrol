using Farmacontrol.Model;

namespace Farmacontrol.Core.Model.UserEntity
{
    public class Employee : User
    {
        public override string Role => "Empleado";
        
        public Employee(string name, string username, string password) : base(name, username, password) { }
        private Employee() : base() { }
        
        public override List<string> GetAllowedActions() =>
        [
            "1. 🔍 Historial de Ventas",
            "2. 💵 Registrar Venta",
            "5. 📋 Ver Stock",
            "6. 🔍 Buscar Producto",
            "8. 🚨 Alertas Activas",
            "9. 📜 Historial de Alertas",
            "0. 🚪 Salir"
        ];
    }
}