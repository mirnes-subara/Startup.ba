using EasyNetQ;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Startupba.Subscriber.Models;
using Startupba.Subscriber.Interfaces;
using System.Net.Sockets;
using System.Net;

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
            // Check internet connectivity to smtp.gmail.com
            try
            {
                var addresses = await Dns.GetHostAddressesAsync("smtp.gmail.com");
                _logger.LogInformation($"smtp.gmail.com resolved to: {string.Join(", ", addresses.Select(a => a.ToString()))}");
                using (var client = new TcpClient())
                {
                    var connectTask = client.ConnectAsync("smtp.gmail.com", 587);
                    var timeoutTask = Task.Delay(5000, stoppingToken);
                    var completed = await Task.WhenAny(connectTask, timeoutTask);
                    if (completed == connectTask && client.Connected)
                    {
                        _logger.LogInformation("Successfully connected to smtp.gmail.com:587");
                    }
                    else
                    {
                        _logger.LogError("Failed to connect to smtp.gmail.com:587 (timeout or error)");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Internet connectivity check failed: {ex.Message}");
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var bus = RabbitHutch.CreateBus($"host={_host};virtualHost={_virtualhost};username={_username};password={_password}"))
                    {
                        // Subscribe to email notifications
                        bus.PubSub.Subscribe<EmailNotification>("Startupba_Notifications", HandleEmailMessage);

                        _logger.LogInformation("Waiting for email notifications...");
                        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                    }
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error in RabbitMQ listener: {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
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
                _logger.LogWarning($"No recipient email found for notification type: {notification.NotificationType}");
                return;
            }

            try
            {
                // Generate HTML email based on notification type
                var htmlBody = EmailTemplateService.GenerateEmail(notification);

                var subject = notification.NotificationType switch
                {
                    "StartupApproved" => $"Startup Approved - {notification.StartupName}",
                    "StartupRejected" => $"Startup Rejected - {notification.StartupName}",
                    "DonationReceived" => $"New Donation Received - {notification.StartupName}",
                    "TicketAnswered" => $"Support Ticket Answered - {notification.TicketSubject}",
                    _ => "Startup.ba Notification"
                };

                await _emailSender.SendHtmlEmailAsync(notification.RecipientEmail, subject, htmlBody);
                _logger.LogInformation($"Email notification ({notification.NotificationType}) sent to: {notification.RecipientEmail}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email notification: {ex.Message}");
            }
        }
    }
}
