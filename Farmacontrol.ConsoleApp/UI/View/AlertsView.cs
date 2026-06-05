using Farmacontrol.ConsoleApp.UI.Helper;
using Farmacontrol.Core.Services;

namespace Farmacontrol.ConsoleApp.UI.View
{
    public class AlertsView(HistoryService historyService, InventoryService inventoryService)
    {
        public void ShowTodayAlerts()
        {
            ConsoleHelper.ShowTitle("Alertas de Hoy");

            if (!inventoryService.GetProducts.Any())
            {
                Console.WriteLine("No hay productos en inventario.");
                ConsoleHelper.Pause();
                return;
            }

            historyService.VerifyAlert(inventoryService.GetProducts.ToList());
            
            var todayAlerts = historyService.GetHistory()
                .Where(a => a.Date.Date == DateTime.Today)
                .OrderByDescending(a => a.Date)
                .ToList();

            ConsoleHelper.PrintAlertsTable(todayAlerts);

            ConsoleHelper.Pause();
        }

        public void ShowHistory()
        {
            ConsoleHelper.ShowTitle("Historial de Alertas");

            var allAlerts = historyService.GetHistory()
                .OrderByDescending(a => a.Date)
                .ToList();

            ConsoleHelper.PrintAlertsTable(allAlerts);

            ConsoleHelper.Pause();
        }
    }
}