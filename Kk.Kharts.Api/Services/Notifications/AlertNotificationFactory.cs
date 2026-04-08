using Kk.Kharts.Api.Services.Telegram;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.Constants;
using Kk.Kharts.Shared.Entities;
using System.Globalization;
using System.Net;

namespace Kk.Kharts.Api.Services.Notifications;

/// <summary>
/// Fabrica payloads de AlertNotification e mantém a formatação coerente entre canais.
/// </summary>
public sealed class AlertNotificationFactory
{
    private readonly ITelegramService _telegram;
    private readonly IKkTimeZoneService _timeZoneService;

    public AlertNotificationFactory(ITelegramService telegram, IKkTimeZoneService timeZoneService)
    {
        _telegram = telegram;
        _timeZoneService = timeZoneService;
    }

    /// <summary>
    /// Cria o payload reutilizável para notificações de sensor offline.
    /// Usa HTML para Telegram e texto plano para os demais canais.
    /// </summary>
    public AlertNotification BuildOfflineDeviceNotification(Device device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var summary = BuildDeviceSummary(device);

        var html = $"""
            {TelegramConstants.Emojis.Warning} <b>CAPTEUR HORS LIGNE</b>
            
            {TelegramConstants.Emojis.Device} <b>{_telegram.EscapeHtml(device.Name)}</b>
            {TelegramConstants.Emojis.Farm} {_telegram.EscapeHtml(device.Company?.Name ?? "N/A")}
            {TelegramConstants.Emojis.Info} Description : {summary.DescriptionHtml}
            📍 Installation : {summary.LocationHtml}
            
            Le capteur n'a pas communiqué depuis plus de 2 heures.
            
            {TelegramConstants.Emojis.Clock} Dernière communication: {summary.LastSeenLabel}
            """;

        var text = $"""
            ⚠️ CAPTEUR HORS LIGNE

            📡 {device.Name}
            ℹ️ {device.Description ?? "—"}
            📍 {device.InstallationLocation ?? "—"}
            🏡 {device.Company?.Name ?? "N/A"}

            Le capteur n'a pas communiqué depuis plus de 2 heures.
            🕐 Dernière communication: {summary.LastSeenLabel}
            """.Trim();

        var actions = new[]
        {
            NotificationActionRow.FromActions(
                new NotificationAction(
                    Label: $"{TelegramConstants.Emojis.Device} Voir Détails",
                    CallbackData: $"device:{device.DevEui}"))
        };

        return new AlertNotification(
            title: "Capteur hors ligne",
            bodyHtml: html,
            bodyText: text,
            occurredAt: DateTimeOffset.UtcNow,
            actions: actions);
    }

