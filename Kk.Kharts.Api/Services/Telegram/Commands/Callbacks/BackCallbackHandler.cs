using Kk.Kharts.Api.Services.Telegram.Commands.Handlers;
using Kk.Kharts.Shared.Constants;
using Telegram.Bot.Types;

namespace Kk.Kharts.Api.Services.Telegram.Commands.Callbacks;

/// <summary>
/// Handler para callbacks de navegação (voltar).
/// </summary>
public sealed class BackCallbackHandler(
    IServiceProvider serviceProvider,
    ITelegramService telegram) : ITelegramCallbackHandler
{
    public string CallbackPrefix => "back:";

    public async Task HandleAsync(CallbackQuery callback, CancellationToken ct = default)
    {
        var data = callback.Data ?? string.Empty;
        var target = data.Replace("back:", "");
        var chatId = callback.Message?.Chat.Id ?? 0;

        await telegram.AnswerCallbackQueryAsync(callback.Id, ct: ct);

        var fakeMessage = new Message
        {
            Chat = callback.Message?.Chat!,
            From = callback.From,
            Text = "/start"
        };

        switch (target)
        {
            case "menu":
                var startHandler = serviceProvider.GetRequiredService<StartCommandHandler>();
                await startHandler.HandleAsync(fakeMessage, ct);
                break;

            case "devices":
                var devicesHandler = serviceProvider.GetRequiredService<DevicesCommandHandler>();
                await devicesHandler.HandleAsync(fakeMessage, ct);
                break;

            default:
                var defaultHandler = serviceProvider.GetRequiredService<StartCommandHandler>();
                await defaultHandler.HandleAsync(fakeMessage, ct);
                break;
        }
    }
}
