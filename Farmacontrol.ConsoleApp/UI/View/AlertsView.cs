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
            historyService.ShowTodayAlerts();

            ConsoleHelper.Pause();
        }

        public void ShowHistory()
        {
            ConsoleHelper.ShowTitle("Historial de Alertas");

            historyService.ShowHistory();

            ConsoleHelper.Pause();
        }
    }
}