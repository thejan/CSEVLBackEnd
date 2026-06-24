using CSEVirtualLabWebAPI.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace CSEVirtualLabWebAPI.Services
{
    public class RegistrationEmailService
    {
        private readonly SmtpSettings settings;
        private readonly ILogger<RegistrationEmailService> logger;

        public RegistrationEmailService(
            IOptions<SmtpSettings> options,
            ILogger<RegistrationEmailService> logger)
        {
            settings = options.Value;
            this.logger = logger;
        }

        public async Task<bool> SendApprovalEmailAsync(
            string recipientEmail)
        {
            if (
                string.IsNullOrWhiteSpace(recipientEmail) ||
                string.IsNullOrWhiteSpace(settings.Host) ||
                string.IsNullOrWhiteSpace(settings.FromEmail))
            {
                logger.LogWarning(
                    "Approval email was not sent because SMTP settings or the recipient email are missing.");

                return false;
            }

            try
            {
                using var message = new MailMessage
                {
                    From = new MailAddress(
                        settings.FromEmail,
                        settings.FromName),
                    Subject =
                        "Virtual Lab Registration Approved",
                    Body =
                        "Dear User,\r\n\r\n" +
                        "Your registration has been approved. " +
                        "You may login.\r\n\r\n" +
                        "Regards,\r\n" +
                        "Admin,\r\n" +
                        "CSE VirtualLabs\r\n" +
                        "ATMECE, Mysuru",
                    IsBodyHtml = false
                };

                message.To.Add(recipientEmail);

                using var client = new SmtpClient(
                    settings.Host,
                    settings.Port)
                {
                    EnableSsl = settings.EnableSsl,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(
                        settings.Username,
                        settings.Password)
                };

                await client.SendMailAsync(message);

                return true;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Unable to send registration approval email to {RecipientEmail}.",
                    recipientEmail);

                return false;
            }
        }
    }
}
