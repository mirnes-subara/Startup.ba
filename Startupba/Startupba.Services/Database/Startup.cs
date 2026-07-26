using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Startupba.Services.Database
{
    public class Startup
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(4000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int FounderId { get; set; } // FK to User (entrepreneur)

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int CityId { get; set; }

        /// <summary>
        /// Funding goal the founder wants to reach.
        /// </summary>
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TargetAmount { get; set; }

        /// <summary>
        /// Total of completed donations. Kept up to date when donations complete.
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal AmountRaised { get; set; } = 0;

        /// <summary>
        /// Percentage the platform keeps when the target is reached.
        /// Snapshot of the PlatformSetting value at approval time (transparency for the founder).
        /// </summary>
        [Column(TypeName = "decimal(5,2)")]
        public decimal PlatformFeePercent { get; set; } = 0;

        /// <summary>
        /// 1=Draft, 2=Pending, 3=Approved, 4=Rejected, 5=Paused, 6=Completed
        /// </summary>
        [Required]
        public int StatusId { get; set; } = StartupStatuses.Pending;

        /// <summary>
        /// Reason the admin gave when rejecting the startup.
        /// </summary>
        [MaxLength(1000)]
        public string? RejectionReason { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        public DateTime? ApprovedAt { get; set; }

        public DateTime? CompletedAt { get; set; }

        // Navigation properties
        public User Founder { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public City City { get; set; } = null!;
        public StartupStatus Status { get; set; } = null!;

        public ICollection<StartupImage> StartupImages { get; set; } = new List<StartupImage>();
        public ICollection<Donation> Donations { get; set; } = new List<Donation>();
        public ICollection<StartupLike> StartupLikes { get; set; } = new List<StartupLike>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
    }
}
