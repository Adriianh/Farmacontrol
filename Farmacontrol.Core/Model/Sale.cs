using Farmacontrol.Model;

namespace Farmacontrol.Core.Model
{
    public class Sale
    {
        private readonly List<SaleDetail> _details = new();

        public int Code { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public decimal Total { get; set; }

        public string? ClientName { get; set; }
        public string? DoctorLicense { get; set; }
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Cash;

        public bool IsVoided { get; set; }
        public string? VoidReason { get; set; }
        public string? VoidDetails { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal TaxAmount { get; set; }

        public string? InvoiceNumber { get; set; }
        public string? Notes { get; set; }
        public string Status { get; set; } = "Completada";

        public Prescription? Prescription { get; set; }

        public Sale(int code)
        {
            Code = code;
        }

        private Sale()
        {
        }

        public IReadOnlyList<SaleDetail> GetDetails => _details.AsReadOnly();

        public List<SaleDetail> Details => _details;

        public void AddDetail(Product product, int quantity)
        {
            _details.Add(new SaleDetail(product, quantity));
            RecalculateTotal();
        }

        public void RecalculateTotal()
        {
            decimal subtotal = _details.Sum(detail => detail.Subtotal);
            decimal discount = subtotal * (DiscountPercentage / 100);
            Total = subtotal - discount + TaxAmount;
        }

        private decimal CalculateTotal()
        {
            decimal subtotal = _details.Sum(detail => detail.Subtotal);
            decimal discount = subtotal * (DiscountPercentage / 100);
            return subtotal - discount + TaxAmount;
        }

        public void VoidSale(string reason, string details)
        {
            if (IsVoided) return;
            IsVoided = true;
            VoidReason = reason;
            VoidDetails = details;
            Status = "Anulada";
            Total = 0;
        }

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
            Console.WriteLine($"Venta #{Code} - {Date:dd/MM/yyyy HH:mm:ss} {(IsVoided ? "[ANULADA]" : "")}");
            if (!string.IsNullOrEmpty(ClientName)) Console.WriteLine($"Cliente: {ClientName}");
            if (!string.IsNullOrEmpty(DoctorLicense)) Console.WriteLine($"Cédula de Médico: {DoctorLicense}");
            Console.WriteLine($"Método de Pago: {GetPaymentMethodName()}");
            Console.WriteLine("------------------");

            foreach (var detail in _details)
            {
                detail.ShowDetails();
            }

            if (DiscountPercentage > 0) Console.WriteLine($"Descuento: {DiscountPercentage}%");
            if (TaxAmount > 0) Console.WriteLine($"Impuestos: Q{TaxAmount:F2}");

            Console.WriteLine("------------------");
            Console.WriteLine($"Total: Q{Total:F2}");
        }
    }
}