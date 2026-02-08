using Kk.Kharts.Shared.DTOs.Interfaces;

namespace Kk.Kharts.Shared.DTOs.Em300.Em300Th
{
    public class Em300ThDTO : IDeviceDataEm300Th
    {
        public DateTime Timestamp { get; set; }
        public string DevEui { get; set; } = string.Empty;
        public float Temperature { get; set; }
        public float Humidity { get; set; }
        public float Battery { get; set; }
    

    
        public override string ToString()
        {
            return $"Time={Timestamp:o}, DevEui={DevEui}, Temp={Temperature}, Hum={Humidity}, Batt={Battery} ";
        }
    }

}


