using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Kk.Kharts.Shared.Entities.UC502.Wet150MultiSensor
{
    public class Wet150MultiSensor4
    {               
        private string _devEui = string.Empty;

        public DateTime Timestamp { get; set; } = DateTime.UtcNow; // Parte da chave primária
        public string DevEui
        {
            get => _devEui;
            set => _devEui = value?.ToUpperInvariant() ?? string.Empty;
        }

        public string? Sdi12_1 { get; set; }
        public string? Sdi12_2 { get; set; }
        public string? Sdi12_3 { get; set; }
        public string? Sdi12_4 { get; set; }
        public float Battery { get; set; }

        [ForeignKey("DeviceId")]
        public int DeviceId { get; set; }                  // Relacionamento com a tabela Device

        [JsonIgnore]                                        // Impede que o Device seja retornado na API
        public Device Device { get; set; } = null!;          // Navegação para a tabela Device
    }
}


