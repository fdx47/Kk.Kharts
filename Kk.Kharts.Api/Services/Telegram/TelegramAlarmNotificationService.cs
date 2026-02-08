using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Services.Notifications;
using Kk.Kharts.Shared.Constants;
using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;
using System.Globalization;

namespace Kk.Kharts.Api.Services.Telegram;

/// <summary>
/// Service pour envoyer des notifications d'alarmes aux utilisateurs via Telegram.
/// </summary>
public interface ITelegramAlarmNotificationService
{
    /// <summary>
    /// Envoie une notification d'alarme aux utilisateurs concernés.
    /// </summary>
    Task NotifyAlarmTriggeredAsync(AlarmRule alarm, double currentValue, CancellationToken ct = default);

    /// <summary>
    /// Envoie une notification de résolution d'alarme.
    /// </summary>
    Task NotifyAlarmResolvedAsync(AlarmRule alarm, double currentValue, CancellationToken ct = default);

    /// <summary>
    /// Envoie une notification de capteur hors ligne.
    /// </summary>
    Task NotifyDeviceOfflineAsync(Device device, CancellationToken ct = default);

    /// <summary>
    /// Envoie une notification de capteur de retour en ligne.
    /// </summary>
    Task NotifyDeviceOnlineAsync(Device device, CancellationToken ct = default);
}

