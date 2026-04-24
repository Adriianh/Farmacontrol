using Farmacontrol.Interface;

namespace Farmacontrol.Model.ProductEntity
{
    public class Cosmetic : Product, IAlertable, IExpirable
    {
        public string ProductType => "Cosmetico";
        public string Brand { get; set; }
        public string Type { get; set; }
        public DateTime ExpirationDate { get; set; }
        
        public bool IsExpired() => ExpirationDate < DateTime.Today;

        public int ExpiresIn() => (ExpirationDate - DateTime.Today).Days;
        
        public override string GetDescription() =>
            $"Marca: {Brand}, Tipo: {Type}, Fecha de Expiración: {ExpirationDate}";

        public void VerifyAlert()
        {
            if (IsStockLow())
                Console.WriteLine($"ALERTA: Stock bajo en {Name}");
            if (IsExpired())
                Console.WriteLine($"ALERTA: {Name} está vencido");
            if ((ExpirationDate - DateTime.Today).Days <= 30)
                Console.WriteLine($"ALERTA: {Name} vence el {ExpirationDate:dd/MM/yyyy}");
        }
    }
}