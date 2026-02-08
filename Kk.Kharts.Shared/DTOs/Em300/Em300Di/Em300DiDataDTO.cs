namespace Kk.Kharts.Shared.DTOs.Em300.Em300Di
{
    public class Em300DiDataDTO
    {
        public DateTime Timestamp { get; set; }
        //public string DevEui { get; set; } = string.Empty;
        public float Temperature { get; set; }
        public float Humidity { get; set; }
        public float Water { get; set; }
        public float Battery { get; set; }     
    }
}
