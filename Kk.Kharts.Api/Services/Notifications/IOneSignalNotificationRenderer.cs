using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services.Notifications;

public interface IOneSignalNotificationRenderer
{
    OneSignalPayload Render(AlertNotification notification, User user);
}

public sealed record OneSignalPayload(string Title, string Message);
