using Kk.Kharts.Api.Services.Telegram.Commands.Handlers;
using Kk.Kharts.Shared.Constants;
using Telegram.Bot.Types;

namespace Kk.Kharts.Api.Services.Telegram.Commands.Callbacks;

/// <summary>
/// Handler para callbacks de atualização (refresh).
/// Formato: refresh:{target}
/// </summary>
public sealed class RefreshCallbackHandler(
    IServiceProvider serviceProvider,
    ITelegramService telegram) : ITelegramCallbackHandler
{
    public string CallbackPrefix => TelegramConstants.Callbacks.RefreshPrefix;

    public async Task HandleAsync(CallbackQuery callback, CancellationToken ct = default)
    {
        var data = callback.Data ?? string.Empty;
        var target = data.Replace(TelegramConstants.Callbacks.RefreshPrefix, "");

        await telegram.AnswerCallbackQueryAsync(callback.Id, "🔄 Actualisation...", ct: ct);

        var fakeMessage = new Message
        {
            Chat = callback.Message?.Chat!,
            From = callback.From,
            Text = $"/{target}"
        };

        switch (target)
        {
            case "alerts":
                var alertsHandler = serviceProvider.GetRequiredService<AlertsCommandHandler>();
                await alertsHandler.HandleAsync(fakeMessage, ct);
                break;

            case "devices":
                var devicesHandler = serviceProvider.GetRequiredService<DevicesCommandHandler>();
                await devicesHandler.HandleAsync(fakeMessage, ct);
                break;

            case "status":
                var statusHandler = serviceProvider.GetRequiredService<StatusCommandHandler>();
                await statusHandler.HandleAsync(fakeMessage, ct);
                break;

            case "battery":
                var batteryHandler = serviceProvider.GetRequiredService<BatteryCommandHandler>();
                await batteryHandler.HandleAsync(fakeMessage, ct);
                break;

            case "offline":
                var offlineHandler = serviceProvider.GetRequiredService<OfflineCommandHandler>();
                await offlineHandler.HandleAsync(fakeMessage, ct);
                break;

            default:
                var startHandler = serviceProvider.GetRequiredService<StartCommandHandler>();
                await startHandler.HandleAsync(fakeMessage, ct);
                break;
        }
    }
}
