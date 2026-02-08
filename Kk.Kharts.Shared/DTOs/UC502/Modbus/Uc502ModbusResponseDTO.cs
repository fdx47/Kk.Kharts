using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Shared.DTOs.UC502.Modbus
{
    public class Uc502ModbusResponseDTO
    {
        public string DevEui { get; set; } = string.Empty;    // DevEui no topo do json
        public int DeviceId { get; set; }                    // DeviceId no topo do json
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<Uc502ModbusDataDTO> Data { get; set; } = new List<Uc502ModbusDataDTO>();  // Dados organizados em uma lista
    }
}
