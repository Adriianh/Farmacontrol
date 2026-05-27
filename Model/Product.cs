namespace Farmacontrol.Model
{
    public abstract class Product
    {
        public string SupplierCode { get; init; }

        public string Name { get; init; }

        public string Code { get; init; }

        public decimal Price { get; init; }

        public int Stock { get; set; }

        public int MinimumStock { get; init; }
        

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