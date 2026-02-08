namespace Kk.Kharts.Shared.DTOs.UC502.Wet150.Wet150Multisensor
{
    public class Wet150Sdi12MetadataResponseDto
    {
        public int Id { get; set; }
        public string DevEui { get; set; } = string.Empty;
        public int Index { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? InstallationLocation { get; set; }
    }
}
