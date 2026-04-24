namespace Farmacontrol.Model.UserEntity
{
    public class Employee(string name, string username, string password) : User(name, username, password)
    {
        public override string Role => "Empleado";

        public override List<string> GetAllowedActions() => new()
        {
            "1. Registrar venta",
            "3. Buscar producto",
            "4. Ver alertas",
            "0. Salir"
        };
    }
}