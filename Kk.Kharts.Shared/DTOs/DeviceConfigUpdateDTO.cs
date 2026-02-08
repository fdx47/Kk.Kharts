using System.ComponentModel.DataAnnotations;

namespace Kk.Kharts.Shared.DTOs
{
    public class DeviceConfigUpdateDTO
    {
        //[Required]
        //public string DevEui { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string? Description { get; set; }       
       
        [Required]
        public bool? ActiveInKropKontrol { get; set; }
       
        //[Required]
        //public int CompanyId { get; set; }
        [Required]
        public string? InstallationLocation { get; set; }
    }

}
