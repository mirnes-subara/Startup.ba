using System;

namespace Startupba.Model.Responses
{
    public class StartupImageResponse
    {
        public int Id { get; set; }
        public int StartupId { get; set; }
        public string StartupName { get; set; } = string.Empty;
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
        public int? DisplayOrder { get; set; }
        public bool IsCover { get; set; }
        public bool IsLogo { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
