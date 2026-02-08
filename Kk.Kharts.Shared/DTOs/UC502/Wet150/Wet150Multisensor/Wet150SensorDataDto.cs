namespace Kk.Kharts.Shared.DTOs.UC502.Wet150.Wet150Multisensor
{
    public class Wet150SensorDataDto
    {
        public DateTime Timestamp { get; set; }
        public float Permittivite { get; set; }
        public float ECb { get; set; }
        public float SoilTemperature { get; set; }
        public float MineralVWC { get; set; }
        public float OrganicVWC { get; set; }
        public float PeatMixVWC { get; set; }
        public float CoirVWC { get; set; }
        public float MinWoolVWC { get; set; }
        public float PerliteVWC { get; set; }
        public float MineralECp { get; set; }
        public float OrganicECp { get; set; }
        public float PeatMixECp { get; set; }
        public float CoirECp { get; set; }
        public float MinWoolECp { get; set; }
        public float PerliteECp { get; set; }
        public float Battery { get; set; }
    }

}
