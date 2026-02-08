namespace Kk.Kharts.Client.Models
{
    public class SensorDataResponse
    {
        public required string DevEui { get; set; }
        public int DeviceId { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required List<SensorData> Data { get; set; }
    }
}
