using Startupba.Model.Requests;
using Startupba.Model.Responses;
using Startupba.Model.SearchObjects;
using System.Threading.Tasks;

namespace Startupba.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentIntentResponse> CreatePaymentIntentAsync(CreatePaymentIntentRequest request);
        Task<PaymentResponse> ConfirmPaymentAsync(int paymentId, ConfirmPaymentRequest request);
        Task<PaymentResponse?> GetByIdAsync(int id);
        Task<PagedResult<PaymentResponse>> GetAsync(PaymentSearchObject search);
    }
}
