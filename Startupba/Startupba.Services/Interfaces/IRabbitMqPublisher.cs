using Startupba.Subscriber.Models;
using System.Threading.Tasks;

namespace Startupba.Services.Interfaces
{
    /// <summary>
    /// Publishes email notification messages to RabbitMQ.
    /// </summary>
    public interface IRabbitMqPublisher
    {
        Task PublishEmailAsync(EmailNotificationDto notification);
    }
}
