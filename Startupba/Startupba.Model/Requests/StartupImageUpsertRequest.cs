using System.ComponentModel.DataAnnotations;

namespace Startupba.Model.Requests
{
    public class StartupImageUpsertRequest
    {
        [Required]
        public int StartupId { get; set; }

        [Required]
        public byte[] ImageData { get; set; } = System.Array.Empty<byte>();

        public int? DisplayOrder { get; set; }

        public bool IsCover { get; set; } = false;

        public bool IsLogo { get; set; } = false;

        public bool IsActive { get; set; } = true;
    }
}
