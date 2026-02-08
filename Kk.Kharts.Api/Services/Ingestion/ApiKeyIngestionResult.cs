using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Kk.Kharts.Api.Services.Ingestion;

public sealed class ApiKeyIngestionResult
{
    private ApiKeyIngestionResult(
        IActionResult? shortCircuitResult,
        Company? company,
        DeviceDto? device,
        string? normalizedDevEui,
        DateTime? measurementTimestampUtc)
    {
        ShortCircuitResult = shortCircuitResult;
        Company = company;
        Device = device;
        NormalizedDevEui = normalizedDevEui;
        MeasurementTimestampUtc = measurementTimestampUtc;
    }

    public IActionResult? ShortCircuitResult { get; }
    public bool ShouldShortCircuit => ShortCircuitResult != null;
    public Company? Company { get; }
    public DeviceDto? Device { get; }
    public string? NormalizedDevEui { get; }
    public DateTime? MeasurementTimestampUtc { get; }

    public static ApiKeyIngestionResult FromShortCircuit(IActionResult result) => new(result, null, null, null, null);

    public static ApiKeyIngestionResult Success(Company company, DeviceDto device, string normalizedDevEui, DateTime? measurementTimestampUtc) =>
        new(null, company, device, normalizedDevEui, measurementTimestampUtc);
}
