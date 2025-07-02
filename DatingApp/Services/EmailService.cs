using DatingApp.Interfaces;
using SendGrid.Helpers.Mail;
using SendGrid;

namespace DatingApp.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }
        public async Task SendEmailAsync(string toEmail, string subject, string message)
        {
            var apiKey = _config["SendGrid:ApiKey"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_config["SendGrid:FromEmail"], _config["SendGrid:FromName"]);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, message, message);
            await client.SendEmailAsync(msg);
        }

    }
}
