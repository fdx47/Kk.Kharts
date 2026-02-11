using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Services.Telegram;
using Kk.Kharts.Api.Utility.Wet150;
using Kk.Kharts.Shared.DTOs.UC502.Wet150;
using Microsoft.Extensions.Logging;

namespace Kk.Kharts.Api.Utils.Wet150;

/// <summary>
/// Orquestrador para processamento de dados SDI-12 do sensor WET150.
/// </summary>
public static class SdiToVwcEc
{
    /// <summary>
    /// Processa dados SDI-12 e calcula VWC/EC para diferentes tipos de solo.
    /// </summary>
    public static async Task<List<CalculationResult>> CalcValueSdiToVwcEcAsync(
        PayloadWet150FromUg65WithApiKeyDTO payloadJson,
        DateTime timestamp,
        string devEui,
        IDeviceService deviceService,
        ITelegramService telegram,
        IKkTimeZoneService timeZoneService,
        ILoggerFactory loggerFactory)
    {
        var parseResult = Sdi12Parser.Parse(payloadJson.SDI12_1);

        var notificationService = new Sdi12NotificationService(deviceService, telegram, timeZoneService, loggerFactory.CreateLogger<Sdi12NotificationService>());
        await notificationService.NotifyIfNeededAsync(parseResult, payloadJson, timestamp, devEui);

        if (!parseResult.ShouldSaveToDatabase)
            return [];

        return SoilWaterContentCalculator.CalcularECpVWC(
            parseResult.Permittivity,
            parseResult.BulkEC,
            parseResult.Temperature);
    }
}
