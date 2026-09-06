using EasyNetQ;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Startupba.Subscriber.Interfaces;
using Startupba.Subscriber.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Startupba.Subscriber.Services
{
    public class BackgroundWorkerService : BackgroundService
    {
        private readonly ILogger<BackgroundWorkerService> _logger;
        private readonly IEmailSenderService _emailSender;
        private readonly string _host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
        private readonly string _username = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "guest";
        private readonly string _password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest";
        private readonly string _virtualhost = Environment.GetEnvironmentVariable("RABBITMQ_VIRTUALHOST") ?? "/";

        public BackgroundWorkerService(
            ILogger<BackgroundWorkerService> logger,
            IEmailSenderService emailSender)
        {
            _logger = logger;
            _emailSender = emailSender;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await CheckSmtpConnectivityAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                IBus? bus = null;
                IDisposable? subscription = null;
                try
                {
                    bus = RabbitHutch.CreateBus(
                        $"host={_host};virtualHost={_virtualhost};username={_username};password={_password}");
                    subscription = bus.PubSub.Subscribe<EmailNotification>(
                        "Startupba_Notifications",
                        HandleEmailMessage);

                    _logger.LogInformation(
                        "Subscribed to email notifications on {Host}. Listening until shutdown.",
                        _host);

                    await Task.Delay(Timeout.Infinite, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "RabbitMQ listener failed; reconnecting in 5 seconds.");
                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    }
                    catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                    {
                        break;
                    }
                }
                finally
                {
                    subscription?.Dispose();
                    bus?.Dispose();
                }
            }
        }

        private async Task HandleEmailMessage(EmailNotification message)
        {
            var notification = message.Notification;

            if (string.IsNullOrWhiteSpace(notification?.NotificationType))
            {
                _logger.LogWarning("No notification type provided in the email notification");
                return;
            }

            if (string.IsNullOrWhiteSpace(notification.RecipientEmail))
            {
                _logger.LogWarning(
                    "No recipient email found for notification type: {Type}",
                    notification.NotificationType);
                return;
            }

            var htmlBody = EmailTemplateService.GenerateEmail(notification);

            var subject = notification.NotificationType switch
            {
                "StartupApproved" => $"Startup Approved - {notification.StartupName}",
                "StartupRejected" => $"Startup Rejected - {notification.StartupName}",
                "DonationReceived" => $"New Donation Received - {notification.StartupName}",
                "TicketAnswered" => $"Support Ticket Answered - {notification.TicketSubject}",
                _ => "Startup.ba Notification"
            };

            try
            {
                await _emailSender.SendHtmlEmailAsync(notification.RecipientEmail, subject, htmlBody);
                _logger.LogInformation(
                    "Email notification ({Type}) sent to: {Email}",
                    notification.NotificationType,
                    notification.RecipientEmail);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to send email notification ({Type}) to {Email} after retries",
                    notification.NotificationType,
                    notification.RecipientEmail);
                throw;
            }
        }

        private async Task CheckSmtpConnectivityAsync(CancellationToken stoppingToken)
        {
            try
            {
                var addresses = await Dns.GetHostAddressesAsync("smtp.gmail.com");
                _logger.LogInformation(
                    "smtp.gmail.com resolved to: {Addresses}",
                    string.Join(", ", addresses.Select(a => a.ToString())));
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync("smtp.gmail.com", 587);
                var completed = await Task.WhenAny(connectTask, Task.Delay(5000, stoppingToken));
                if (completed == connectTask && client.Connected)
                    _logger.LogInformation("Successfully connected to smtp.gmail.com:587");
                else
                    _logger.LogError("Failed to connect to smtp.gmail.com:587 (timeout or error)");
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Internet connectivity check failed");
            }
        }
    }
}
