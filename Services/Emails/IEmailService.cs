namespace VontaGenta.Services
{
    public interface IEmailService
    {
        bool SendMail(string toEmail, string subject, string body);
    }
}