using Kk.Kharts.Shared.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Kk.Kharts.Shared.DTOs.UC502.Wet150
{
    public class PayloadWet150FromUg65
    {
        public int Id { get; }

        [JsonIgnore]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;          // Parte da chave primária

        [JsonPropertyName("devEUI")]
        public string DevEui { get; set; } = string.Empty;  // Parte da chave primária

        [JsonPropertyName("sdi12_1")]
        public string? SDI12 { get; set; }

        public float? Battery { get; set; } = 99.99f;

        [ForeignKey("DeviceId")]
        public uint DeviceId { get; set; }                  // Relacionamento com a tabela Device

        [JsonIgnore]                                        // Impede que o Device seja retornado na API
        public Device? Device { get; set; }                  // Navegação para a tabela Device
    }
}

 //  { "applicationID":4,"devEUI":"24e124454e353793","deviceName":"UC502_353793","sdi12_1":"0+63.627+54.6+22.17\r\n"}
