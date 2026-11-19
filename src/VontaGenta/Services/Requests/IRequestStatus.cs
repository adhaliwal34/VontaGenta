namespace VontaGenta.Services
{
    public interface IRequestStatus
    {
        string AppError { get; set; }
        string Status { get; set; }
    }
}
