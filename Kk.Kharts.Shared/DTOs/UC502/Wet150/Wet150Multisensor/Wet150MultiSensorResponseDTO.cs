using System.Text.Json.Serialization;

namespace Kk.Kharts.Shared.DTOs.UC502.Wet150.Wet150Multisensor
{
    public class Wet150MultiSensorResponseDTO
    {
        public string DevEui { get; set; } = string.Empty;
        public int DeviceId { get; set; }
        public int Model { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string InstallationLocation { get; set; } = string.Empty;

        [JsonPropertyName("sdi12_1_metadata")]
        public Sdi12MetadataDto Sdi12_1_Metadata{ get; set; } = new();

        [JsonPropertyName("sdi12_2_metadata")]
        public Sdi12MetadataDto Sdi12_2_Metadata { get; set; } = new();

        [JsonPropertyName("sdi12_3_metadata")]
        public Sdi12MetadataDto? Sdi12_3_Metadata { get; set; }

        [JsonPropertyName("sdi12_4_metadata")]
        public Sdi12MetadataDto? Sdi12_4_Metadata { get; set; } 

        public List<Wet150SensorDataDto> Sdi12_1 { get; set; } = new();
        public List<Wet150SensorDataDto> Sdi12_2 { get; set; } = new();
        public List<Wet150SensorDataDto>? Sdi12_3 { get; set; } = new();
        public List<Wet150SensorDataDto>? Sdi12_4 { get; set; } = new();
    }


}
