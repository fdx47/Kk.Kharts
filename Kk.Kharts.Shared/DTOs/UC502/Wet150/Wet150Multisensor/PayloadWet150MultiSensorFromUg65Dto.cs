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

        public float? Battery { get; set; } = 99.99f;
    }
}