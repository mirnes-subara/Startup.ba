using EasyNetQ;
using Microsoft.Extensions.Logging;
using Startupba.Services.Interfaces;
using Startupba.Subscriber.Models;
using System;
using System.Threading.Tasks;

namespace Startupba.Services.Helpers
{
    /// <summary>
    /// Publishes email notification messages to RabbitMQ using a singleton IBus connection.
    /// Connection settings are read once from environment variables at construction time.
    /// Registered as Singleton in DI so the connection is reused across the application lifetime.
    /// </summary>
    public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
    {
        private readonly IBus _bus;
        private readonly ILogger<RabbitMqPublisher> _logger;

        public RabbitMqPublisher(ILogger<RabbitMqPublisher> logger)
        {
            _logger = logger;

            var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
            var username = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "guest";
            var password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest";
            var virtualhost = Environment.GetEnvironmentVariable("RABBITMQ_VIRTUALHOST") ?? "/";

            _bus = RabbitHutch.CreateBus($"host={host};virtualHost={virtualhost};username={username};password={password}");
        }

        public async Task PublishEmailAsync(EmailNotificationDto notification)
        {
            try
            {
                await _bus.PubSub.PublishAsync(new EmailNotification { Notification = notification });
            }
            catch (Exception ex)
            {
                // Log error but don't throw - email failure shouldn't break the main operation
                _logger.LogError(ex, "Failed to publish email notification");
            }
        }

        public void Dispose()
        {
            _bus?.Dispose();
        }
    }
}
