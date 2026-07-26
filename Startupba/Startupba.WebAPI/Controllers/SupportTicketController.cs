using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Startupba.WebAPI.Controllers
{
    public class SupportTicketController : BaseCRUDController<SupportTicketResponse, SupportTicketSearchObject, SupportTicketUpsertRequest, SupportTicketUpsertRequest>
    {
        public SupportTicketController(ISupportTicketService service) : base(service)
        {
        }

        /// <summary>
        /// Admin answers a ticket (user gets an in-app notification and an email).
        /// </summary>
        [HttpPut("{id}/answer")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<SupportTicketResponse>> Answer(int id, [FromBody] SupportTicketAnswerRequest request)
        {
            var result = await ((ISupportTicketService)_service).AnswerAsync(id, request);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        /// <summary>
        /// Admin closes a ticket.
        /// </summary>
        [HttpPut("{id}/close")]
        [Authorize(Roles = "Administrator")]
        public async Task<ActionResult<SupportTicketResponse>> Close(int id)
        {
            var result = await ((ISupportTicketService)_service).CloseAsync(id);
            if (result == null)
                return NotFound();
            return Ok(result);
        }
    }
}
