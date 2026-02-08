using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services.Notifications;

public sealed class PushoverNotificationChannel : INotificationChannel
{
    private readonly IPushoverService _pushoverService;
    private readonly IPushoverNotificationRenderer _renderer;
    private readonly ILogger<PushoverNotificationChannel> _logger;

    public PushoverNotificationChannel(
        IPushoverService pushoverService,
        IPushoverNotificationRenderer renderer,
        ILogger<PushoverNotificationChannel> logger)
    {
        _pushoverService = pushoverService;
        _renderer = renderer;
        _logger = logger;
    }

    public NotificationChannelType Type => NotificationChannelType.Pushover;

    public async Task<NotificationChannelResult> SendAsync(User user, AlertNotification notification, CancellationToken ct = default)
    {
        var settings = user.Pushover;

        if (settings is null ||
            string.IsNullOrWhiteSpace(settings.AppToken) ||
            string.IsNullOrWhiteSpace(settings.UserKey))
        {
            _logger.LogDebug("Paramètres Pushover absents ou incomplets pour l'utilisateur {UserId}", user.Id);
            return NotificationChannelResult.MissingData;
        }

        try
        {
            var payload = _renderer.Render(notification, user);

            var options = new PushoverMessageOptions
            {
                AppToken = settings.AppToken!,
                UserKey = settings.UserKey!,
                Message = payload.Message,
                Title = payload.Title,
                Sound = payload.Sound,
                Device = payload.Device,
                Priority = payload.Priority,
                RetrySeconds = payload.RetrySeconds,
                ExpireSeconds = payload.ExpireSeconds
            };

            await _pushoverService.SendAsync(options, ct);

            _logger.LogInformation("Notification Pushover envoyée à {UserId}", user.Id);
            return NotificationChannelResult.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Échec de l'envoi Pushover pour l'utilisateur {UserId}", user.Id);
            return NotificationChannelResult.Failed;
        }
    }
}
