namespace Farmacontrol.Model.UserEntity
{
    public class Employee : User
    {
        public override string Role => "Empleado";
        
        public Employee(string name, string username, string password) : base(name, username, password) { }
        private Employee() : base() { }
        
        public override List<string> GetAllowedActions() =>
        [
            "1. Registrar venta",
            "3. Buscar producto",
            "4. Ver alertas",
            "5. Ver el historial de alertas",
            "7. Ver medicamentos vencidos",
            "0. Salir"
        ];
    }
}