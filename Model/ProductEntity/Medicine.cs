using System;
using Farmacontrol.Interface;

namespace Farmacontrol.Model.ProductEntity
{
    public class Medicine : Product, IAlertable, IExpirable
    {
        public string ProductType => "Medicamento";
        public string ActivePrinciple { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; }
        public bool RequiresPrescription { get; set; }

        // Nuevos atributos específicos de medicamentos
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
            $", Requiere Receta: {RequiresPrescription}" +
            $", Medicamento Controlado: {IsControlled}";

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