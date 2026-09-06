using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Startupba.Services.Database
{
    /// <summary>
    /// Audit row for every startup status change (admin, founder resubmit, funding complete, delete).
    /// </summary>
    public class StartupStatusHistory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StartupId { get; set; }

        public int FromStatusId { get; set; }

        [Required]
        public int ToStatusId { get; set; }

        public int? ActorUserId { get; set; }

        [MaxLength(1000)]
        public string? Reason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(StartupId))]
        public Startup Startup { get; set; } = null!;

        [ForeignKey(nameof(FromStatusId))]
        public StartupStatus FromStatus { get; set; } = null!;

        [ForeignKey(nameof(ToStatusId))]
        public StartupStatus ToStatus { get; set; } = null!;

        [ForeignKey(nameof(ActorUserId))]
        public User? Actor { get; set; }
    }
}
