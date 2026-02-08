using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kk.Kharts.Shared.DTOs.UC502
{
    public class PayloadWet150MultiSensorFromUg65Dto
    {
        public int Id { get; }

        [JsonPropertyName("kktimestamp")]
        public DateTime? KkTimestamp { get; set; }

        [JsonIgnore]
        public DateTime Timestamp
        {
            get => !KkTimestamp.HasValue || KkTimestamp.Value == default
                ? DateTime.UtcNow
                : KkTimestamp.Value.ToUniversalTime();
            set => KkTimestamp = value;
        }

        [JsonPropertyName("devEUI")]
        public string DevEui { get; set; } = string.Empty;  // Parte da chave primária

        /// <summary>
        /// Campos dinâmicos no formato "sdi12_1", "sdi12_2", ..., "sdi12_16".
        /// Exemplo: { "sdi12_1": "123.4", "sdi12_2": "567.8" }
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, JsonElement> ExtraFieldsSdi12 { get; set; } = new();

        //[JsonPropertyName("sdi12_1")]
        //public string? SDI12_1 { get; set; }
        //[JsonPropertyName("sdi12_2")]
        //public string? SDI12_21 { get; set; }
        //[JsonPropertyName("sdi12_3")]
        //public string? SDI12_3 { get; set; }
        //[JsonPropertyName("sdi12_4")]
        //public string? SDI12_4 { get; set; }
        public float? Battery { get; set; } = 99.99f;
    }
}


/*
{
   "applicationID":4,
   "devEUI":"24e124454e353793",
   "deviceName":"UC502_353793",
   "sdi12_1":"0+63.627+54.6+22.17\r\n"
}
*/