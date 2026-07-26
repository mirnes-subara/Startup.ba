using System.ComponentModel.DataAnnotations;

namespace Startupba.Model.Requests
{
    public class StartupUpsertRequest
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(4000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public int FounderId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [Required]
        public int CityId { get; set; }

        [Required]
        [Range(1, 100000000)]
        public decimal TargetAmount { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
