using Kk.Kharts.Api.Services.Telegram.Commands.Handlers;
using Kk.Kharts.Shared.Constants;
using Telegram.Bot.Types;

namespace Kk.Kharts.Api.Services.Telegram.Commands.Callbacks;

/// <summary>
/// Handler para callbacks de menu principal.
/// </summary>
public sealed class MenuCallbackHandler(
    IServiceProvider serviceProvider,
    ITelegramService telegram) : ITelegramCallbackHandler
{
    public string CallbackPrefix => "menu:";

    public async Task HandleAsync(CallbackQuery callback, CancellationToken ct = default)
    {
        var data = callback.Data ?? string.Empty;
        var action = data.Replace("menu:", "");
        var chatId = callback.Message?.Chat.Id ?? 0;

        // Criar uma mensagem fake para reutilizar os handlers existentes
        var fakeMessage = new Message
        {
            Chat = callback.Message?.Chat!,
            From = callback.From,
            Text = action switch
            {
                "devices" => "/devices",
                "status" => "/status",
                "charts" or "chart" => "/chart",
                "alerts" => "/alerts",
                "battery" => "/battery",
                "offline" => "/offline",
                "help" => "/help",
                _ => "/start"
            }
        };

        await telegram.AnswerCallbackQueryAsync(callback.Id, ct: ct);

        // Executar o handler apropriado
        switch (action)
        {
            case "devices":
                var devicesHandler = serviceProvider.GetRequiredService<DevicesCommandHandler>();
                await devicesHandler.HandleAsync(fakeMessage, ct);
                break;

            case "status":
                var statusHandler = serviceProvider.GetRequiredService<StatusCommandHandler>();
                await statusHandler.HandleAsync(fakeMessage, ct);
                break;

            case "charts":
            case "chart":
                var chartHandler = serviceProvider.GetRequiredService<ChartCommandHandler>();
                await chartHandler.HandleAsync(fakeMessage, ct);
                break;

            case "alerts":
                var alertsHandler = serviceProvider.GetRequiredService<AlertsCommandHandler>();
                await alertsHandler.HandleAsync(fakeMessage, ct);
                break;

            case "battery":
                var batteryHandler = serviceProvider.GetRequiredService<BatteryCommandHandler>();
                await batteryHandler.HandleAsync(fakeMessage, ct);
                break;

            case "offline":
                var offlineHandler = serviceProvider.GetRequiredService<OfflineCommandHandler>();
                await offlineHandler.HandleAsync(fakeMessage, ct);
                break;

            case "help":
                var helpHandler = serviceProvider.GetRequiredService<HelpCommandHandler>();
                await helpHandler.HandleAsync(fakeMessage, ct);
                break;

            case "app":
                var appHandler = serviceProvider.GetRequiredService<AppCommandHandler>();
                await appHandler.HandleAsync(fakeMessage, ct);
                break;

            default:
                var startHandler = serviceProvider.GetRequiredService<StartCommandHandler>();
                await startHandler.HandleAsync(fakeMessage, ct);
                break;
        }
    }
}
