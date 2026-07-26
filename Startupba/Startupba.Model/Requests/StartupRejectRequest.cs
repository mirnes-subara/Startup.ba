using System.ComponentModel.DataAnnotations;

namespace Startupba.Model.Requests
{
    public class StartupRejectRequest
    {
        [Required]
        [MaxLength(1000)]
        public string Reason { get; set; } = string.Empty;
    }
}
