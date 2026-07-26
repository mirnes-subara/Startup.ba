using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Startupba.Services.Database
{
    public class StartupImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int StartupId { get; set; }

        [ForeignKey("StartupId")]
        public Startup Startup { get; set; } = null!;

        [Required]
        public byte[] ImageData { get; set; } = Array.Empty<byte>();

        public int? DisplayOrder { get; set; } // For ordering images (1, 2, 3, etc.)

        public bool IsCover { get; set; } = false; // Mark one image as cover for the startup

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
