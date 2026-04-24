using Farmacontrol.Model;

namespace Farmacontrol.Services
{
    public class AppState
    {
        public Inventory Inventory { get; } = new();
        public UserManager UserManager { get; } = new();
        public SupplierManager SupplierManager { get; } = new();
        public HistoryManager HistoryManager { get; } = new();
        public List<Sale> Sales { get; }

        public Report Report { get; }

        public int SalesCount { get; }

        public AppState(List<Sale> sales)
        {
            Sales = sales;
            Report = new Report(Sales);

            SalesCount = Sales.Count > 0
                ? Sales.Max(sale => sale.Code)
                : 0;
        }
    }
}