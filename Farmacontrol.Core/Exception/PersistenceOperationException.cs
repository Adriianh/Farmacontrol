namespace Farmacontrol.Exception
{
    public class PersistenceOperationException : System.Exception
    {
        public PersistenceOperationException(string message, System.Exception inner)
            : base(message, inner)
        {
        }
    }
}
