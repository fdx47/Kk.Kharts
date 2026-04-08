using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services.IService;

public interface IOneSignalService
{
    Task SendAsync(string appId, string apiKey, IEnumerable<string> playerIds, string title, string message, CancellationToken ct = default);
}
