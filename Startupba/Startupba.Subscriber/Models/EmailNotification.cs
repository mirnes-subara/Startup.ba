namespace Startupba.Subscriber.Models
{
    /// <summary>
    /// Message published to RabbitMQ whenever an email should be sent.
    /// </summary>
    public class EmailNotification
    {
        public EmailNotificationDto Notification { get; set; } = null!;
    }
}
