using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Startupba.Services.Database
{
    /// <summary>
    /// Reference table for startup statuses.
    /// Fixed IDs: 1=Draft, 2=Pending, 3=Approved, 4=Rejected, 5=Paused, 6=Completed, 7=Deleted
    /// </summary>
    public class StartupStatus
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation property
        public ICollection<Startup> Startups { get; set; } = new List<Startup>();
    }

    /// <summary>
    /// Well-known StartupStatus IDs (kept in sync with seeded data).
    /// </summary>
    public static class StartupStatuses
    {
        public const int Draft = 1;
        public const int Pending = 2;
        public const int Approved = 3;
        public const int Rejected = 4;
        public const int Paused = 5;
        public const int Completed = 6;
        public const int Deleted = 7;
    }
}
