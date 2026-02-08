using System.Text.Json.Serialization;
using Kk.Kharts.Shared.DTOs.UC502.Wet150;

namespace Kk.Kharts.Api.DTOs.Requests.Uc502
{
    public class Uc502Wet150PostRequest
    {
        [JsonPropertyName("kkTimestamp")]
        public DateTime? KkTimestamp { get; set; }

        [JsonPropertyName("devEui")]
        public string DevEui { get; set; } = string.Empty;

        public float Permittivite { get; set; }
        public float ECb { get; set; }
        public float SoilTemperature { get; set; }
        public float MineralVWC { get; set; }
        public float OrganicVWC { get; set; }
        public float PeatMixVWC { get; set; }
        public float CoirVWC { get; set; }
        public float MinWoolVWC { get; set; }
        public float PerliteVWC { get; set; }
        public float MineralECp { get; set; }
        public float OrganicECp { get; set; }
        public float PeatMixECp { get; set; }
        public float CoirECp { get; set; }
        public float MinWoolECp { get; set; }
        public float PerliteECp { get; set; }
        public float Battery { get; set; }

        public Uc502Wet150DTO ToDto()
        {
            var measurementTimestamp = !KkTimestamp.HasValue || KkTimestamp.Value == default
                ? DateTime.UtcNow
                : KkTimestamp.Value.ToUniversalTime();

            return new Uc502Wet150DTO
            {
                Timestamp = measurementTimestamp,
                DevEui = DevEui,
                Permittivite = Permittivite,
                ECb = ECb,
                SoilTemperature = SoilTemperature,
                MineralVWC = MineralVWC,
                OrganicVWC = OrganicVWC,
                PeatMixVWC = PeatMixVWC,
                CoirVWC = CoirVWC,
                MinWoolVWC = MinWoolVWC,
                PerliteVWC = PerliteVWC,
                MineralECp = MineralECp,
                OrganicECp = OrganicECp,
                PeatMixECp = PeatMixECp,
                CoirECp = CoirECp,
                MinWoolECp = MinWoolECp,
                PerliteECp = PerliteECp,
                Battery = Battery
            };
        }
    }
}
