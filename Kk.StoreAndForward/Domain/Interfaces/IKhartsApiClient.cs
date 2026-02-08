using KK.UG6x.StoreAndForward.Domain.DTOs;

namespace KK.UG6x.StoreAndForward.Domain.Interfaces
{
    public interface IKhartsApiClient
    {
        Task<bool> SendDataAsync(GatewayPayloadDTO data, CancellationToken cancellationToken, string? destinationUrl = null, Dictionary<string, string>? customHeaders = null);
    }

}
