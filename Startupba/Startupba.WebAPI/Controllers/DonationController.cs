using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using Startupba.Services.Interfaces;

namespace Startupba.WebAPI.Controllers
{
    public class DonationController : BaseCRUDController<DonationResponse, DonationSearchObject, DonationUpsertRequest, DonationUpsertRequest>
    {
        public DonationController(IDonationService service) : base(service)
        {
        }
    }
}
