using KK.UG6x.StoreAndForward.Domain.DTOs;
using KK.UG6x.StoreAndForward.Domain.Entities;

namespace KK.UG6x.StoreAndForward.Domain.Interfaces
{
    public interface ILocalStore
    {
        Task SavePayloadAsync(GatewayPayloadDTO data, string endpointType);
        Task<List<PendingPayload>> GetPendingPayloadsAsync(int limit);
        Task MarkAsSentAsync(long id);
        Task IncrementRetryCountAsync(long id);
        Task<int> GetPendingCountAsync();
        Task<bool> WasAlreadySentAsync(GatewayPayloadDTO data);
        Task<bool> AlreadyExistsAsync(GatewayPayloadDTO data);
        Task CleanupOldRecordsAsync(int daysRetention);
        Task ClearAllAsync();
    }
}
