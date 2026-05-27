using System;
using System.ComponentModel;
using Farmacontrol.Interface;
using System.Linq;
using Farmacontrol.Util;

namespace Farmacontrol.Model.ProductEntity
{
    public class Supplement : Product, IAlertable, IExpirable
    {
        public string ProductType => "Suplemento";
        public string ActivePrinciple { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public SupplementFormat Format { get; set; }
        public DateTime ExpirationDate 
        {
            get => Batches.Where(b => b.Quantity > 0).OrderBy(b => b.ExpirationDate).FirstOrDefault()?.ExpirationDate ?? DateTime.MaxValue;
            set { }
        }

        public string? Concentration { get; set; }
        public string? RecommendedDosage { get; set; }

        public bool IsExpired() => ExpirationDate < DateTime.Today;

        public int ExpiresIn() => (ExpirationDate - DateTime.Today).Days;

        public override string GetDescription() =>
            $"Principio Activo: {ActivePrinciple}, Tipo: {Type}, Formato: {Format.GetDescription()}" +
            $"{(Concentration != null ? $", Concentración: {Concentration}" : "")}" +
            $"{(RecommendedDosage != null ? $", Dosis Recomendada: {RecommendedDosage}" : "")}" +
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