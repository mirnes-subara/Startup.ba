using System.ComponentModel.DataAnnotations;

namespace Startupba.Model.Requests
{
    public class SupportTicketUpsertRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;
    }
}
