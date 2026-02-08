namespace Kk.Kharts.Client.Models
{
    public class SensorData
    {
        public DateTime Timestamp { get; set; }
        public decimal Permittivite { get; set; }
        public decimal ECb { get; set; }
        public decimal SoilTemperature { get; set; }
        public decimal MineralVWC { get; set; }
        public decimal OrganicVWC { get; set; }
        public decimal PeatMixVWC { get; set; }
        public decimal CoirVWC { get; set; }
        public decimal MinWoolVWC { get; set; }
        public double PerliteVWC { get; set; }
        public double MineralECp { get; set; }
        public double OrganicECp { get; set; }
        public double PeatMixECp { get; set; }
        public double CoirECp { get; set; }
        public double MinWoolECp { get; set; }
        public double PerliteECp { get; set; }
        public double Battery { get; set; }
    }

}
