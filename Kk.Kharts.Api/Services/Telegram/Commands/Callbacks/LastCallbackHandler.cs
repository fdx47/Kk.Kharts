using Kk.Kharts.Api.Services.Telegram.Commands.Handlers;
using Kk.Kharts.Api.Utils;
using Kk.Kharts.Shared.Constants;
using Telegram.Bot.Types;

namespace Kk.Kharts.Api.Services.Telegram.Commands.Callbacks;

/// <summary>
/// Handler pour les callbacks de dernière lecture.
/// Format: last:{devEui}
/// </summary>
public sealed class LastCallbackHandler(
    IServiceProvider serviceProvider,
    ITelegramService telegram) : ITelegramCallbackHandler
{
    public string CallbackPrefix => "last:";

    public async Task HandleAsync(CallbackQuery callback, CancellationToken ct = default)
    {
        var data = callback.Data ?? string.Empty;
        var devEui = DevEuiNormalizer.Normalize(data.Replace("last:", ""));

        if (string.IsNullOrEmpty(devEui))
        {
            await telegram.AnswerCallbackQueryAsync(callback.Id, "⚠️ DevEui manquant", true, ct: ct);
            return;
        }

        await telegram.AnswerCallbackQueryAsync(callback.Id, "Chargement...", ct: ct);

        var chatId = callback.Message?.Chat.Id ?? 0;
        var telegramUserId = callback.From.Id;

        var lastHandler = serviceProvider.GetRequiredService<LastCommandHandler>();
        await lastHandler.ShowLastReadingAsync(chatId, devEui, telegramUserId, ct);
    }
}