public class TelegramAlarmNotificationService(
    IServiceScopeFactory scopeFactory,
    ITelegramService telegram,
    ILogger<TelegramAlarmNotificationService> logger,
    INotificationRouter router,
    AlertNotificationFactory alertFactory) : ITelegramAlarmNotificationService
{
    private readonly INotificationRouter _router = router;
    private readonly AlertNotificationFactory _alertFactory = alertFactory;
    public async Task NotifyAlarmTriggeredAsync(AlarmRule alarm, double currentValue, CancellationToken ct = default)
    {
        try
        {
            var users = await GetUsersToNotifyAsync(alarm.DeviceId, ct);

            if (users.Count == 0)
            {
                logger.LogDebug("Aucun utilisateur à notifier pour l'alarme {AlarmId}", alarm.Id);
                return;
            }

            var thresholdType = alarm.ActiveThresholdType == "Low" ? "en dessous" : "au dessus";
            var thresholdValue = alarm.ActiveThresholdType == "Low" ? alarm.LowValue : alarm.HighValue;
            var propertyLabel = GetPropertyLabel(alarm.PropertyName);

            var message = $"""
                {TelegramConstants.Emojis.Warning} <b>ALERTE DÉCLENCHÉE</b>
                
                {TelegramConstants.Emojis.Device} <b>{telegram.EscapeHtml(alarm.Device?.Name ?? alarm.DevEui)}</b>
                {TelegramConstants.Emojis.Farm} {telegram.EscapeHtml(alarm.Device?.Company?.Name ?? "N/A")}
                
                ━━━━━━━━━━━━━━━━━━━━━━
                
                {propertyLabel} est {thresholdType} du seuil
                
                📊 <b>Valeur actuelle:</b> {currentValue:F1}
                ⚠️ <b>Seuil:</b> {thresholdValue:F1}
                
                ━━━━━━━━━━━━━━━━━━━━━━
                
                {TelegramConstants.Emojis.Clock} {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC
                """;

            var pushoverMessage = $"Alerte déclenchée sur {alarm.Device?.Name ?? alarm.DevEui} : {propertyLabel} est {thresholdType} du seuil. Valeur {currentValue:F1} / Seuil {thresholdValue:F1}.";

            var actions = new[]
            {
                NotificationActionRow.FromActions(
                    new NotificationAction($"{TelegramConstants.Emojis.Chart} Voir Graphique", CallbackData: $"chart:select:{alarm.DevEui}"),
                    new NotificationAction($"{TelegramConstants.Emojis.BellOff} Silencier 1h", CallbackData: $"alert:mute:{alarm.Id}:1h")),
                NotificationActionRow.FromActions(
                    new NotificationAction($"{TelegramConstants.Emojis.Device} Voir Capteur", CallbackData: $"device:{alarm.DevEui}"))
            };

            var notification = new AlertNotification(
                title: "Alerte déclenchée",
                bodyHtml: message,
                bodyText: pushoverMessage,
                occurredAt: DateTimeOffset.UtcNow,
                actions: actions);

            await _router.RouteAsync(users, notification, ct);

            logger.LogInformation(
                "Notification d'alarme dispatchée pour {DevEui} à {Count} utilisateur(s)",
                alarm.DevEui,
                users.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de l'envoi de notification d'alarme {AlarmId}", alarm.Id);
        }
    }

    public async Task NotifyAlarmResolvedAsync(AlarmRule alarm, double currentValue, CancellationToken ct = default)
    {
        try
        {
            var users = await GetUsersToNotifyAsync(alarm.DeviceId, ct);

            if (users.Count == 0) return;

            var propertyLabel = GetPropertyLabel(alarm.PropertyName);

            var message = $"""
                {TelegramConstants.Emojis.Success} <b>ALERTE RÉSOLUE</b>
                
                {TelegramConstants.Emojis.Device} <b>{telegram.EscapeHtml(alarm.Device?.Name ?? alarm.DevEui)}</b>
                
                {propertyLabel} est revenu dans les limites normales.
                
                📊 <b>Valeur actuelle:</b> {currentValue:F1}
                
                {TelegramConstants.Emojis.Clock} {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC
                """;

            var pushoverMessage = $"Alerte résolue sur {alarm.Device?.Name ?? alarm.DevEui} : {propertyLabel} est revenu à {currentValue:F1}.";

            var notification = new AlertNotification(
                title: "Alerte résolue",
                bodyHtml: message,
                bodyText: pushoverMessage,
                occurredAt: DateTimeOffset.UtcNow);

            await _router.RouteAsync(users, notification, ct);

            logger.LogInformation(
                "Notification de résolution dispatchée pour {DevEui} à {Count} utilisateur(s)",
                alarm.DevEui,
                users.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de l'envoi de notification de résolution {AlarmId}", alarm.Id);
        }
    }

    public async Task NotifyDeviceOfflineAsync(Device device, CancellationToken ct = default)
    {
        try
        {
            var users = await GetUsersToNotifyAsync(device.Id, ct);

            if (users.Count == 0) return;

            var notification = _alertFactory.BuildOfflineDeviceNotification(device);

            await _router.RouteAsync(users, notification, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de l'envoi de notification offline pour {DevEui}", device.DevEui);
        }
    }

    public async Task NotifyDeviceOnlineAsync(Device device, CancellationToken ct = default)
    {
        try
        {
            var users = await GetUsersToNotifyAsync(device.Id, ct);

            if (users.Count == 0) return;

            var message = $"""
                {TelegramConstants.Emojis.Success} <b>CAPTEUR EN LIGNE</b>
                
                {TelegramConstants.Emojis.Device} <b>{telegram.EscapeHtml(device.Name)}</b>
                
                Le capteur a repris la communication.
                
                {TelegramConstants.Emojis.Clock} {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC
                """;

            var pushoverMessage = $"""
                ✅ CAPTEUR EN LIGNE

                📡 {device.Name}
                🏡 {device.Company?.Name ?? "N/A"}

                Le capteur a repris la communication.
                🕐 {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC
                """.Trim();

            var notification = new AlertNotification(
                title: "Capteur en ligne",
                bodyHtml: message,
                bodyText: pushoverMessage,
                occurredAt: DateTimeOffset.UtcNow);

            await _router.RouteAsync(users, notification, ct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de l'envoi de notification online pour {DevEui}", device.DevEui);
        }
    }

    private async Task<List<User>> GetUsersToNotifyAsync(int deviceId, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var device = await db.Devices
            .Include(d => d.Company)
            .FirstOrDefaultAsync(d => d.Id == deviceId, ct);

        if (device == null) return [];

        // Récupérer tous les utilisateurs avec Telegram lié qui ont accès à ce device
        var users = await db.Users
            .Include(u => u.Company)
            .Include(u => u.Pushover)
            .ToListAsync(ct);

        // Filtrer par accès
        var usersWithAccess = new List<User>();

        foreach (var user in users)
        {
            // Déterminer l'étendue d'accès depuis l'AccessLevel (plutôt que le rôle)
            var accessLevel = user.AccessLevel;

            if (accessLevel == Kk.Kharts.Shared.Enums.UserAccessLevel.Root)
            {
                usersWithAccess.Add(user);
                continue;
            }

            var companyIds = new List<int> { user.CompanyId };

            if (accessLevel == Kk.Kharts.Shared.Enums.UserAccessLevel.CompanyAndSubsidiaries)
            {
                var subsidiaryIds = await db.Companies
                    .Where(c => c.ParentCompanyId == user.CompanyId)
                    .Select(c => c.Id)
                    .ToListAsync(ct);

                companyIds.AddRange(subsidiaryIds);
            }

            if (companyIds.Contains(device.CompanyId))
            {
                usersWithAccess.Add(user);
            }
        }

        return usersWithAccess;
    }

    private static string GetPropertyLabel(string? propertyName) => propertyName?.ToLowerInvariant() switch
    {
        "temperature" or "soiltemperature" => $"{TelegramConstants.Emojis.Thermometer} Température",
        "humidity" => $"{TelegramConstants.Emojis.Droplet} Humidité",
        "mineralvwc" or "organicvwc" => $"{TelegramConstants.Emojis.Droplet} VWC",
        "mineralecp" or "organicecp" => $"{TelegramConstants.Emojis.Lightning} EC",
        "battery" => $"{TelegramConstants.Emojis.Battery} Batterie",
        "permittivite" => "📊 Permittivité",
        _ => $"📊 {propertyName}"
    };
}
