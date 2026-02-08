using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services.Notifications;

public interface INotificationChannel
{
    NotificationChannelType Type { get; }
    Task<NotificationChannelResult> SendAsync(User user, AlertNotification notification, CancellationToken ct = default);
}
