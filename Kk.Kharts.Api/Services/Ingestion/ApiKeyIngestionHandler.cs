using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Services.Telegram;
using Kk.Kharts.Api.Utils;
using Kk.Kharts.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Globalization;

namespace Kk.Kharts.Api.Services.Ingestion;

public class ApiKeyIngestionHandler : IApiKeyIngestionHandler
{
    private readonly IApiKeyCompanyAccessor _companyAccessor;
    private readonly IDeviceService _deviceService;
    private readonly ITelegramService _telegram;
    private readonly IDuplicateMetricsService _duplicateMetrics;
    private readonly ILogger<ApiKeyIngestionHandler> _logger;

    public ApiKeyIngestionHandler(
        IApiKeyCompanyAccessor companyAccessor,
        IDeviceService deviceService,
        ITelegramService telegram,
        IDuplicateMetricsService duplicateMetrics,
        ILogger<ApiKeyIngestionHandler> logger)
    {
        _companyAccessor = companyAccessor;
        _deviceService = deviceService;
        _telegram = telegram;
        _duplicateMetrics = duplicateMetrics;
        _logger = logger;
    }

    public async Task<ApiKeyIngestionResult> PrepareAsync(
        ControllerBase controller,
        string? rawDevEui,
        string duplicateMessage,
        DateTime? measurementTimestampUtc = null,
        object? payload = null//,
      /*  string? duplicateContext = null*/)
    {
        if (!controller.ModelState.IsValid)
        {
            return ApiKeyIngestionResult.FromShortCircuit(controller.BadRequest(controller.ModelState));
        }

        var normalizedDevEui = DevEuiNormalizer.Normalize(rawDevEui);

        var company = _companyAccessor.GetCompany(controller.HttpContext);
        if (company == null)
        {
            return ApiKeyIngestionResult.FromShortCircuit(controller.Unauthorized(new
            {
                message = "Entreprise non trouvée dans le contexte."
            }));
        }

        var device = await _deviceService.GetDeviceByDevEuiAsync<DeviceDto>(normalizedDevEui, company);

        if (device == null)
        {
            return ApiKeyIngestionResult.FromShortCircuit(controller.NotFound(new
            {
                message = "Device non trouvé."
            }));
        }

        var attendu = ObtenirNombreChampsSdi12Attendus(device.Model);
        var champsReçus = CompterChampsSdi12(payload);
        var estPartiel = champsReçus > 0 && champsReçus < attendu; // payload partiel : on ne bloque pas l'agrégation

        if (!estPartiel && DeviceTransmissionGuard.IsDuplicateMeasurement(device.LastSendAt, measurementTimestampUtc))
        {
            // Send notification to Doublons topic + record metrics
            await SendDuplicateNotificationAsync(
                device,
                measurementTimestampUtc,
                controller.HttpContext.Request.Path,
                duplicateMessage,
                payload//,
                //duplicateContext
                );

            await _duplicateMetrics.RecordDuplicateAsync(
                normalizedDevEui, device.Name, device.CompanyName, controller.HttpContext.Request.Path);

            return ApiKeyIngestionResult.FromShortCircuit(controller.Ok(new
            {
                message = duplicateMessage
            }));
        }

        return ApiKeyIngestionResult.Success(company, device, normalizedDevEui, measurementTimestampUtc);
    }

    private async Task SendDuplicateNotificationAsync(
        DeviceDto device,
        DateTime? measurementTimestampUtc,
        string endpoint,
        string duplicateMessage,
        object? payload//,
        //string? duplicateContext
        )
    {
        try
        {
            var timestamp = measurementTimestampUtc ?? DateTime.UtcNow;
            var (lastSendAtUtc, delta, tolerance) = ExtraireDelta(device.LastSendAt, timestamp);
            var (permittivite, ecBulk, soilTemp) = ExtraireSdi12(payload);
            var (sdi12Raw, batteryStr) = ExtraireSdi12EtBatterie(payload);
            var payloadNettoye = ConstruirePayloadSansKkTimestamp(payload);

            var titre = string.IsNullOrWhiteSpace(duplicateMessage)
                ? "Donnée dupliquée détectée"
                : $"Donnée dupliquée détectée ({duplicateMessage})";

            var message = $"""
                🔄 <b>{titre}</b>

                <b>DevEUI:</b> <code>{device.DevEui}</code>
                <b>Id device:</b> {device.Id}
                <b>Device:</b> {device.Name}
                <b>Description:</b> {device.Description}
                <b>Localisation:</b> {device.InstallationLocation}
                <b>Entreprise:</b> {device.CompanyName ?? "N/A"}
                <b>Endpoint traité:</b> <code>{endpoint}</code>

                <b>Horodatage reçu:</b> {timestamp:dd/MM/yyyy HH:mm:ss} UTC
                <b>LastSendAt (db):</b> {device.LastSendAt}
                <b>LastSendAt (UTC parsé):</b> {(lastSendAtUtc.HasValue ? lastSendAtUtc.Value.ToString("dd/MM/yyyy HH:mm:ss") + " UTC" : "n/a")}
                <b>Delta calculé (ms):</b> {(delta?.TotalMilliseconds.ToString("F0", CultureInfo.InvariantCulture) ?? "n/a")}
                <b>Tolérance utilisée (ms):</b> {tolerance.TotalMilliseconds:F0}
                <b>Message:</b> {duplicateMessage}

                <b>Valeurs stockées (base) utilisées pour la comparaison</b>
                - Permittivité (ε)     : {permittivite}
                - EC_bulk (dS/m)       : {ecBulk}
                - Soil temperature (°C): {soilTemp}

                <b>Mesures reçues (payload)</b>
                - SDI-12 brut  : {sdi12Raw}
                - Batterie     : {batteryStr}

                <b>Payload reçu (brut)</b>
                <pre>{payloadNettoye}</pre>
                """;

            await _telegram.SendToDoublonsTopicAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Échec de l'envoi de la notification de doublon pour {DevEui}", device.DevEui);
        }
    }

