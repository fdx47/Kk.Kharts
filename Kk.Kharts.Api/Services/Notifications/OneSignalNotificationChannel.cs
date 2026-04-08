using System.Linq;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services.Notifications;

public sealed class OneSignalNotificationChannel : INotificationChannel
{
    private readonly IOneSignalService _service;
    private readonly IOneSignalNotificationRenderer _renderer;
    private readonly ILogger<OneSignalNotificationChannel> _logger;

    public OneSignalNotificationChannel(
        IOneSignalService service,
        IOneSignalNotificationRenderer renderer,
        ILogger<OneSignalNotificationChannel> logger)
    {
        _service = service;
        _renderer = renderer;
        _logger = logger;
    }

    public NotificationChannelType Type => NotificationChannelType.OneSignal;

    public async Task<NotificationChannelResult> SendAsync(User user, AlertNotification notification, CancellationToken ct = default)
    {
        var settings = user.OneSignal;
        if (settings is null || string.IsNullOrWhiteSpace(settings.AppId) || string.IsNullOrWhiteSpace(settings.ApiKey))
        {
            _logger.LogDebug("Paramètres OneSignal incomplets pour l'utilisateur {UserId}", user.Id);
            return NotificationChannelResult.MissingData;
        }

        var targets = CollectTargets(settings);
        if (targets.Length == 0)
        {
            _logger.LogDebug("Aucun device OneSignal configuré pour l'utilisateur {UserId}", user.Id);
            return NotificationChannelResult.MissingData;
        }

        try
        {
            var payload = _renderer.Render(notification, user);
            await _service.SendAsync(settings.AppId!, settings.ApiKey!, targets, payload.Title, payload.Message, ct);
            _logger.LogInformation("Notification OneSignal envoyée à {UserId} sur {Count} device(s)", user.Id, targets.Length);
            return NotificationChannelResult.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Échec de l'envoi OneSignal pour l'utilisateur {UserId}", user.Id);
            return NotificationChannelResult.Failed;
        }
    }

    private static string[] CollectTargets(OneSignalSettings settings)
    {
        return settings.PlayerIds
            .Concat(string.IsNullOrWhiteSpace(settings.PlayerId) ? Array.Empty<string>() : new[] { settings.PlayerId })
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct()
            .ToArray();
    }
}
