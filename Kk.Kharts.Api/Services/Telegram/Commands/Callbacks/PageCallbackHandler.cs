using Kk.Kharts.Api.Services.Telegram.Commands.Handlers;
using Kk.Kharts.Shared.Constants;
using Telegram.Bot.Types;

namespace Kk.Kharts.Api.Services.Telegram.Commands.Callbacks;

/// <summary>
/// Handler para callbacks de paginação.
/// </summary>
public sealed class PageCallbackHandler(
    IServiceProvider serviceProvider,
    ITelegramService telegram) : ITelegramCallbackHandler
{
    public string CallbackPrefix => TelegramConstants.Callbacks.PagePrefix;

    public async Task HandleAsync(CallbackQuery callback, CancellationToken ct = default)
    {
        var data = callback.Data ?? string.Empty;
        // Format: page:devices:2 ou page:alerts:3
        var parts = data.Replace(TelegramConstants.Callbacks.PagePrefix, "").Split(':');
        
        if (parts.Length < 2)
        {
            await telegram.AnswerCallbackQueryAsync(callback.Id, "⚠️ Format invalide", ct: ct);
            return;
        }

        var target = parts[0];
        var page = int.TryParse(parts[1], out var p) ? p : 1;

        await telegram.AnswerCallbackQueryAsync(callback.Id, $"Page {page}...", ct: ct);

        // Criar mensagem fake para reutilizar handlers
        var fakeMessage = new Message
        {
            Chat = callback.Message?.Chat!,
            From = callback.From,
            Text = target switch
            {
                "devices" => "/devices",
                "alerts" => "/alerts",
                _ => "/start"
            }
        };

        // Por agora, apenas redireciona para o handler principal
        // TODO: Implementar paginação real nos handlers
        switch (target)
        {
            case "devices":
                var devicesHandler = serviceProvider.GetRequiredService<DevicesCommandHandler>();
                await devicesHandler.HandleAsync(fakeMessage, ct);
                break;

            case "alerts":
                var alertsHandler = serviceProvider.GetRequiredService<AlertsCommandHandler>();
                await alertsHandler.HandleAsync(fakeMessage, ct);
                break;

            default:
                var startHandler = serviceProvider.GetRequiredService<StartCommandHandler>();
                await startHandler.HandleAsync(fakeMessage, ct);
                break;
        }
    }
}
