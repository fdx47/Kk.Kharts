using System.Text.Json.Serialization;

namespace KK.UG6x.StoreAndForward.Domain.DTOs;

public class GatewayPayloadDTO
{
    [JsonPropertyName("kktimestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("devEUI")]
    public string DevEui { get; set; } = string.Empty;

    // Captura bateria se existir no JSON original (EM300-TH, UC502, etc)
    [JsonPropertyName("battery")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public float? Battery { get; set; }

    // Captura todos os outros campos dinamicamente (temperature, humidity, sdi12_1, etc)
    [JsonExtensionData]
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}
