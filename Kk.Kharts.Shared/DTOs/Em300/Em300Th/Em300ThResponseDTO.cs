namespace Kk.Kharts.Shared.DTOs.Em300.Em300Th
{
    public class Em300ThResponseDTO
    {
        public string DevEui { get; set; } = string.Empty;    // DevEui no topo do json
        public int DeviceId { get; set; }                    // DeviceId no topo do json
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<Em300ThDataDTO> Data { get; set; } = new();
    }
}
