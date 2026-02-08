using Kk.Kharts.Shared.DTOs.Interfaces;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Kk.Kharts.Shared.Entities.Em300
{
    public class Em300Th : IReading
    {
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string DevEui { get; set; } = string.Empty;
        //public string DevEui
        //{
        //    get => _devEui;
        //    set => _devEui = value?.ToUpperInvariant() ?? string.Empty;
        //}
        public float Temperature { get; set; }
        public float Humidity { get; set; }
        public float Battery { get; set; }

        [ForeignKey("DeviceId")]
        public int DeviceId { get; set; } 

        [JsonIgnore] // Impede que o Device seja retornado na API        
        public Device? Device { get; set; }
    }
}


