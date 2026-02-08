using System.Text.Json.Serialization;

namespace Kk.Kharts.Shared.DTOs.UC502.Wet150
{
    public class PayloadWet150FromUg65WithApiKeyDTO
    {
        public int Id { get; }

        [JsonPropertyName("kktimestamp")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public DateTime? KkTimestamp { get; set; }

        [JsonIgnore]
        public bool HasOriginalTimestamp => OriginalKkTimestamp.HasValue;

        [JsonIgnore]
        private DateTime? OriginalKkTimestamp { get; set; }

        [JsonIgnore]
        public DateTime Timestamp
        {
            get => !KkTimestamp.HasValue || KkTimestamp.Value == default
                ? DateTime.UtcNow
                : KkTimestamp.Value.ToUniversalTime();
            set
            {
                OriginalKkTimestamp ??= KkTimestamp;
                KkTimestamp = value;
            }
        }

        [JsonPropertyName("devEUI")]
        public string DevEui { get; set; }  // Parte da chave primária

        [JsonPropertyName("sdi12_1")]
        public string? SDI12_1 { get; set; }
    
        public float? Battery { get; set; } = 99.99f;


    }
}

//  { "applicationID":4,"devEUI":"24e124454e353793","deviceName":"UC502_353793","sdi12_1":"0+63.627+54.6+22.17\r\n"}
/*
{
   "applicationID":4,
   "devEUI":"24e124454e353793",
   "deviceName":"UC502_353793",
   "sdi12_1":"0+63.627+54.6+22.17\r\n"
}

{
   "applicationID":4,
   "devEUI":"24e124454e353793",
   "deviceName":"UC502_353793",
   "sdi12_1":"0+63.627+54.6+22.17\r\n"
   "sdi12_2":"0+63.627+54.6+22.17\r\n"
   "sdi12_3":"0+63.627+54.6+22.17\r\n"
}



*/