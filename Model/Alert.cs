namespace Farmacontrol.Model
{
    public class Alert(string type, string productCode, string productName, string description)
    {
        public string Type { get; set; } = type;
        public string ProductCode { get; set; } = productCode;
        public string ProductName { get; set; } = productName;
        public string Description { get; set; } = description;
        public DateTime Date { get; set; } = DateTime.Now;

        public void ShowAlert() =>
            Console.WriteLine($"[{Date:dd/MM/yyyy HH:mm:ss} {Type} - {ProductName} ({ProductCode} - {Description}");
    }
}