using Telegram.Bot.Types;

namespace Kk.Kharts.Api.Services.Telegram.Commands.Handlers;

/// <summary>
/// Aliases en français pour les commandes du bot.
/// </summary>

// /statut -> /status
public sealed class StatutCommandHandler(StatusCommandHandler handler) : ITelegramCommandHandler
{
    public string Command => "/statut";
    public string Description => "État général du système";
    public Task HandleAsync(Message message, CancellationToken ct = default) => handler.HandleAsync(message, ct);
}

// /capteurs -> /devices
public sealed class CapteursCommandHandler(DevicesCommandHandler handler) : ITelegramCommandHandler
{
    public string Command => "/capteurs";
    public string Description => "Liste de vos capteurs";
    public Task HandleAsync(Message message, CancellationToken ct = default) => handler.HandleAsync(message, ct);
}

// /dernier -> /last
public sealed class DernierCommandHandler(LastCommandHandler handler) : ITelegramCommandHandler
{
    public string Command => "/dernier";
    public string Description => "Dernière lecture d'un capteur";
    public Task HandleAsync(Message message, CancellationToken ct = default) => handler.HandleAsync(message, ct);
}

// /horsligne -> /offline
public sealed class HorsligneCommandHandler(OfflineCommandHandler handler) : ITelegramCommandHandler
{
    public string Command => "/horsligne";
    public string Description => "Capteurs hors ligne";
    public Task HandleAsync(Message message, CancellationToken ct = default) => handler.HandleAsync(message, ct);
}

// /batterie -> /battery
public sealed class BatterieCommandHandler(BatteryCommandHandler handler) : ITelegramCommandHandler
{
    public string Command => "/batterie";
    public string Description => "État des batteries";
    public Task HandleAsync(Message message, CancellationToken ct = default) => handler.HandleAsync(message, ct);
}

// /graphique -> /chart
public sealed class GraphiqueCommandHandler(ChartCommandHandler handler) : ITelegramCommandHandler
{
    public string Command => "/graphique";
    public string Description => "Graphique d'un capteur";
    public Task HandleAsync(Message message, CancellationToken ct = default) => handler.HandleAsync(message, ct);
}

// /alertes -> /alerts
public sealed class AlertesCommandHandler(AlertsCommandHandler handler) : ITelegramCommandHandler
{
    public string Command => "/alertes";
    public string Description => "Alertes actives";
    public Task HandleAsync(Message message, CancellationToken ct = default) => handler.HandleAsync(message, ct);
}

// /lier -> /link
public sealed class LierCommandHandler(LinkCommandHandler handler) : ITelegramCommandHandler
{
    public string Command => "/lier";
    public string Description => "Lier votre compte";
    public Task HandleAsync(Message message, CancellationToken ct = default) => handler.HandleAsync(message, ct);
}

// /delier -> /unlink
public sealed class DelierCommandHandler(UnlinkCommandHandler handler) : ITelegramCommandHandler
{
    public string Command => "/delier";
    public string Description => "Délier votre compte";
    public Task HandleAsync(Message message, CancellationToken ct = default) => handler.HandleAsync(message, ct);
}
