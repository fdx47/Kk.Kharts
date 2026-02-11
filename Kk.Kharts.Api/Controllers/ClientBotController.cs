// Bot Cliente (kropkontrol_bot) - Para utilizadores finais
// Webhook URL: https://kropkontrol.premiumasp.net/api/v1/webhooks/client/{secret}

using Kk.Kharts.Api.Services.Telegram;
using Kk.Kharts.Api.Services.Telegram.Commands;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Telegram.Bot.Types;

namespace Kk.Kharts.Api.Controllers;

/// <summary>
/// Controller para o Bot Cliente (kropkontrol_bot).
/// Comandos para utilizadores finais: /start, /help, /devices, /chart, /link, /unlink, etc.
/// Toute la logique métier est déléguée au TelegramCommandDispatcher.
/// </summary>
[ApiController]
[Route("api/v1/webhooks")]
[ApiExplorerSettings(IgnoreApi = true)]
public sealed class ClientBotController(IServiceScopeFactory scopeFactory, IOptions<TelegramOptions> telegramOptions, ILogger<ClientBotController> logger) : ControllerBase
{
    private readonly string _webhookSecret = telegramOptions.Value.ClientWebhookSecret ?? throw new InvalidOperationException("ClientWebhookSecret non configuré");

    /// <summary>
    /// Point d'entrée pour le bot client (kropkontrol_bot).
    /// </summary>
    /// <param name="secret">Secret de validation du webhook</param>
    /// <param name="update">Mise à jour Telegram</param>
    /// <param name="ct">Token d'annulation</param>
    /// <returns>OK dans tous les cas pour éviter les retries Telegram</returns>
    [HttpPost("client/{secret}")]
    public async Task<IActionResult> HandleClientBotUpdate(string secret, [FromBody] Update update, CancellationToken ct = default)
    {
        if (!IsValidSecret(secret))
        {
            logger.LogWarning("Client bot: tentative d'accès avec secret invalide depuis {IP}", 
                HttpContext.Connection.RemoteIpAddress);
            return Unauthorized();
        }

        try
        {
            await using var scope = scopeFactory.CreateAsyncScope();
            var dispatcher = scope.ServiceProvider.GetRequiredService<TelegramCommandDispatcher>();
            
            await dispatcher.ProcessUpdateAsync(update, ct);
        }
        catch (OperationCanceledException)
        {
            //logger.LogDebug("Client bot: traitement annulé pour update {UpdateId}", update.Id);
        }
        //catch (Exception ex)
        {
            //logger.LogError(ex, "Client bot: erreur lors du traitement de l'update {UpdateId}", update.Id);
        }

        return Ok();
    }

    private bool IsValidSecret(string secret) => string.Equals(secret, _webhookSecret, StringComparison.Ordinal);
}
