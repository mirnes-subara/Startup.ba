using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using System.Threading.Tasks;

namespace Startupba.Services.Interfaces
{
    public interface ISupportTicketService : ICRUDService<SupportTicketResponse, SupportTicketSearchObject, SupportTicketUpsertRequest, SupportTicketUpsertRequest>
    {
        /// <summary>
        /// Admin answers a ticket: stores the response, marks the ticket Answered
        /// and notifies the user (in-app + email).
        /// </summary>
        Task<SupportTicketResponse?> AnswerAsync(int id, SupportTicketAnswerRequest request);

        Task<SupportTicketResponse?> CloseAsync(int id);
    }
}
