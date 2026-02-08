using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services.Notifications;

public interface IPushoverNotificationRenderer
{
    PushoverNotificationPayload Render(AlertNotification notification, User user);
}
