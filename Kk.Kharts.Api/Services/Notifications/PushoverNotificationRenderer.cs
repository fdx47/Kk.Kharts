using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services.Notifications;

public sealed class PushoverNotificationRenderer : IPushoverNotificationRenderer
{
    public PushoverNotificationPayload Render(AlertNotification notification, User user)
    {
        var settings = user.Pushover ?? throw new InvalidOperationException("L'utilisateur ne dispose pas de paramètres Pushover.");

        var message = BuildMessage(notification, settings);
        var title = Normalize(settings.Title) ?? notification.Title;

        return new PushoverNotificationPayload(
            title: title,
            message: message,
            sound: Normalize(settings.Sound),
            device: Normalize(settings.Device),
            priority: settings.Priority,
            retrySeconds: settings.RetrySeconds,
            expireSeconds: settings.ExpireSeconds);
    }

    private static string BuildMessage(AlertNotification notification, PushoverSettings settings)
    {
        var baseMessage = notification.BodyText;

        if (string.IsNullOrWhiteSpace(settings.MessageTemplate))
        {
            return baseMessage;
        }

        var template = settings.MessageTemplate.Trim();

        if (template.Contains("{message}", StringComparison.OrdinalIgnoreCase))
        {
            return template.Replace("{message}", baseMessage, StringComparison.OrdinalIgnoreCase);
        }

        return string.Join(Environment.NewLine + Environment.NewLine, template, baseMessage);
    }

    private static string? Normalize(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
