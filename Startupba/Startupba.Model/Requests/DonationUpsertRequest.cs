using System.ComponentModel.DataAnnotations;

namespace Startupba.Model.Requests
{
    public class DonationUpsertRequest
    {
        [Required]
        public int StartupId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [Range(1, 100000000)]
        public decimal Amount { get; set; }

        [MaxLength(500)]
        public string? Message { get; set; }
    }
}
