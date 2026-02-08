using Telegram.Bot.Types.ReplyMarkups;

namespace Kk.Kharts.Api.Services.Notifications;

public sealed class TelegramNotificationRenderer : ITelegramNotificationRenderer
{
    public TelegramNotificationPayload Render(AlertNotification notification)
    {
        var markup = BuildMarkup(notification.Actions);
        return new TelegramNotificationPayload(notification.BodyHtml, markup: markup);
    }

    private static InlineKeyboardMarkup? BuildMarkup(IReadOnlyList<NotificationActionRow> rows)
    {
        if (rows.Count == 0)
        {
            return null;
        }

        var inlineRows = new List<InlineKeyboardButton[]>();

        foreach (var row in rows)
        {
            var buttons = new List<InlineKeyboardButton>();

            foreach (var action in row.Actions)
            {
                InlineKeyboardButton button;

                if (action.IsUrl)
                {
                    button = InlineKeyboardButton.WithUrl(action.Label, action.Url!);
                }
                else if (action.IsCallback)
                {
                    button = InlineKeyboardButton.WithCallbackData(action.Label, action.CallbackData!);
                }
                else
                {
                    continue;
                }

                buttons.Add(button);
            }

            if (buttons.Count > 0)
            {
                inlineRows.Add(buttons.ToArray());
            }
        }

        return inlineRows.Count == 0 ? null : new InlineKeyboardMarkup(inlineRows);
    }
}
