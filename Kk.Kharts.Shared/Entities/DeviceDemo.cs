using System.ComponentModel.DataAnnotations;

namespace Kk.Kharts.Shared.Entities
{
    public class DeviceDemo
    {
        [Key()]
        [MaxLength(16, ErrorMessage = "Le champ DevEui ne peut pas dépasser 16 caractères.")]
        [Required]
        public string DevEui { get; set; } = string.Empty;
        [Required]
        [MaxLength(16, ErrorMessage = "Le champ Name ne peut pas dépasser 16 caractères.")]
        public string Name { get; set; } = string.Empty;
        [Required]
        [MaxLength(40, ErrorMessage = "Le champ Description ne peut pas dépasser 40 caractères.")]
        public string Description { get; set; } = string.Empty;
        [Required]
        [MaxLength(50, ErrorMessage = "Le champ InstallationLocation ne peut pas dépasser 50 caractères.")]
        public string InstallationLocation { get; set; } = string.Empty;
    }
}