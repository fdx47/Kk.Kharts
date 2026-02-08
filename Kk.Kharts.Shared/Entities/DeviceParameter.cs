using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kk.Kharts.Shared.Entities
{
    public class DeviceParameter
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Device")]
        public int DeviceId { get; set; }

        // Parâmetros específicos por dispositivo
        public float ValuePerPulse { get; set; }
        public float WaterConversion { get; set; }
        public float PulseConversion { get; set; }
        public float? AlarmThreshold { get; set; }

        public Device? Device { get; set; }
    }
}
