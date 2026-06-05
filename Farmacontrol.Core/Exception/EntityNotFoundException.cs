namespace Farmacontrol.Core.Exception
{
    public class EntityNotFoundException : System.Exception
    {
        public EntityNotFoundException(string entityName, string identifier)
            : base($"No se encontró la entidad {entityName} con identificador {identifier}.")
        {
        }
    }
}