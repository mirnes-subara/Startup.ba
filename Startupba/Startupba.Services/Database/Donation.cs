using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Startupba.Services.Database
{
    /// <summary>
    /// A donation (investment) made by a user to a startup.
    /// Created as "Pending" and marked "Completed" once payment is confirmed.
    /// </summary>
    public class Donation
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StartupId { get; set; }

        [Required]
        public int UserId { get; set; } // FK to User (donor / investor)

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        /// <summary>
        /// Optional message of support shown to the founder.
        /// </summary>
        [MaxLength(500)]
        public string? Message { get; set; }

        /// <summary>
        /// "Pending", "Completed", "Failed"
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? CompletedAt { get; set; }

        // Navigation properties
        [ForeignKey("StartupId")]
        public Startup Startup { get; set; } = null!;

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}
