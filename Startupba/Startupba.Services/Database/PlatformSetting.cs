using System;
using System.ComponentModel.DataAnnotations;

namespace Startupba.Services.Database
{
    /// <summary>
    /// Key/value store for platform-wide settings managed by the administrator
    /// (e.g. "PlatformFeePercent", "TermsOfUse").
    /// </summary>
    public class PlatformSetting
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Key { get; set; } = string.Empty;

        [Required]
        [MaxLength(4000)]
        public string Value { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public DateTime? UpdatedAt { get; set; }
    }

    /// <summary>
    /// Well-known PlatformSetting keys (kept in sync with seeded data).
    /// </summary>
    public static class PlatformSettingKeys
    {
        public const string PlatformFeePercent = "PlatformFeePercent";
        public const string TermsOfUse = "TermsOfUse";
        public const string ContactEmail = "ContactEmail";
    }
}
