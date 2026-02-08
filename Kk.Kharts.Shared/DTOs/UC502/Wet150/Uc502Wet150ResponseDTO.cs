namespace Kk.Kharts.Shared.DTOs.UC502.Wet150
{
    public class Uc502Wet150ResponseDTO
    {
        public string DevEui { get; set; } = string.Empty;    // DevEui no topo do json
        public int DeviceId { get; set; }                    // DeviceId no topo do json
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? InstallationLocation { get; set; } = string.Empty;
        public List<Uc502Wet150DTO> Data { get; set; } = new List<Uc502Wet150DTO>();  // Dados organizados em uma lista
    }
}
