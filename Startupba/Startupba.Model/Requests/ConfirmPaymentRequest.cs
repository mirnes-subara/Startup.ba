using System.ComponentModel.DataAnnotations;

namespace Startupba.Model.Requests
{
    public class ConfirmPaymentRequest
    {
        [Required]
        public int DonationId { get; set; }
    }
}
