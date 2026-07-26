using System.ComponentModel.DataAnnotations;

namespace Startupba.Model.Requests
{
    public class AnnouncementUpsertRequest
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(4000)]
        public string Content { get; set; } = string.Empty;

        [Required]
        public int CreatedByUserId { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
