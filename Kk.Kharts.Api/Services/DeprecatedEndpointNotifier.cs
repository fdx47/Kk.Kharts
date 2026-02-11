using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Services.Telegram;
using Kk.Kharts.Api.Utils;

namespace Kk.Kharts.Api.Services;

/// <summary>
/// Implémentation centralisée de la notification d'endpoints obsolètes.
/// Résout les métadonnées du device (description, entreprise, localisation) et envoie à Telegram.
/// </summary>
public sealed class DeprecatedEndpointNotifier(
    IDeviceService deviceService,
    ITelegramService telegram,
    ILogger<DeprecatedEndpointNotifier> logger) : IDeprecatedEndpointNotifier
{
    public async Task NotifyAsync(string endpoint, string? devEui = null)
    {
        var normalizedDevEui = string.IsNullOrWhiteSpace(devEui)
            ? null
            : DevEuiNormalizer.Normalize(devEui);

        string description = "(sans description)";
        string companyName = "(entreprise inconnue)";
        string location = "(localisation non renseignée)";

        if (!string.IsNullOrWhiteSpace(normalizedDevEui))
        {
            try
            {
                var device = await deviceService.GetDeviceByDevEuiApiKeyInternalAsync(normalizedDevEui);
                if (device is not null)
                {
                    description = device.Description ?? description;
                    companyName = device.Company?.Name ?? device.CompanyId.ToString();
                    location = string.IsNullOrWhiteSpace(device.InstallationLocation)
                        ? location
                        : device.InstallationLocation;
                }
            }
            catch (Exception ex)
            {
                logger.LogDebug(ex, "Impossible d'enrichir la notification obsolète pour {DevEui}", normalizedDevEui);
            }
        }

        var msg = $"⚠️ Endpoint obsolète appelé : {endpoint}\n" +
                  $"DevEui : {normalizedDevEui ?? "(non fourni)"}\n" +
                  $"Description : {description}\n" +
                  $"Entreprise : {companyName}\n" +
                  $"Site d'installation : {location}\n" +
                  $"Horodatage : {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC";

        await telegram.SendToDebugTopicAsync(msg);
    }
}
