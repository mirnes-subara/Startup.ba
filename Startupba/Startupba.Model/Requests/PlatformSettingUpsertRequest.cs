using System.ComponentModel.DataAnnotations;

namespace Startupba.Model.Requests
{
    public class PlatformSettingUpsertRequest
    {
        [Required]
        [MaxLength(100)]
        public string Key { get; set; } = string.Empty;

        [Required]
        [MaxLength(4000)]
        public string Value { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }
    }
}
