using System.Net;
using System.Net.Mail;

namespace VontaGenta.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _config;

        public EmailService(ILogger<EmailService> logger,
            IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }


        public bool SendMail(string toEmail, string subject, string body)
        {

            try
            {
                var from = _config["Emails:FromEmail"] ?? throw new ArgumentNullException("From email is null");

                MailMessage message = new MailMessage(from, toEmail, subject, body);

                var port = _config["AWS:SES_SMTP_PORT"] ?? throw new ArgumentNullException("Port is null");

                // https://docs.aws.amazon.com/ses/latest/DeveloperGuide/smtp-connect.html
                int aws_ses_smtp_port = int.Parse(port);
                var aws_ses_smtp_host = _config["AWS:SES_SMTP_ENDPOINT"];
                using var objSmtpClient = new SmtpClient(aws_ses_smtp_host, aws_ses_smtp_port);
                objSmtpClient.Credentials = new NetworkCredential(_config["AWS:SES_SMTP_USERNAME"], _config["AWS:SES_SMTP_PASSWORD"]);
                objSmtpClient.EnableSsl = true;
                objSmtpClient.Send(message);

            }
            catch (Exception ex)
            {
                while (ex.InnerException != null) { ex = ex.InnerException; }
                _logger.LogError("{Message}", ex.Message);
                return false;
            }

            return true;
        }
    }
}
