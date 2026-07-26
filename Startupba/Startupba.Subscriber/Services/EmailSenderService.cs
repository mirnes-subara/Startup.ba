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

        public EmailSenderService(IConfiguration configuration)
        {
            _smtpEmail = configuration["SMTP:Email"] ?? throw new InvalidOperationException("SMTP:Email is not set (set SMTP__EMAIL in .env / environment).");
            _smtpPassword = configuration["SMTP:Password"] ?? throw new InvalidOperationException("SMTP:Password is not set (set SMTP__PASSWORD in .env / environment).");
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_smtpEmail, _smtpPassword)
            };

            return client.SendMailAsync(
                new MailMessage(from: _smtpEmail,
                              to: email,
                              subject,
                              message
                              ));
        }

        public Task SendHtmlEmailAsync(string email, string subject, string htmlBody)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_smtpEmail, _smtpPassword)
            };

            var mailMessage = new MailMessage(from: _smtpEmail, to: email, subject, htmlBody)
            {
                IsBodyHtml = true
            };

            return client.SendMailAsync(mailMessage);
        }
    }
}