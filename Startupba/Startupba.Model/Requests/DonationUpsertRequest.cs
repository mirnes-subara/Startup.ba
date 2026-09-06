using System.ComponentModel.DataAnnotations;

namespace Startupba.Model.Requests
{
    public class DonationUpsertRequest
    {
        [Required]
        public int StartupId { get; set; }

        /// <summary>
        /// Set server-side from JWT. Ignored if sent by the client.
        /// </summary>
        public int UserId { get; set; }

        [Required]
        [Range(1, 100000000)]
        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string? Message { get; set; }
    }
}
