namespace Kk.Kharts.Api.Services.Notifications;

public interface ITelegramNotificationRenderer
{
    TelegramNotificationPayload Render(AlertNotification notification);
}
