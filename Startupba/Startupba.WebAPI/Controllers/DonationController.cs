using Startupba.Model;
using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;
using Startupba.WebAPI.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Startupba.WebAPI.Controllers
{
    public class DonationController : BaseCRUDController<DonationResponse, DonationSearchObject, DonationUpsertRequest, DonationUpsertRequest>
    {
        public DonationController(IDonationService service) : base(service)
        {
        }

        [HttpPost]
        public override Task<DonationResponse> Create([FromBody] DonationUpsertRequest request)
        {
            throw new UserException("Donations can only be created through the payment flow.");
        }

        [HttpPut("{id}")]
        public override Task<DonationResponse?> Update(int id, [FromBody] DonationUpsertRequest request)
        {
            throw new UserException("Donation amount, startup, and donor cannot be changed.");
        }

        [HttpDelete("{id}")]
        public override Task<bool> Delete(int id)
        {
            throw new UserException("Donations cannot be deleted.");
        }
    }
}
