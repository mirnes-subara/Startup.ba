using System.ComponentModel.DataAnnotations;

namespace Startupba.Model.Requests
{
    public class SupportTicketAnswerRequest
    {
        [Required]
        [MaxLength(2000)]
        public string AdminResponse { get; set; } = string.Empty;
    }
}
