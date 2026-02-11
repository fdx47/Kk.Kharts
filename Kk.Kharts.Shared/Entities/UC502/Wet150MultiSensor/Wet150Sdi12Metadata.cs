using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kk.Kharts.Shared.Entities.UC502.Wet150MultiSensor
{
    public class Wet150Sdi12Metadata
    {               
        private string _devEui = string.Empty;
     
        [Key]
        public int Id { get; set; }
        public string DevEui
        {
            get => _devEui;
            set => _devEui = value?.ToUpperInvariant() ?? string.Empty;
        }

        public int Sdi12Index { get; set; }                 // 1, 2, 3, ...

        public string Sdi12Name { get; set; } = string.Empty;

        public string? Sdi12InstallationLocation { get; set; } = string.Empty;

        [ForeignKey("DevEui")]
        public Device Device { get; set; } = null!;
    }
}
