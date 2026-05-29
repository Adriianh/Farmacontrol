namespace Farmacontrol.Core.Exception
{
    public class InsufficientStockException : System.Exception
    {
        public InsufficientStockException(string productName, int requested, int available)
            : base($"Stock insuficiente de '{productName}'. Solicitado: {requested}, Disponible: {available}.")
        {
        }
    }
}
