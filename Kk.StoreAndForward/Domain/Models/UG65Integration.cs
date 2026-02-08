using System.Text.Json.Serialization;

namespace KK.UG6x.StoreAndForward.Domain.Models
{
    public class UG65Integration
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("dataUpURL")]
        public string DataUpUrl { get; set; } = string.Empty;

        [JsonPropertyName("headers")]
        public List<UG65Header> Headers { get; set; } = new();
    }

}
