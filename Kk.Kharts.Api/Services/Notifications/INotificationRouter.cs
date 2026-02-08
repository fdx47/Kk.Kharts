using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services.Notifications;

public interface INotificationRouter
{
    Task RouteAsync(User user, AlertNotification notification, CancellationToken ct = default);
    Task RouteAsync(IEnumerable<User> users, AlertNotification notification, CancellationToken ct = default);
}
