using Kk.Kharts.Api.Services.Telegram;
using Telegram.Bot.Types.Enums;

namespace Kk.Kharts.Api.Services;

/// <summary>
/// Serviço que envia notificação no Telegram quando a API inicia.
/// </summary>
public sealed class StartupNotificationService(
    ITelegramService telegramService,
    IWebHostEnvironment env,
    ILogger<StartupNotificationService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            var msg = $"""
                🚀 <b>KropKharts API Started</b>

                • <b>Environment:</b> {env.EnvironmentName}
                • <b>Time:</b> {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss} UTC
                """;

            await telegramService.SendToDebugTopicAsync(msg, ParseMode.Html, cancellationToken);
            
            logger.LogInformation("Startup notification sent to Telegram");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send startup notification to Telegram");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
