using EasyNetQ;
using Startupba.Subscriber.Models;
using System;
using System.Threading.Tasks;

namespace Startupba.Services.Helpers
{
    /// <summary>
    /// Publishes email notification messages to RabbitMQ.
    /// Connection settings come from the same environment variables the template used
    /// (RABBITMQ_HOST / RABBITMQ_USERNAME / RABBITMQ_PASSWORD / RABBITMQ_VIRTUALHOST).
    /// </summary>
    public static class RabbitMqPublisher
    {
        public static async Task PublishEmailAsync(EmailNotificationDto notification)
        {
            try
            {
                var host = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
                var username = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "guest";
                var password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest";
                var virtualhost = Environment.GetEnvironmentVariable("RABBITMQ_VIRTUALHOST") ?? "/";

                using var bus = RabbitHutch.CreateBus($"host={host};virtualHost={virtualhost};username={username};password={password}");

                await bus.PubSub.PublishAsync(new EmailNotification { Notification = notification });
            }
            catch (Exception ex)
            {
                // Log error but don't throw - email failure shouldn't break the main operation
                Console.WriteLine($"Failed to publish email notification: {ex.Message}");
            }
        }
    }
}
