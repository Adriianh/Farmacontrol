namespace Farmacontrol.Core.Model
{
    public class Alert
    {
        public string Type { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string Description { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        
        public Alert(string type, string productCode, string productName, string description)
        {
            Type = type;
            ProductCode = productCode;
            ProductName = productName;
            Description = description;
        }
        
        private Alert() { }
    
        public void ShowAlert() =>
            Console.WriteLine($"[{Date:dd/MM/yyyy HH:mm:ss}] {Type} - {ProductName} ({ProductCode}) - {Description}");
    }
}