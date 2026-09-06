using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Startupba.Services.Database
{
    /// <summary>
    /// A user opened a startup's detail page. One row per (startup, user);
    /// LastViewedAt is updated when the throttle window has elapsed.
    /// </summary>
    public class StartupView
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StartupId { get; set; }

        [Required]
        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime LastViewedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(StartupId))]
        public Startup Startup { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public User User { get; set; } = null!;
    }
}
