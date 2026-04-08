using Kk.Kharts.Api.Services.IService;
using OneSignal.RestAPIv3.Client;
using OneSignal.RestAPIv3.Client.Resources.Notifications;

namespace Kk.Kharts.Api.Services.Notifications;

public sealed class OneSignalService : IOneSignalService
{
    public async Task SendAsync(string appId, string apiKey, IEnumerable<string> playerIds, string title, string message, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(appId)) throw new ArgumentException("AppId requis", nameof(appId));
        if (string.IsNullOrWhiteSpace(apiKey)) throw new ArgumentException("ApiKey requise", nameof(apiKey));
        var targets = playerIds?.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct().ToArray() ?? Array.Empty<string>();
        if (targets.Length == 0) throw new ArgumentException("PlayerIds requis", nameof(playerIds));

        if (!Guid.TryParse(appId, out var appGuid))
        {
            throw new ArgumentException("AppId doit être un GUID", nameof(appId));
        }

        var client = new OneSignalClient(apiKey);
        var options = new NotificationCreateOptions
        {
            AppId = appGuid,
            IncludePlayerIds = targets.ToList(),
            Headings = new Dictionary<string, string> { { "en", title } },
            Contents = new Dictionary<string, string> { { "en", message } }
        };

        await Task.Run(() => client.Notifications.Create(options), ct).ConfigureAwait(false);
    }
}
