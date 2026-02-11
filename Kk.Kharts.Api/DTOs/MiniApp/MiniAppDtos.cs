namespace Kk.Kharts.Api.DTOs.MiniApp;

// ── Request DTOs ──────────────────────────────────────────────

public record MiniAppAuthRequest(long TelegramId, string? Username, string? FirstName, string? LastName, string? InitData);
public record SupportMessageRequest(string Message, long? TelegramId, string? UserName);
public record UpdateThresholdsRequest(List<ThresholdUpdateItem> Thresholds);

public record ThresholdUpdateItem(
    string SensorType,
    double? MinValue,
    double? MaxValue,
    bool IsEnabled,
    bool UseTimePeriods,
    List<TimePeriodUpdateItem>? Periods);

public record TimePeriodUpdateItem(
    string Name,
    string StartTime,
    string EndTime,
    double? MinValue,
    double? MaxValue,
    bool IsEnabled);

// ── Response DTOs ─────────────────────────────────────────────

public record MiniAppDeviceDto(
    string DevEui,
    string Name,
    string? Description,
    string? InstallationLocation,
    string Model,
    bool IsOnline,
    DateTime LastSeenAt,
    float Battery,
    string? Company,
    List<string> Variables,
    float? LastTemperature,
    float? LastHumidity,
    float? LastVwc,
    float? LastEc);

public record MiniAppDashboardStateDto(
    float? Temperature,
    float? Humidity,
    float? Vwc,
    float? Ec,
    DateTime Timestamp);

public record MiniAppAlertDto(
    int Id,
    string DevEui,
    string DeviceName,
    string Type,
    string Message,
    bool IsActive,
    DateTime TriggeredAt);

public record MiniAppStatsDto(
    int TotalDevices,
    int OnlineDevices,
    int OfflineDevices,
    int ActiveAlerts);

public record MiniAppThresholdResponse(
    string SensorType,
    double? MinValue,
    double? MaxValue,
    bool IsEnabled,
    bool UseTimePeriods,
    List<MiniAppTimePeriodResponse> Periods);

public record MiniAppTimePeriodResponse(
    string Name,
    string StartTime,
    string EndTime,
    double? MinValue,
    double? MaxValue,
    bool IsEnabled);

public record MiniAppProfileDto(
    int Id,
    string Email,
    string Name,
    string? Company,
    bool IsTelegram);
