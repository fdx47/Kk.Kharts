using System.Text.Json.Serialization;

namespace Kk.Kharts.Shared.DTOs.UC502.Modbus
{
    public class Uc502ModbusDataDTO
    {
        [JsonPropertyName("kktimestamp")]
        public DateTime? KkTimestamp { get; set; }

        public DateTime Timestamp
        {
            get => !KkTimestamp.HasValue || KkTimestamp.Value == default
                ? _timestamp
                : KkTimestamp.Value.ToUniversalTime();
            set
            {
                _timestamp = value;
                KkTimestamp = value;
            }
        }
        private DateTime _timestamp = DateTime.UtcNow;

        [JsonPropertyName("devEUI")]
        public string DevEui { get; set; } = string.Empty;  // Parte da chave primária

        [JsonPropertyName("modbus_chn_1")]
        public float? ModbusChannel1 { get; set; }

        [JsonPropertyName("modbus_chn_2")]
        public float? ModbusChannel2 { get; set; }

        [JsonPropertyName("modbus_chn_3")]
        public float? ModbusChannel3 { get; set; }

        [JsonPropertyName("modbus_chn_4")]
        public float? ModbusChannel4 { get; set; }

        [JsonPropertyName("modbus_chn_5")]
        public float? ModbusChannel5 { get; set; }

        [JsonPropertyName("modbus_chn_6")]
        public float? ModbusChannel6 { get; set; }

        [JsonPropertyName("battery")]
        public float? Battery { get; set; } = 99.99f;
    }
}


//{ "applicationID":1,"devEUI":"24e124454e045483","deviceName":"UC502_045483","modbus_chn_1":411}