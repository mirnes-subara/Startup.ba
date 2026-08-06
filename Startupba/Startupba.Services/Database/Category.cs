using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Startupba.Services.Database
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public ICollection<Startup> Startups { get; set; } = new List<Startup>();
    }
}
