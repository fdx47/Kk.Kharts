using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Services.Telegram;
using Kk.Kharts.Api.Utils;
using Kk.Kharts.Shared.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

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

        if (DeviceTransmissionGuard.IsDuplicateMeasurement(device.LastSendAt, measurementTimestampUtc))
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
            var jsonPayload = JsonSerializer.Serialize(new
            {
                devEUI = device.DevEui,
                timestamp = timestamp.ToString("yyyy-MM-ddTHH:mm:ss.fffffffZ"),
                lastSendAt = device.LastSendAt,
                endpoint,
                payload//,
                //duplicateContext
            }, new JsonSerializerOptions { WriteIndented = true });

            //var contextSection = string.IsNullOrWhiteSpace(duplicateContext) ? string.Empty: $"{duplicateContext}\n\n";

            //var message = $"""
            //    🔄 <b>Donnée dupliquée détectée</b>

            //    {contextSection}<b>DevEUI:</b> <code>{device.DevEui}</code>
            //    <b>Device:</b> {device.Name}
            //    <b>Description:</b> {device.Description}
            //    <b>Localisation:</b> {device.InstallationLocation}
            //    <b>Entreprise:</b> {device.CompanyName ?? "N/A"}

            //    <b>Timestamp reçu:</b> {timestamp:dd/MM/yyyy HH:mm:ss} UTC
            //    <b>LastSendAt:</b> {device.LastSendAt}

            //    <b>Endpoint:</b> <code>{endpoint}</code>

            //    <i>{duplicateMessage}</i>

            //    <pre>{jsonPayload}</pre>
            //    """;


            var message = $"""
                🔄 <b>Donnée dupliquée détectée</b>

                <b>DevEUI:</b> <code>{device.DevEui}</code>
                <b>Device:</b> {device.Name}
                <b>Description:</b> {device.Description}
                <b>Localisation:</b> {device.InstallationLocation}
                <b>Entreprise:</b> {device.CompanyName ?? "N/A"}

                <b>Timestamp reçu:</b> {timestamp:dd/MM/yyyy HH:mm:ss} UTC
                <b>LastSendAt:</b> {device.LastSendAt}

                <b>Endpoint:</b> <code>{endpoint}</code>

                <i>{duplicateMessage}</i>

                <pre>{jsonPayload}</pre>
                """;

            await _telegram.SendToDoublonsTopicAsync(message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Échec de l'envoi de la notification de doublon pour {DevEui}", device.DevEui);
        }
    }
}
