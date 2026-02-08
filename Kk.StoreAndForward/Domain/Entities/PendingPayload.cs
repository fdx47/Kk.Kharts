namespace KK.UG6x.StoreAndForward.Domain.Entities;

public class PendingPayload
{
    public long Id { get; set; }
    public string DevEui { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = string.Empty;
    public string EndpointType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int RetryCount { get; set; }
    public bool IsSent { get; set; }
    public DateTime? LastUpdated { get; set; }
}
