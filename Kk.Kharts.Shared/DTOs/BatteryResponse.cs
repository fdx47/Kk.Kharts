namespace Kk.Kharts.Shared.DTOs
{
    public class BatteryResponse
    {

        public int Id { get; set; }
        public string DevEui { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public float Battery { get; set; }      
    }
}
