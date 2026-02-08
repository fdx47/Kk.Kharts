using Kk.Kharts.Shared.DTOs.Interfaces;

namespace Kk.Kharts.Shared.DTOs.Em300.Em300Di
{
    public class Em300DiDTO : IDeviceDataEm300Di
    {
        public DateTime Timestamp { get; set; }
        public string DevEui { get; set; } = string.Empty;
        public float Temperature { get; set; }
        public float Humidity { get; set; }
        public float Water { get; set; }
        public float Battery { get; set; }



        public override string ToString()
        {
            return $"Time={Timestamp:o}, DevEui={DevEui}, Temp={Temperature}, Hum={Humidity}, Water={Water}, Batt={Battery} ";
        }
    }

}


