using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Startupba.Services.Database
{
    /// <summary>
    /// Platform-wide announcement created and managed by the administrator
    /// to keep users informed about news and changes.
    /// </summary>
    public class Announcement
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(4000)]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int CreatedByUserId { get; set; } // FK to User (admin)

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        [ForeignKey("CreatedByUserId")]
        public User CreatedBy { get; set; } = null!;
    }
}
