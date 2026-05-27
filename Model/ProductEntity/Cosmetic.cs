using Farmacontrol.Interface;

namespace Farmacontrol.Model.ProductEntity
{
    public class Cosmetic : Product, IAlertable, IExpirable
    {
        public string ProductType => "Cosmetico";
        public string Brand { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }

        public string? Presentation { get; set; }
        public bool Hypoallergenic { get; set; }

        public bool IsExpired() => ExpirationDate < DateTime.Today;

        public int ExpiresIn() => (ExpirationDate - DateTime.Today).Days;

        public override string GetDescription() =>
            $"Marca: {Brand}, Tipo: {Type}" +
            $"{(Presentation != null ? $", Presentación: {Presentation}" : "")}" +
            $", Hipoalergénico: {(Hypoallergenic ? "Sí" : "No")}" +
            $", Fecha de Expiración: {ExpirationDate.ToShortDateString()}";

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