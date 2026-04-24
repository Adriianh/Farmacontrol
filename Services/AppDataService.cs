using Farmacontrol.Model;

namespace Farmacontrol.Services
{
    public class AppDataService(Persistence.Persistence persistence)
    {
        public AppState Load()
        {
            List<Sale> savedSales = persistence.LoadSales();
            var state = new AppState(savedSales);

            List<User> savedUsers = persistence.LoadUsers();
            if (savedUsers.Count > 0)
                savedUsers.ForEach(user => state.UserManager.AddUser(user));

            List<Product> savedProducts = persistence.LoadProducts();
            if (savedProducts.Count > 0)
                savedProducts.ForEach(product => state.Inventory.AddProduct(product));

            List<Supplier> savedSuppliers = persistence.LoadSuppliers();
            if (savedSuppliers.Count > 0)
                savedSuppliers.ForEach(supplier => state.SupplierManager.AddSupplier(supplier));

            List<Alert> savedHistory = persistence.LoadHistory();
            if (savedHistory.Count > 0)
                state.HistoryManager.LoadHistory(savedHistory);

            return state;
        }

        public void Save(AppState state)
        {
            persistence.SaveProducts(state.Inventory.GetProducts.ToList());
            persistence.SaveSales(state.Sales);
            persistence.SaveUsers(state.UserManager.GetAllUsers().ToList());
            persistence.SaveSuppliers(state.SupplierManager.GetSuppliers().ToList());
            persistence.SaveHistory(state.HistoryManager.GetHistory().ToList());
        }
    }
}