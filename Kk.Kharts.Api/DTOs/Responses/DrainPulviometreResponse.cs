namespace Kk.Kharts.Api.DTOs.Responses;

public sealed class DrainPluviometreResponse
{
    public string DevEui { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public double WaterUsedLiters { get; set; }
    public double ValuePerPulse { get; set; }
    public double PulseCount { get; set; }
    public double MeasuredLiters { get; set; }
    public double DrainageLiters { get; set; }
}
