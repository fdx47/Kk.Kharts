using Kk.Kharts.Shared.DTOs.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Kk.Kharts.Shared.Entities.UC502
{
    public class Uc502Modbus : IReading
    {
        public int Id { get; }

        [JsonIgnore]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;  // Parte da chave primária

        [JsonPropertyName("devEUI")]        
        public string DevEui { get; set; }  // Parte da chave primária

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

        //--------------------------------------//
        [ForeignKey("DeviceId")]
        public int DeviceId { get; set; }          // Relacionamento com a tabela Device
        
        [JsonIgnore]                                // Impede que o Device seja retornado na API
        public Device? Device { get; set; }         // Navegação para a tabela Device
    }
}

//{ "applicationID":1,"devEUI":"24e124454e045483","deviceName":"UC502_045483","modbus_chn_1":0}