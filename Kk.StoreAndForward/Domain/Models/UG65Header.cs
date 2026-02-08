using System.Text.Json.Serialization;

namespace KK.UG6x.StoreAndForward.Domain.Models
{
    public class UG65Header
    {
        [JsonPropertyName("key")]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }

}
