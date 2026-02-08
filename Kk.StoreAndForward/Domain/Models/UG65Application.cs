using System.Text.Json;
using System.Text.Json.Serialization;

namespace KK.UG6x.StoreAndForward.Domain.Models
{
    public class UG65Application
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        // A estrutura exata do output pode variar, vamos tentar mapear o que é comum
        // Geralmente pode ser uma lista de outputs ou um objeto complexo.
        // Vamos usar JsonElement para flexibilidade inicial e parsear manualmente se precisar.
        [JsonPropertyName("dataTransmission")]
        public JsonElement DataTransmission { get; set; }
    }
}
