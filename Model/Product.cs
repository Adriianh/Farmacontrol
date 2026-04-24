namespace Farmacontrol.Model
{
    public abstract class Product
    {
        private string _name;
        private string _code;
        private decimal _price;
        private int _stock;
        private int _minimumStock;
        
        public string SupplierCode { get; set; }

        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public string Code
        {
            get => _code;
            set => _code = value;
        }

        public decimal Price
        {
            get => _price;
            set => _price = value;
        }

        public int Stock
        {
            get => _stock;
            set => _stock = value;
        }

        public int MinimumStock
        {
            get => _minimumStock;
            set => _minimumStock = value;
        }

        public abstract string GetDescription();

        public bool IsStockLow()
        {
            return Stock < MinimumStock;
        }

        public void UpdateStock(int stock)
        {
            Stock = stock;
        }

        public void ShowInformation()
        {
            Console.WriteLine($"Name: {Name}");
            Console.WriteLine($"Code: {Code}");
            Console.WriteLine($"Price: {Price}");
            Console.WriteLine($"Stock: {Stock}");
            Console.WriteLine(GetDescription());
        }
    }
}