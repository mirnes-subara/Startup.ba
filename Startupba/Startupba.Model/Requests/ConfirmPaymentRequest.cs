using System.ComponentModel.DataAnnotations;

namespace Startupba.Model.Requests
{
    /// <summary>
    /// Confirm a Stripe payment. The linked donation is taken from the payment record,
    /// not from the client body.
    /// </summary>
    public class ConfirmPaymentRequest
    {
        /// <summary>
        /// Ignored. Kept so older clients that still send donationId do not fail model binding.
        /// </summary>
        public int? DonationId { get; set; }
    }
}
