using Kk.Kharts.Shared.DTOs.Interfaces;

namespace Kk.Kharts.Shared.DTOs.Em300.Em300Di
{
    public class EM300DiByHttpUg65 : IDeviceDataEm300Di
    {
        public int ApplicationId { get; set; }
        public uint DeviceId { get; set; }
        public string DevEui { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public float Temperature { get; set; }

        // Faltou esta propriedade obrigatória da interface IDeviceData
        public float Humidity { get; set; }
        public float Water { get; set; }
        public float Battery { get; set; }

    }
}
