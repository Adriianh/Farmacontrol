using System;
using System.Collections.Generic;
using System.Linq;

namespace Farmacontrol.Model
{
    public class Sale
    {
        private readonly List<SaleDetail> _details = new();
        
        public int Code { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public decimal Total { get; set; }

        // Nuevos atributos de venta de farmacia
        public string? ClientName { get; set; }
        public string? DoctorLicense { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        public Sale(int code)
        {
            Code = code;
        }

        private Sale() { }

        public IReadOnlyList<SaleDetail> GetDetails => _details.AsReadOnly();

        public List<SaleDetail> Details => _details;

        public void AddDetail(Product product, int quantity)
        {
            product.UpdateStock(-quantity);
            _details.Add(new SaleDetail(product, quantity));
            Total = CalculateTotal();
        }

        private decimal CalculateTotal() => _details.Sum(detail => detail.Subtotal);

        private string GetPaymentMethodName() => PaymentMethod switch
        {
            PaymentMethod.Cash => "Efectivo",
            PaymentMethod.CreditCard => "Tarjeta de Crédito",
            PaymentMethod.DebitCard => "Tarjeta de Débito",
            PaymentMethod.Transfer => "Transferencia Bancaria",
            _ => PaymentMethod.ToString()
        };

        public void ShowResume()
        {
            Console.WriteLine($"Venta #{Code} - {Date:dd/MM/yyyy HH:mm:ss}");
            if (!string.IsNullOrEmpty(ClientName)) Console.WriteLine($"Cliente: {ClientName}");
            if (!string.IsNullOrEmpty(DoctorLicense)) Console.WriteLine($"Cédula de Médico: {DoctorLicense}");
            Console.WriteLine($"Método de Pago: {GetPaymentMethodName()}");
            Console.WriteLine("------------------");

            foreach (var detail in _details)
            {
                detail.ShowDetails();
            }

            Console.WriteLine("------------------");
            Console.WriteLine($"Total: Q{Total:F2}");
        }
    }
}