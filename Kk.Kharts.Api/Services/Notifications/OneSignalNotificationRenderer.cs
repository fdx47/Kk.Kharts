using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services.Notifications;

public sealed class OneSignalNotificationRenderer : IOneSignalNotificationRenderer
{
    public OneSignalPayload Render(AlertNotification notification, User user)
    {
        var settings = user.OneSignal ?? throw new InvalidOperationException("Paramètres OneSignal manquants pour l'utilisateur.");

        var message = string.IsNullOrWhiteSpace(settings.MessageTemplate)
            ? notification.BodyText
            : settings.MessageTemplate.Replace("{message}", notification.BodyText, StringComparison.OrdinalIgnoreCase);

        var title = string.IsNullOrWhiteSpace(settings.Title) ? notification.Title : settings.Title;

        return new OneSignalPayload(title ?? notification.Title, message);
    }
}
