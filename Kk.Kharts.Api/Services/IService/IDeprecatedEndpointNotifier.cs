namespace Kk.Kharts.Api.Services.IService;

/// <summary>
/// Notifie l'équipe via Telegram lorsqu'un endpoint obsolète est appelé.
/// Remplace les méthodes privées NotifyDeprecatedUsageAsync dupliquées dans les controllers.
/// </summary>
public interface IDeprecatedEndpointNotifier
{
    /// <summary>
    /// Envoie une notification d'utilisation d'endpoint obsolète, enrichie des métadonnées du device si disponible.
    /// </summary>
    Task NotifyAsync(string endpoint, string? devEui = null);
}
