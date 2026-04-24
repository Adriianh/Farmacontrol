using Farmacontrol.Model;
using Farmacontrol.UI.Helper;

namespace Farmacontrol.UI.View
{
    public class ReportsView(Report report)
    {
        public void ShowReportsMenu()
        {
            ConsoleHelper.ShowTitle("Reportes");
            Console.WriteLine("1. Ventas del día");
            Console.WriteLine("2. Ventas del mes");
            Console.WriteLine("3. Productos más vendidos");

            string option = ConsoleHelper.ReadText("\nSeleccione una opción (o 'fin' para cancelar): ");
            if (option.ToLower() == "fin") return;
            switch (option)
            {
                case "1":
                    if (!report.HasSales)
                        Console.WriteLine("No hay ventas registradas.");
                    else
                        report.GenerateDailySales();
                    break;
                case "2":
                    if (!report.HasSales)
                        Console.WriteLine("No hay ventas registradas.");
                    else
                        report.GenerateMonthSales();
                    break;
                case "3":
                    if (!report.HasSales)
                        Console.WriteLine("No hay ventas registradas.");
                    else
                        report.BestSellingProducts();
                    break;
            }

            ConsoleHelper.Pause();
        }
    }
}