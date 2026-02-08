using Kk.Kharts.Shared.DTOs.Interfaces;

namespace Kk.Kharts.Shared.DTOs.Em300.Em300Th
{
    public class Em300ThDTOAdapter : IDeviceDataEm300Th
    {
        private readonly Em300ThDTO _dto;

        public Em300ThDTOAdapter(Em300ThDTO dto)
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
    }

}

/*
 
Se você não pode alterar essas classes (exemplo, são geradas em outro projeto ou não tem acesso
Uma alternativa é criar classes adaptadoras (wrappers) que implementam a interface e encapsulam o DTO

 */
