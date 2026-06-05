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
            "1. Registrar venta",
            "5. Buscar producto",
            "6. Ver medicamentos vencidos",
            "9. Ver alertas",
            "10. Ver el historial de alertas",
            "0. Salir"
        ];
    }
}