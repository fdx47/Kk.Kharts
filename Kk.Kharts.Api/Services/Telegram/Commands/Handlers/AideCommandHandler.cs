using Kk.Kharts.Shared.Constants;
using Telegram.Bot.Types;

namespace Kk.Kharts.Api.Services.Telegram.Commands.Handlers;

/// <summary>
/// Handler pour la commande /aide - Alias de /help en français.
/// </summary>
public sealed class AideCommandHandler(HelpCommandHandler helpHandler) : ITelegramCommandHandler
{
    public string Command => "/aide";
    public string Description => "Message d'aide (alias de /help)";

    public Task HandleAsync(Message message, CancellationToken ct = default)
    {
        return helpHandler.HandleAsync(message, ct);
    }
}
