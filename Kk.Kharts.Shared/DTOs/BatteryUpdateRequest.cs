using System.ComponentModel.DataAnnotations;

namespace Kk.Kharts.Shared.DTOs
{
    public class BatteryUpdateRequest
    {
        [Required]
        public required string DevEui { get; set; }
        [Required]
        public required float Battery { get; set; }
    }
}
