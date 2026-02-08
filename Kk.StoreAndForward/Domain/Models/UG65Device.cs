using System.Text.Json.Serialization;

namespace KK.UG6x.StoreAndForward.Domain.Models
{
    public class UG65Device
    {
        [JsonPropertyName("devEUI")]
        public string DevEui { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("applicationID")]
        public string ApplicationId { get; set; } = string.Empty;
    }
}
