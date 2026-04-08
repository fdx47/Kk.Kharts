using Kk.Kharts.Shared.Entities;
using Kk.Kharts.Shared.Enums;

namespace Kk.Kharts.Api.Services.Notifications;

public sealed class NotificationRouter : INotificationRouter
{
    private readonly IReadOnlyDictionary<NotificationChannelType, INotificationChannel> _channels;
    private readonly ILogger<NotificationRouter> _logger;

    private static readonly (NotificationChannelType Type, NotificationChannelPreference Flag)[] ChannelProcessingOrder =
    {
        (NotificationChannelType.Telegram, NotificationChannelPreference.Telegram),
        (NotificationChannelType.Pushover, NotificationChannelPreference.Pushover),
        (NotificationChannelType.Email, NotificationChannelPreference.Email),
        (NotificationChannelType.OneSignal, NotificationChannelPreference.OneSignal)
    };

    public NotificationRouter(IEnumerable<INotificationChannel> channels, ILogger<NotificationRouter> logger)
    {
        _channels = channels.ToDictionary(channel => channel.Type);
        _logger = logger;
    }

    public async Task RouteAsync(User user, AlertNotification notification, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(notification);

        if (user.NotificationPreference == NotificationChannelPreference.Aucun)
        {
            _logger.LogDebug("Notifications désactivées pour l'utilisateur {UserId}", user.Id);
            return;
        }

        await RoutePreferredChannelsAsync(user, notification, ct);
    }

    public async Task RouteAsync(IEnumerable<User> users, AlertNotification notification, CancellationToken ct = default)
    {
        foreach (var user in users)
        {
            await RouteAsync(user, notification, ct);
        }
    }

    private async Task<NotificationChannelResult> TrySendAsync(
        NotificationChannelType channelType,
        User user,
        AlertNotification notification,
        CancellationToken ct)
    {
        if (!_channels.TryGetValue(channelType, out var channel))
        {
            _logger.LogError(
                "Canal {Channel} non enregistré dans le routeur",
                channelType);
            return NotificationChannelResult.Failed;
        }

        return await channel.SendAsync(user, notification, ct);
    }

    private void LogMissingData(int userId, NotificationChannelType channelType)
    {
        _logger.LogWarning(
            "Impossible de notifier {UserId}: configuration {Channel} incomplète",
            userId,
            channelType);
    }

    private async Task RoutePreferredChannelsAsync(User user, AlertNotification notification, CancellationToken ct)
    {
        var preference = user.NotificationPreference;

        if (!Enum.IsDefined(typeof(NotificationChannelPreference), preference))
        {
            _logger.LogWarning(
                "Préférence de notification inconnue ({Preference}) pour l'utilisateur {UserId}",
                preference,
                user.Id);
            return;
        }

        foreach (var (type, flag) in ChannelProcessingOrder)
        {
            if (!preference.HasFlag(flag))
            {
                continue;
            }

            var result = await TrySendAsync(type, user, notification, ct);

            switch (result)
            {
                case NotificationChannelResult.MissingData:
                    LogMissingData(user.Id, type);
                    break;
                case NotificationChannelResult.Failed:
                    _logger.LogWarning("Envoi {Channel} échoué pour {UserId}", type, user.Id);
                    break;
            }
        }
    }
}
