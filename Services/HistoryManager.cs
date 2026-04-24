using Farmacontrol.Interface;
using Farmacontrol.Model;

namespace Farmacontrol.Services
{
    public class HistoryManager
    {
        List<Alert> _history = new();

        public void VerifyAlert(List<Product> products)
        {
            foreach (var product in products)
            {
                if (product is not IAlertable) continue;

                if (product.IsStockLow())
                    RegisterAlert(new Alert(
                        "STOCK BAJO",
                        product.Code,
                        product.Name,
                        $"El producto {product.Name} tiene un stock bajo. Stock actual {product.Stock} | Stock Mínimo {product.MinimumStock}"
                    ));

                if (product is IExpirable expirable && expirable.IsExpired())
                    RegisterAlert(new Alert(
                        "PRODUCTO VENCIDO",
                        product.Code,
                        product.Name,
                        $"El producto {product.Name} está vencido. Fecha de vencimiento: {expirable.ExpirationDate:dd/MM/yyyy}"
                    ));
            }
        }

        public void ShowHistory()
        {
            if (_history.Count == 0)
            {
                Console.WriteLine("No hay alertas registradas.");
                return;
            }
            
            _history
                .OrderByDescending(alert => alert.Date)
                .ToList()
                .ForEach(alert => alert.ShowAlert());
        }
        
        public void ShowTodayAlerts()
        {
            List<Alert> recentAlerts = _history
                .Where(alert => alert.Date.Date == DateTime.Today)
                .OrderByDescending(alert => alert.Date)
                .Take(5)
                .ToList();

            if (recentAlerts.Count == 0)
            {
                Console.WriteLine("No hay alertas registradas hoy.");
                return;
            }
        
            recentAlerts.ForEach(alert => alert.ShowAlert());
        }
        
        public IReadOnlyList<Alert> GetHistory() => _history.AsReadOnly();
        
        private void RegisterAlert(Alert alert)
        {
            bool alreadyRegistered = _history.Any(a =>
                a.ProductCode == alert.ProductCode &&
                a.Type == alert.Type &&
                a.Date.Date == alert.Date.Date
            );

            if (!alreadyRegistered)
                _history.Add(alert);
        }
    }
}