using System.Text.Json.Serialization;
using Kk.Kharts.Shared.DTOs.Em300.Em300Th;

namespace Kk.Kharts.Api.DTOs.Requests.Em300
{
    public class Em300ThPostRequest
    {
        [JsonPropertyName("kkTimestamp")]
        public DateTime? KkTimestamp { get; set; }

        // Campo legado ignorado propositalmente (mantido apenas para compatibilidade)
        [JsonPropertyName("timestamp")]
        public DateTime? LegacyTimestamp { get; set; }

        [JsonPropertyName("devEui")]
        public string DevEui { get; set; } = string.Empty;

        public float Temperature { get; set; }
        public float Humidity { get; set; }
        public float Battery { get; set; }

        public Em300ThDTO ToDto()
        {
            var measurementTimestamp = !KkTimestamp.HasValue || KkTimestamp.Value == default
                ? DateTime.UtcNow
                : KkTimestamp.Value.ToUniversalTime();

            return new Em300ThDTO
            {
                Timestamp = measurementTimestamp,
                DevEui = DevEui,
                Temperature = Temperature,
                Humidity = Humidity,
                Battery = Battery
            };
        }
    }
}
