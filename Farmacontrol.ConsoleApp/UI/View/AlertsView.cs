using Farmacontrol.ConsoleApp.UI.Helper;
using Farmacontrol.Core.Services;

namespace Farmacontrol.ConsoleApp.UI.View
{
    public class AlertsView(HistoryManager historyManager, Inventory inventory)
    {
        public void ShowTodayAlerts()
        {
            ConsoleHelper.ShowTitle("Alertas de Hoy");

            if (!inventory.GetProducts.Any())
            {
                Console.WriteLine("No hay productos en inventario.");
                ConsoleHelper.Pause();
                return;
            }

            historyManager.VerifyAlert(inventory.GetProducts.ToList());
            historyManager.ShowTodayAlerts();

            ConsoleHelper.Pause();
        }

        public void ShowHistory()
        {
            ConsoleHelper.ShowTitle("Historial de Alertas");

            historyManager.ShowHistory();

            ConsoleHelper.Pause();
        }
    }
}