namespace Farmacontrol.Interface
{
    public interface IExpirable
    {
        DateTime ExpirationDate { get; set; }
        
        bool IsExpired();
        
        int ExpiresIn();
    }
}