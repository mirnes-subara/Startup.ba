namespace Startupba.Model.Responses
{
    public class PaymentIntentResponse
    {
        public int PaymentId { get; set; }
        public int DonationId { get; set; }
        public string PaymentIntentId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
        public string EphemeralKey { get; set; } = string.Empty;
        public string CustomerId { get; set; } = string.Empty;
    }
}
