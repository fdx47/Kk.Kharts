using Kk.Kharts.Api.DTOs.MiniApp;
using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Api.Services.IService;

/// <summary>
/// Service pour la Telegram Mini App — encapsule tous les accès DB.
/// </summary>
public interface IMiniAppService
{
    Task<User?> GetUserByIdWithCompanyAsync(int userId, CancellationToken ct);
    Task<List<MiniAppDeviceDto>> GetDevicesWithReadingsAsync(User user, CancellationToken ct);
    Task<MiniAppDashboardStateDto> GetDashboardStateAsync(string devEui, CancellationToken ct);
    Task<List<MiniAppAlertDto>> GetActiveAlertsAsync(List<string> devEuis, CancellationToken ct);
    Task<MiniAppStatsDto> GetStatsAsync(List<string> devEuis, int deviceCount, CancellationToken ct);
    Task<object> GetDeviceDataAsync(string devEui, string period, CancellationToken ct);
    Task<List<MiniAppThresholdResponse>> GetDeviceThresholdsAsync(string devEui, CancellationToken ct);
    Task<bool> UpdateDeviceThresholdsAsync(string devEui, List<ThresholdUpdateItem> thresholds, CancellationToken ct);
    Task<List<Device>> GetDevicesForUserAsync(User user, CancellationToken ct);
    Task<bool> UserHasAccessToDeviceAsync(User user, string devEui, CancellationToken ct);
}
