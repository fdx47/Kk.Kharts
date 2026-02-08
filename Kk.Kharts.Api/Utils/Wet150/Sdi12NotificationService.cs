using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Services.Telegram;
using Kk.Kharts.Shared.DTOs.UC502.Wet150;
using System.Text.Json;

namespace Kk.Kharts.Api.Utils.Wet150;

/// <summary>
/// Serviço de notificações Telegram para erros SDI-12.
/// </summary>
public sealed class Sdi12NotificationService(
    IDeviceService deviceService,
    ITelegramService telegram,
    IKkTimeZoneService timeZoneService)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };


    public async Task NotifyIfNeededAsync(Sdi12ParseResult parseResult, PayloadWet150FromUg65WithApiKeyDTO payload, DateTime timestamp, string devEui)
    {
        var notificationType = DetermineNotificationType(parseResult);

        if (notificationType == NotificationType.None) return;

        var deviceInfo = await GetDeviceInfoAsync(devEui);
        var formattedPayload = JsonSerializer.Serialize(payload, JsonOptions);
        var message = BuildMessage(notificationType, parseResult, timestamp, devEui, deviceInfo, formattedPayload);

        await SendNotificationAsync(notificationType, message);
    }


    private static NotificationType DetermineNotificationType(Sdi12ParseResult result) => result.Status switch
    {
        Sdi12ValidationStatus.Valid => NotificationType.None,
        Sdi12ValidationStatus.NullInput or
        Sdi12ValidationStatus.CorruptedData or
        Sdi12ValidationStatus.InvalidControlCharacters or
        Sdi12ValidationStatus.InvalidFormat => NotificationType.CorruptedData,
        Sdi12ValidationStatus.AllValuesAre69 or
        Sdi12ValidationStatus.AllValuesAreZero or
        Sdi12ValidationStatus.LowHumidity8020 => NotificationType.InvalidSensorValues,
        _ => NotificationType.None
    };


    private async Task<string> GetDeviceInfoAsync(string devEui)
    {
        try
        {
            var device = await deviceService.GetDeviceByDevEuiApiKeyInternalAsync(devEui);
            if (device is null) return string.Empty;

            return $"""

                📱 Device Information:
                • Name: {device.Name}
                • Description: {device.Description}
                • Location: {device.InstallationLocation}
                • Company: {device.Company?.Name ?? "Unknown"}
                """;
        }
        catch (Exception ex)
        {
            return $"\n\n⚠️ Impossible d'obtenir les informations du dispositif: {ex.Message}";
        }
    }

    private string BuildMessage(NotificationType type, Sdi12ParseResult result, DateTime timestamp, string devEui, string deviceInfo, string formattedPayload)
    {
        var parisTime = timeZoneService.ConvertToParisTime(timestamp);

        var (emoji, title) = type switch
        {
            NotificationType.CorruptedData => ("🔴", "Données SDI12 Corrompues"),
            NotificationType.InvalidSensorValues => ("_⚠️_", "Valeurs de Capteur Invalides"),
            NotificationType.Debug => ("🐞", "Debug -<SdiToVwcEc - CalcValueSdiToVwcEcAsync>-"),
            _ => ("ℹ️", "Information")
        };

        var logsText = string.Join("\n", result.Logs);

        return $"""
            {emoji} {title}

            Détectées le: {timestamp:dd MMM yyyy à HH:mm:ss} UTC || {parisTime:dd MMM yyyy à HH:mm:ss} Paris)

            DevEui: {devEui}{deviceInfo}

            Payload:            
            {formattedPayload}            

            {logsText}
            """;
    }


    private async Task SendNotificationAsync(NotificationType type, string message)
    {
        if (type == NotificationType.CorruptedData || type == NotificationType.InvalidSensorValues)
        {
            Console.WriteLine($"[WARN] SDI12 notification: {type}");
            await telegram.SendToErrorsTopicAsync(message);
        }
        else if (type == NotificationType.Debug)
        {
            await telegram.SendToDebugTopicAsync(message);
        }
    }


    private enum NotificationType
    {
        None,
        CorruptedData,
        InvalidSensorValues,
        Debug
    }
}
