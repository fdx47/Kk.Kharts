namespace Kk.Kharts.Shared.DTOs.Interfaces
{
    public interface IDeviceDataEm300Th
    {
        float Battery { get; set; }
        string DevEui { get; set; }
        DateTime Timestamp { get; set; }
        float Temperature { get; set; }
        float Humidity { get; set; }
        //float Water { get; set; }
    }

}
