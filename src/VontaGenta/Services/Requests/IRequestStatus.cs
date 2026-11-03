namespace VontaGenta.Services
{
    public interface IRequestStatus
    {
        string Error { get; set; }
        string Status { get; set; }
    }
}
