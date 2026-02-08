using Kk.Kharts.Api.Services.Notifications;

namespace Kk.Kharts.Api.Tests.Helpers;

/// <summary>
/// Factory pour créer des notifications d'alerte pour les tests.
/// Permet de générer différents types de notifications (device offline, alarme, etc.).
/// </summary>
public static class TestAlertNotificationFactory
{
    /// <summary>
    /// Crée une notification simple pour les tests unitaires.
    /// </summary>
    public static AlertNotification CreateSimpleNotification(
        string title = "Test Alert",
        string bodyHtml = "<b>Test</b> notification",
        string bodyText = "Test notification")
    {
        return new AlertNotification(
            title: title,
            bodyHtml: bodyHtml,
            bodyText: bodyText,
            occurredAt: DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Crée une notification de device hors ligne.
    /// </summary>
    public static AlertNotification CreateOfflineDeviceNotification(string deviceName = "Test Device")
    {
        var html = $"""
            {deviceName} <b>hors ligne</b>
            
            Le capteur n'a pas communiqué depuis plus de 2 heures.
            """;

        var text = $"""
            {deviceName} hors ligne
            
            Le capteur n'a pas communiqué depuis plus de 2 heures.
            """;

        return new AlertNotification(
            title: "Capteur hors ligne",
            bodyHtml: html,
            bodyText: text,
            occurredAt: DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Crée une notification d'alarme déclenchée.
    /// </summary>
    public static AlertNotification CreateAlarmTriggeredNotification(
        string deviceName = "Test Device",
        string property = "Temperature",
        double currentValue = 35.5,
        double threshold = 30.0)
    {
        var html = $"""
            <b>ALERTE DÉCLENCHÉE</b>
            
            {deviceName}
            
            {property} est au dessus du seuil
            Valeur actuelle: {currentValue:F1}
            Seuil: {threshold:F1}
            """;

        var text = $"Alerte sur {deviceName}: {property} = {currentValue:F1} (seuil: {threshold:F1})";

        return new AlertNotification(
            title: "Alerte déclenchée",
            bodyHtml: html,
            bodyText: text,
            occurredAt: DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// Crée une notification avec des actions interactives.
    /// </summary>
    public static AlertNotification CreateNotificationWithActions()
    {
        var actions = new[]
        {
            NotificationActionRow.FromActions(
                new NotificationAction("Voir Détails", CallbackData: "device:view:123"),
                new NotificationAction("Silencier", CallbackData: "alert:mute:123:1h")),
            NotificationActionRow.FromActions(
                new NotificationAction("Voir Graphique", CallbackData: "chart:view:123"))
        };

        return new AlertNotification(
            title: "Alerte avec actions",
            bodyHtml: "<b>Alerte</b> avec actions disponibles",
            bodyText: "Alerte avec actions disponibles",
            occurredAt: DateTimeOffset.UtcNow,
            actions: actions);
    }
}
