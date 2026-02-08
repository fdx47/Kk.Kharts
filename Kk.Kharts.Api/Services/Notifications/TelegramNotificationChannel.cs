using Kk.Kharts.Api.Services.Telegram;
using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services.Notifications;

public sealed class TelegramNotificationChannel : INotificationChannel
{
    private readonly ITelegramService _telegramService;
    private readonly ITelegramNotificationRenderer _renderer;
    private readonly ILogger<TelegramNotificationChannel> _logger;

    public TelegramNotificationChannel(
        ITelegramService telegramService,
        ITelegramNotificationRenderer renderer,
        ILogger<TelegramNotificationChannel> logger)
    {
        _telegramService = telegramService;
        _renderer = renderer;
        _logger = logger;
    }

    public NotificationChannelType Type => NotificationChannelType.Telegram;

    public async Task<NotificationChannelResult> SendAsync(User user, AlertNotification notification, CancellationToken ct = default)
    {
        if (!user.TelegramUserId.HasValue)
        {
            _logger.LogDebug("Telegram non lié pour l'utilisateur {UserId}", user.Id);
            return NotificationChannelResult.MissingData;
        }

        try
        {
            var payload = _renderer.Render(notification);

            await _telegramService.SendToChatAsync(
                chatId: user.TelegramUserId.Value,
                message: payload.Message,
                parseMode: payload.ParseMode,
                replyMarkup: payload.Markup,
                ct: ct);

            _logger.LogInformation(
                "Notification Telegram envoyée à {UserId} ({TelegramId})",
                user.Id,
                user.TelegramUserId);

            return NotificationChannelResult.Succeeded;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Échec de l'envoi Telegram pour l'utilisateur {UserId}", user.Id);
            return NotificationChannelResult.Failed;
        }
    }
}
