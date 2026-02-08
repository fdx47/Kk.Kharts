namespace Kk.Kharts.Shared.DTOs.UC502.Wet150.Wet150Multisensor
{
    public class SensorDataEntity
    {
        public DateTime Timestamp { get; set; }
        public string? Sdi12_1 { get; set; }
        public string? Sdi12_2 { get; set; }
        public string? Sdi12_3 { get; set; }  // Apenas para modelo 63+
        public string? Sdi12_4 { get; set; }  // Apenas para modelo 64
        public float Battery { get; set; }
    }

}
