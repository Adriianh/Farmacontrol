using Farmacontrol.Core.Interface;
using Farmacontrol.Model;

namespace Farmacontrol.Core.Model.ProductEntity
{
    public class Medicine : Product, IAlertable, IExpirable
    {
        public string ProductType => "Medicamento";
        public string ActivePrinciple { get; set; } = string.Empty;
        public DateTime ExpirationDate 
        {
            get => Batches.Where(b => b.Quantity > 0).OrderBy(b => b.ExpirationDate).FirstOrDefault()?.ExpirationDate ?? DateTime.MaxValue;
            set { }
        }
        public bool RequiresPrescription { get; set; }

        public string? Concentration { get; set; }
        public string? Presentation { get; set; }
        public bool IsControlled { get; set; }

        public bool IsExpired() => ExpirationDate < DateTime.Today;

        public int ExpiresIn() => (ExpirationDate - DateTime.Today).Days;

        public override string GetDescription() =>
            $"Principio Activo: {ActivePrinciple}" +
            $"{(Concentration != null ? $", Concentración: {Concentration}" : "")}" +
            $"{(Presentation != null ? $", Presentación: {Presentation}" : "")}" +
            $", Fecha de Expiración: {ExpirationDate.ToShortDateString()}" +
            $", Requiere Receta: {(RequiresPrescription ? "Sí" : "No")}" +
            $", Medicamento Controlado: {(IsControlled ? "Sí" : "No")}";

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