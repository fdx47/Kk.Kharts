using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services.IService;

/// <summary>
/// Service d'accès aux données pour le bot de debug.
/// Remplace l'usage direct d'AppDbContext + IServiceScopeFactory dans DebugBotController.
/// </summary>
public interface IDebugBotDataService
{
    Task<List<Device>> GetActiveDevicesWithCompanyAsync(CancellationToken ct);
    Task<List<Device>> GetInactiveDevicesWithCompanyAsync(CancellationToken ct);
    Task<DebugBotSystemStats> GetSystemStatsAsync(CancellationToken ct);
}

public record DebugBotSystemStats(
    int TotalActiveDevices,
    int OnlineDevices,
    int OfflineDevices,
    int InactiveDevices,
    int LowBatteryDevices,
    int ActiveAlerts,
    int TotalAlarmRules,
    int TotalUsers,
    int LinkedTelegramUsers,
    int Dashboards,
    List<CompanyDeviceCount> TopCompanies);

public record CompanyDeviceCount(string Company, int Count);
