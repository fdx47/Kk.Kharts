using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Kk.Kharts.Api.Services.Notifications;

public sealed class TelegramNotificationPayload
{
    public TelegramNotificationPayload(
        string message,
        ParseMode parseMode = ParseMode.Html,
        InlineKeyboardMarkup? markup = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Le message Telegram est obligatoire.", nameof(message));

        Message = message;
        ParseMode = parseMode;
        Markup = markup;
    }

    public string Message { get; }
    public ParseMode ParseMode { get; }
    public InlineKeyboardMarkup? Markup { get; }
}
