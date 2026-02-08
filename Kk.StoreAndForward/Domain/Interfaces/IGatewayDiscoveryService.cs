namespace KK.UG6x.StoreAndForward.Domain.Interfaces
{
    public interface IGatewayDiscoveryService
    {
        Task<string?> FindGatewayAsync(string? lastKnownIp, string username, string password, CancellationToken ct);
    }
}
