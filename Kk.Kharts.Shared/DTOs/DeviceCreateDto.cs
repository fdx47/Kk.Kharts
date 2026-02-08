namespace Kk.Kharts.Shared.DTOs
{
    public class DeviceCreateDto
    {
        public string DevEui { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? InstallationLocation { get; set; }
        public float Battery { get; set; } = 100.0f; // Valor padrão de bateria
        public DateTime InstallationDate { get; set; }
        public int DeviceModel { get; set; }
        public int CompanyId { get; set; }
        public string Seller { get; set; } = string.Empty;
        public bool ActiveInKropKontrol { get; set; } = true;
    }

}
