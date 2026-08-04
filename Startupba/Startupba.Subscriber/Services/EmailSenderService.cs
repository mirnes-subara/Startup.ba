using System.Net;
using System.Net.Mail;
using Startupba.Subscriber.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Startupba.Subscriber.Services
{
    public class EmailSenderService : IEmailSenderService
    {
        private readonly string _smtpEmail;
        private readonly string _smtpPassword;
        private const string FromDisplayName = "Startup.ba";

        public EmailSenderService(IConfiguration configuration)
        {
            _smtpEmail = configuration["SMTP:Email"] ?? throw new InvalidOperationException("SMTP:Email is not set (set SMTP__EMAIL in .env / environment).");
            _smtpPassword = configuration["SMTP:Password"] ?? throw new InvalidOperationException("SMTP:Password is not set (set SMTP__PASSWORD in .env / environment).");
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = CreateClient();
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpEmail, FromDisplayName),
                Subject = subject,
                Body = message,
                IsBodyHtml = false,
            };
            mailMessage.To.Add(email);
            return client.SendMailAsync(mailMessage);
        }

        public Task SendHtmlEmailAsync(string email, string subject, string htmlBody)
        {
            var client = CreateClient();
            var mailMessage = new MailMessage
            {
                From = new MailAddress(_smtpEmail, FromDisplayName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(email);
            return client.SendMailAsync(mailMessage);
        }

        private SmtpClient CreateClient() => new("smtp.gmail.com", 587)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_smtpEmail, _smtpPassword),
        };
    }
}
