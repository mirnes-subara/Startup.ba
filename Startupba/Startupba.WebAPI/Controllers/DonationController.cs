using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Startupba.WebAPI.Controllers
{
    public class DonationController : BaseCRUDController<DonationResponse, DonationSearchObject, DonationUpsertRequest, DonationUpsertRequest>
    {
        public DonationController(IDonationService service) : base(service)
        {
        }

        /// <summary>
        /// Marks a pending donation as completed: updates the startup's raised amount,
        /// notifies the founder (in-app + email) and auto-completes the startup when
        /// the funding target is reached. Normally invoked through the payment flow;
        /// exposed here for the desktop app and testing.
        /// </summary>
        [HttpPut("{id}/complete")]
        public async Task<ActionResult<DonationResponse>> Complete(int id)
        {
            var result = await ((IDonationService)_service).CompleteAsync(id);
            if (result == null)
                return NotFound();
            return Ok(result);
        }
    }
}
