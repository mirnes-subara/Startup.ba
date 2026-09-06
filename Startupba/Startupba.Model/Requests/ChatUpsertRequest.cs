using System.ComponentModel.DataAnnotations;

namespace Startupba.Model.Requests
{
    public class ChatUpsertRequest
    {
        /// <summary>
        /// Set server-side from JWT. Ignored if sent by the client.
        /// </summary>
        public int SenderId { get; set; }

        [Required]
        public int ReceiverId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;
    }
}
