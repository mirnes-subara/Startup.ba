using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Startupba.Subscriber.Interfaces;

namespace Startupba.Subscriber.Services
{
    public class EmailSenderService : IEmailSenderService
    {
        private const int MaxAttempts = 3;
        private const string FromDisplayName = "Startup.ba";

        private readonly string _smtpEmail;
        private readonly string _smtpPassword;
        private readonly ILogger<EmailSenderService> _logger;

        public EmailSenderService(IConfiguration configuration, ILogger<EmailSenderService> logger)
        {
            _smtpEmail = configuration["SMTP:Email"] ?? throw new InvalidOperationException("SMTP:Email is not set (set SMTP__EMAIL in .env / environment).");
            _smtpPassword = configuration["SMTP:Password"] ?? throw new InvalidOperationException("SMTP:Password is not set (set SMTP__PASSWORD in .env / environment).");
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string message)
            => SendWithRetryAsync(email, subject, message, isHtml: false);

        public Task SendHtmlEmailAsync(string email, string subject, string htmlBody)
            => SendWithRetryAsync(email, subject, htmlBody, isHtml: true);

        private async Task SendWithRetryAsync(string email, string subject, string body, bool isHtml)
        {
            Exception? last = null;
            for (var attempt = 1; attempt <= MaxAttempts; attempt++)
            {
                try
                {
                    using var client = CreateClient();
                    using var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_smtpEmail, FromDisplayName),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = isHtml,
                    };
                    mailMessage.To.Add(email);
                    await client.SendMailAsync(mailMessage);
                    return;
                }
                catch (Exception ex)
                {
                    last = ex;
                    if (attempt == MaxAttempts)
                        break;

                    var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt - 1));
                    _logger.LogWarning(
                        ex,
                        "SMTP send failed (attempt {Attempt}/{Max}); retrying in {Delay}s",
                        attempt,
                        MaxAttempts,
                        delay.TotalSeconds);
                    await Task.Delay(delay);
                }
            }

            throw last ?? new InvalidOperationException("SMTP send failed.");
        }

        private SmtpClient CreateClient() => new("smtp.gmail.com", 587)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_smtpEmail, _smtpPassword),
        };
    }
}
