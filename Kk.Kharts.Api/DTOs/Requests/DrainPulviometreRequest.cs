namespace Kk.Kharts.Api.DTOs.Requests;

public sealed class DrainPluviometreRequest
{
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    /// <summary>
    /// Volume d'eau apporté pendant la régie (litres).
    /// </summary>
    public double WaterUsedLiters { get; set; }
}
