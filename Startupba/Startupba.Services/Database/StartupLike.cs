using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Startupba.Services.Database
{
    /// <summary>
    /// Many-to-many: a user likes (recommends) a startup.
    /// </summary>
    public class StartupLike
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StartupId { get; set; }

        [Required]
        public int UserId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("StartupId")]
        public Startup Startup { get; set; } = null!;

        [ForeignKey("UserId")]
        public User User { get; set; } = null!;
    }
}
