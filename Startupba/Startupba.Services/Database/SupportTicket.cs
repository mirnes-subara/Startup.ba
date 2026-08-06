using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Startupba.Services.Database
{
    /// <summary>
    /// A user support request (complaint, question or suggestion)
    /// handled by the platform administrator.
    /// </summary>
    public class SupportTicket
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 0=Open, 1=Answered, 2=Closed
        /// </summary>
        [Required]
        public int Status { get; set; } = 0;

        [MaxLength(2000)]
        public string? AdminResponse { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? AnsweredAt { get; set; }

        public DateTime? ClosedAt { get; set; }

        // Navigation property
        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}
