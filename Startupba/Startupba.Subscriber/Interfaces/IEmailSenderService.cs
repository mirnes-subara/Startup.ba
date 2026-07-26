using System.Threading.Tasks;

namespace Startupba.Subscriber.Interfaces
{
    public interface IEmailSenderService
    {
        Task SendEmailAsync(string email, string subject, string message);
        Task SendHtmlEmailAsync(string email, string subject, string htmlBody);
    }
}
