using KK.UG6x.StoreAndForward.Domain.DTOs;
using KK.UG6x.StoreAndForward.Domain.Models;

namespace KK.UG6x.StoreAndForward.Domain.Interfaces
{
    public interface IUG65Client
    {
        string BaseUrl { get; }
        Task<List<GatewayPayloadDTO>> FetchLatestDataAsync(CancellationToken cancellationToken);
        Task<List<UG65Device>> GetDevicesAsync(CancellationToken cancellationToken);
        Task<List<UG65Application>> GetApplicationsAsync(CancellationToken cancellationToken);
        Task<UG65Integration?> GetIntegrationAsync(string appId, string type, CancellationToken cancellationToken);
        Task<bool> PurgeUrPacketsAsync(CancellationToken cancellationToken);
    }

}
