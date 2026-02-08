namespace Kk.Kharts.Shared.DTOs.Em300.Em300Di   
{
    public class Em300DiResponseDTO
    {
        public string DevEui { get; set; } = string.Empty;    // DevEui no topo do json
        public int DeviceId { get; set; }                    // DeviceId no topo do json
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public float ValuePerPulse { get; set; }
        public List<Em300DiDataDTO> Data { get; set; } = new();
    }
}
