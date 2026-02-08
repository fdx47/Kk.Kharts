using Kk.Kharts.Api.Services.Telegram.Commands.Handlers;
using Kk.Kharts.Shared.Constants;
using Telegram.Bot.Types;

namespace Kk.Kharts.Api.Services.Telegram.Commands.Callbacks;

/// <summary>
/// Handler para callbacks relacionados com dispositivos.
/// </summary>
public sealed class DeviceCallbackHandler(
    IServiceProvider serviceProvider,
    ITelegramService telegram) : ITelegramCallbackHandler
{
    public string CallbackPrefix => TelegramConstants.Callbacks.DevicePrefix;

    public async Task HandleAsync(CallbackQuery callback, CancellationToken ct = default)
    {
        var data = callback.Data ?? string.Empty;
        var devEui = data.Replace(TelegramConstants.Callbacks.DevicePrefix, "");
        var chatId = callback.Message?.Chat.Id ?? 0;

        await telegram.AnswerCallbackQueryAsync(callback.Id, $"Chargement {devEui}...", ct: ct);

        var telegramUserId = callback.From.Id;
        var lastHandler = serviceProvider.GetRequiredService<LastCommandHandler>();
        await lastHandler.ShowLastReadingAsync(chatId, devEui, telegramUserId, ct);
    }
}
