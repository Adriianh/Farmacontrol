using Farmacontrol.Core.Interface;
using Farmacontrol.Core.Model;
using Farmacontrol.Core.Repository;
using Farmacontrol.Model;
using Microsoft.EntityFrameworkCore;

namespace Farmacontrol.Core.Services
{
    public class HistoryManager
    {
        private readonly AppDbContext _db;

        public HistoryManager(AppDbContext db)
        {
            _db = db;
        }

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
                {
                    RegisterAlert(new Alert(
                        "PRODUCTO VENCIDO",
                        product.Code,
                        product.Name,
                        $"El producto {product.Name} está vencido. Fecha de vencimiento: {expirable.ExpirationDate:dd/MM/yyyy}"
                    ));
                }
                else if (product is IExpirable soonExpirable && !soonExpirable.IsExpired() && soonExpirable.ExpiresIn() <= 30 && product.Stock > 0)
                {
                    RegisterAlert(new Alert(
                        "PRODUCTO PRÓXIMO A VENCER",
                        product.Code,
                        product.Name,
                        $"El producto {product.Name} (Quedan {product.Stock} unidades) vencerá el {soonExpirable.ExpirationDate:dd/MM/yyyy}."
                    ));
                }
            }
        }

        public void ShowHistory()
        {
            var history = _db.Alerts.AsNoTracking().ToList();
            if (history.Count == 0)
            {
                Console.WriteLine("No hay alertas registradas.");
                return;
            }

            history
                .OrderByDescending(alert => alert.Date)
                .ToList()
                .ForEach(alert => alert.ShowAlert());
        }

        public void ShowTodayAlerts()
        {
            var history = _db.Alerts.AsNoTracking().ToList();
            List<Alert> recentAlerts = history
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

        public IReadOnlyList<Alert> GetHistory() => _db.Alerts.AsNoTracking().ToList().AsReadOnly();

        public void LoadHistory(IEnumerable<Alert> alerts)
        {
            foreach (Alert alert in alerts)
                RegisterAlert(alert);
        }

        private void RegisterAlert(Alert alert)
        {
            bool alreadyRegistered = _db.Alerts.Any(a =>
                a.ProductCode == alert.ProductCode &&
                a.Type == alert.Type &&
                a.Date.Year == alert.Date.Year &&
                a.Date.Month == alert.Date.Month &&
                a.Date.Day == alert.Date.Day
            );

            if (!alreadyRegistered)
            {
                _db.Alerts.Add(alert);
                _db.SaveChanges();
            }
        }
    }
}