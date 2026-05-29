using Farmacontrol.Core.Interface;
using Farmacontrol.Model;

namespace Farmacontrol.Core.Model.ProductEntity
{
    public class Supply : Product, IAlertable, IExpirable
    {
        public string ProductType => "Suministro";
        public string Brand { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Size { get; set; }
        public string? Material { get; set; }
        public DateTime ExpirationDate 
        {
            get => Batches.Where(b => b.Quantity > 0).OrderBy(b => b.ExpirationDate).FirstOrDefault()?.ExpirationDate ?? DateTime.MaxValue;
            set { }
        }

        public bool IsSterile { get; set; }

        public bool IsExpired() => ExpirationDate < DateTime.Today;

        public int ExpiresIn() => (ExpirationDate - DateTime.Today).Days;

        public override string GetDescription() =>
            $"Marca: {Brand}, Tipo: {Type}" +
            $"{(Size != null ? $", Tamaño: {Size}" : "")}" +
            $"{(Material != null ? $", Material: {Material}" : "")}" +
            $", Estéril: {(IsSterile ? "Sí" : "No")}" +
            $", Fecha de Expiración: {ExpirationDate:dd/MM/yyyy}";

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