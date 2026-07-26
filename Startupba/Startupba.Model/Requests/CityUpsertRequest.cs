using System.ComponentModel.DataAnnotations;

namespace Startupba.Model.Requests
{
    public class CityUpsertRequest
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public int CountryId { get; set; }
        
        public bool IsActive { get; set; } = true;
    }
} 