    private static (string sdi12Raw, string batteryStr) ExtraireSdi12EtBatterie(object? payload)
    {
        const string na = "n/a";
        if (payload is null) return (na, na);

        var type = payload.GetType();
        string sdi12Raw = na;
        string battery = na;

        var sdiProp = type.GetProperty("sdi12_1") ?? type.GetProperty("Sdi12_1") ?? type.GetProperty("SDI12_1");
        if (sdiProp?.GetValue(payload) is string sdiVal && !string.IsNullOrWhiteSpace(sdiVal))
        {
            sdi12Raw = sdiVal.Replace("\r", string.Empty).Replace("\n", string.Empty);
        }

        var batteryProp = type.GetProperty("Battery") ?? type.GetProperty("battery") ?? type.GetProperty("batteryLevel");
        if (batteryProp?.GetValue(payload) is not null)
        {
            battery = batteryProp.GetValue(payload)?.ToString() ?? na;
        }

        return (sdi12Raw, battery);
    }

    private static (DateTime? lastSendAtUtc, TimeSpan? delta, TimeSpan tolerance) ExtraireDelta(string? lastSendAt, DateTime timestamp)
    {
        var tolerance = TimeSpan.FromMinutes(1);

        if (string.IsNullOrWhiteSpace(lastSendAt))
            return (null, null, tolerance);

        if (!DateTime.TryParse(
                lastSendAt,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsedUtc))
        {
            return (null, null, tolerance);
        }

        var delta = (timestamp - parsedUtc).Duration();
        return (parsedUtc, delta, tolerance);
    }

    private static (string permittivite, string ecBulk, string soilTemp) ExtraireSdi12(object? payload)
    {
        const string na = "n/a";

        if (payload is null)
            return (na, na, na);

        var sdi12 = EssayerLireSdi12(payload);
        if (string.IsNullOrWhiteSpace(sdi12))
            return (na, na, na);

        var parts = sdi12.Replace("\r", string.Empty).Replace("\n", string.Empty).Split('+', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 4)
            return (na, na, na);

        return (parts[1], parts[2], parts[3]);
    }

    private static int CompterChampsSdi12(object? payload)
    {
        if (payload is null)
            return 0;

        var type = payload.GetType();

        // 1) ExtraFieldsSdi12 (JsonExtensionData)
        var extraFields = type.GetProperty("ExtraFieldsSdi12")?.GetValue(payload) as IDictionary<string, JsonElement>;

        var count = 0;

        if (extraFields is not null)
        {
            foreach (var kvp in extraFields)
            {
                if (!kvp.Key.StartsWith("sdi12_", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (kvp.Value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(kvp.Value.GetString()))
                    count++;
            }
        }

        // 2) Fallback sur les propriétés directes
        var names = new[] { "sdi12_1", "sdi12_2", "sdi12_3", "sdi12_4", "Sdi12_1", "Sdi12_2", "Sdi12_3", "Sdi12_4", "SDI12_1", "SDI12_2", "SDI12_3", "SDI12_4" };

        foreach (var name in names)
        {
            var prop = type.GetProperty(name);
            if (prop is null)
                continue;

            var value = prop.GetValue(payload) as string;
            if (!string.IsNullOrWhiteSpace(value))
                count++;
        }

        return count;
    }

    private static int ObtenirNombreChampsSdi12Attendus(int? model)
    {
        return model switch
        {
            64 => 4,
            63 => 3,
            _ => 2
        };
    }

    private static string? EssayerLireSdi12(object payload)
    {
        var type = payload.GetType();

        // 1) Cherche dans ExtraFieldsSdi12 (JsonExtensionData)
        var extraFields = type.GetProperty("ExtraFieldsSdi12")?.GetValue(payload) as IDictionary<string, JsonElement>;
        if (extraFields is not null)
        {
            foreach (var kvp in extraFields)
            {
                if (!kvp.Key.StartsWith("sdi12_", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (kvp.Value.ValueKind == JsonValueKind.String)
                {
                    var value = kvp.Value.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                        return value;
                }
            }
        }

        // 2) Fallback sur les propriétés directes
        var candidates = new[] { "sdi12_1", "Sdi12_1", "SDI12_1" };

        foreach (var name in candidates)
        {
            var prop = type.GetProperty(name);
            if (prop is null)
                continue;

            var value = prop.GetValue(payload) as string;
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }

        return null;
    }

    private static string ConstruirePayloadSansKkTimestamp(object? payload)
    {
        if (payload is null)
            return "{}";

        var options = new JsonSerializerOptions { WriteIndented = true };
        using var document = JsonSerializer.SerializeToDocument(payload, options);

        if (document.RootElement.ValueKind != JsonValueKind.Object)
            return JsonSerializer.Serialize(payload, options);

        var filtered = new Dictionary<string, object?>();
        foreach (var property in document.RootElement.EnumerateObject())
        {
            if (property.Name.Equals("kktimestamp", StringComparison.OrdinalIgnoreCase))
                continue;

            filtered[property.Name] = property.Value.ValueKind switch
            {
                JsonValueKind.String => property.Value.GetString(),
                JsonValueKind.Number => property.Value.GetDouble(),
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Null => null,
                _ => property.Value.ToString()
            };
        }

        return JsonSerializer.Serialize(filtered, options);
    }
}
