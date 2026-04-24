using System.ComponentModel;
using Farmacontrol.Interface;
using Farmacontrol.Util;

namespace Farmacontrol.Model.ProductEntity
{
    public class Supplement : Product, IAlertable, IExpirable
    {
        public string ActivePrinciple { get; set; }
        public string Type { get; set; }
        public SupplementFormat Format { get; set; }
        public DateTime ExpirationDate { get; set; }

        public bool IsExpired() => ExpirationDate < DateTime.Today;

        public int ExpiresIn() => (ExpirationDate - DateTime.Today).Days;
        
        public override string GetDescription() =>
            $"Principio Activo: {ActivePrinciple}, Tipo: {Type}, Formato: {Format.GetDescription()}";

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

public enum SupplementFormat
{
    [Description ("Cápsulas")]
    Capsules,
    [Description ("Tabletas")]
    Tablets,
    [Description ("Polvo")]
    Powder,
    [Description ("Líquido")]
    Liquid,
    [Description ("Gomitas")]
    Gummies
}