    /// <summary>
    /// Fabrica uma notificação genérica para sensores que voltaram a comunicar.
    /// </summary>
    public AlertNotification BuildDeviceOnlineNotification(Device device)
    {
        ArgumentNullException.ThrowIfNull(device);

        var html = $"""
            {TelegramConstants.Emojis.Success} <b>CAPTEUR EN LIGNE</b>
            
            {TelegramConstants.Emojis.Device} <b>{_telegram.EscapeHtml(device.Name)}</b>
            {TelegramConstants.Emojis.Farm} {_telegram.EscapeHtml(device.Company?.Name ?? "N/A")}
            
            Le capteur a repris la communication.
            
            {TelegramConstants.Emojis.Clock} {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC
            """;

        var text = $"""
            ✅ CAPTEUR EN LIGNE

            📡 {device.Name}
            🏡 {device.Company?.Name ?? "N/A"}

            Le capteur a repris la communication.
            🕐 {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC
            """.Trim();

        return new AlertNotification(
            title: "Capteur en ligne",
            bodyHtml: html,
            bodyText: text,
            occurredAt: DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Monta a mensagem padrão quando uma regra de alarme é disparada.
    /// </summary>
    public AlertNotification BuildAlarmTriggeredNotification(AlarmRule alarm, double currentValue)
    {
        ArgumentNullException.ThrowIfNull(alarm);

        var (thresholdType, thresholdValue) = GetThresholdInfo(alarm);
        var propertyLabel = GetPropertyLabel(alarm.PropertyName);
        var deviceLabel = alarm.Device?.Name ?? alarm.DevEui;
        var companyLabel = alarm.Device?.Company?.Name ?? "N/A";

        var html = $"""
            {TelegramConstants.Emojis.Warning} <b>ALERTE DÉCLENCHÉE</b>
            
            {TelegramConstants.Emojis.Device} <b>{_telegram.EscapeHtml(deviceLabel)}</b>
            {TelegramConstants.Emojis.Farm} {_telegram.EscapeHtml(companyLabel)}
            
            ━━━━━━━━━━━━━━━━━━━━━━
            
            {propertyLabel} est {thresholdType} du seuil
            
            📊 <b>Valeur actuelle:</b> {currentValue:F1}
            ⚠️ <b>Seuil:</b> {thresholdValue:F1}
            
            ━━━━━━━━━━━━━━━━━━━━━━
            
            {TelegramConstants.Emojis.Clock} {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC
            """;

        var text = $"Alerte déclenchée sur {deviceLabel} : {propertyLabel} est {thresholdType} du seuil. Valeur {currentValue:F1} / Seuil {thresholdValue:F1}.";

        var actions = new[]
        {
            NotificationActionRow.FromActions(
                new NotificationAction($"{TelegramConstants.Emojis.Chart} Voir Graphique", CallbackData: $"chart:select:{alarm.DevEui}"),
                new NotificationAction($"{TelegramConstants.Emojis.BellOff} Silencier 1h", CallbackData: $"alert:mute:{alarm.Id}:1h")),
            NotificationActionRow.FromActions(
                new NotificationAction($"{TelegramConstants.Emojis.Device} Voir Capteur", CallbackData: $"device:{alarm.DevEui}"))
        };

        return new AlertNotification(
            title: "Alerte déclenchée",
            bodyHtml: html,
            bodyText: text,
            occurredAt: DateTimeOffset.UtcNow,
            actions: actions);
    }

    /// <summary>
    /// Monta a notificação enviada quando o alarme volta ao estado normal.
    /// </summary>
    public AlertNotification BuildAlarmResolvedNotification(AlarmRule alarm, double currentValue)
    {
        ArgumentNullException.ThrowIfNull(alarm);

        var propertyLabel = GetPropertyLabel(alarm.PropertyName);
        var deviceLabel = alarm.Device?.Name ?? alarm.DevEui;

        var html = $"""
            {TelegramConstants.Emojis.Success} <b>ALERTE RÉSOLUE</b>
            
            {TelegramConstants.Emojis.Device} <b>{_telegram.EscapeHtml(deviceLabel)}</b>
            
            {propertyLabel} est revenu dans les limites normales.
            
            📊 <b>Valeur actuelle:</b> {currentValue:F1}
            
            {TelegramConstants.Emojis.Clock} {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC
            """;

        var text = $"Alerte résolue sur {deviceLabel} : {propertyLabel} est revenu à {currentValue:F1}.";

        return new AlertNotification(
            title: "Alerte résolue",
            bodyHtml: html,
            bodyText: text,
            occurredAt: DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Constrói uma notificação mínima (texto único) para testes ou mensagens livres.
    /// </summary>
    public AlertNotification CreateMinimal(string title, string bodyText)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(bodyText);

        var safeHtml = WebUtility.HtmlEncode(bodyText).Replace("\n", "<br/>");

        return new AlertNotification(
            title: title,
            bodyHtml: safeHtml,
            bodyText: bodyText,
            occurredAt: DateTimeOffset.UtcNow);
    }

    private DeviceSummary BuildDeviceSummary(Device device)
    {
        var description = string.IsNullOrWhiteSpace(device.Description)
            ? "—"
            : _telegram.EscapeHtml(device.Description);

        var location = string.IsNullOrWhiteSpace(device.InstallationLocation)
            ? "—"
            : _telegram.EscapeHtml(device.InstallationLocation);

        var lastSeenUtc = device.LastSeenAt.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(device.LastSeenAt, DateTimeKind.Utc)
            : device.LastSeenAt;

        var lastSeenParis = _timeZoneService.ConvertToParisTime(lastSeenUtc);
        var lastSeenLabel = lastSeenParis.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture) + " (heure de Paris)";

        return new DeviceSummary(description, location, lastSeenLabel);
    }

    private static (string ThresholdTypeLabel, double ThresholdValue) GetThresholdInfo(AlarmRule alarm)
    {
        var isLow = string.Equals(alarm.ActiveThresholdType, "Low", StringComparison.OrdinalIgnoreCase);
        var typeLabel = isLow ? "en dessous" : "au dessus";
        var value = isLow ? alarm.LowValue : alarm.HighValue;
        return (typeLabel, value ?? 0);
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

    private sealed record DeviceSummary(string DescriptionHtml, string LocationHtml, string LastSeenLabel);
}
