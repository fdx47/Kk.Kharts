using Kk.Kharts.Shared.DTOs.Interfaces;

namespace Kk.Kharts.Shared.DTOs.Em300.Em300Di
{
    public class Em300DiDTOAdapter : IDeviceDataEm300Di
    {
        private readonly Em300DiDTO _dto;

        public Em300DiDTOAdapter(Em300DiDTO dto)
        {
            _dto = dto;
        }

        public float Battery
        {
            get => _dto.Battery;
            set => _dto.Battery = value;
        }

        public string DevEui
        {
            get => _dto.DevEui;
            set => _dto.DevEui = value;
        }

        public DateTime Timestamp
        {
            get => _dto.Timestamp;
            set => _dto.Timestamp = value;
        }

        public float Temperature
        {
            get => _dto.Temperature;
            set => _dto.Temperature = value;
        }

        public float Humidity
        {
            get => _dto.Humidity;
            set => _dto.Humidity = value;
        }

        public float Water
        {
            get => _dto.Water;
            set => _dto.Water = value;
        }

    }

}
