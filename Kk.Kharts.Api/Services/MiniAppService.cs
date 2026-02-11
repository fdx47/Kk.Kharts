using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.DTOs.MiniApp;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Services.Telegram;
using Kk.Kharts.Shared.Entities;
using Kk.Kharts.Shared.Enums;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text.Json;

namespace Kk.Kharts.Api.Services;

public class MiniAppService(
    AppDbContext db,
    IDashboardConfigService dashboardConfigService,
    ILogger<MiniAppService> logger) : IMiniAppService
{
    public async Task<User?> GetUserByIdWithCompanyAsync(int userId, CancellationToken ct)
    {
        return await db.Users
            .Include(u => u.Company)
            .FirstOrDefaultAsync(u => u.Id == userId, ct);
    }

    public async Task<List<MiniAppDeviceDto>> GetDevicesWithReadingsAsync(User user, CancellationToken ct)
    {
        var devices = await GetDevicesForUserAsync(user, ct);
        var variablesByDevice = await GetVariablesByDeviceAsync(user.Id, ct);

        var devEuis = devices.Select(d => d.DevEui).ToList();

        var lastEm300Readings = await db.Em300ths
            .Where(x => devEuis.Contains(x.DevEui))
            .GroupBy(x => x.DevEui)
            .Select(g => g.OrderByDescending(x => x.Timestamp).First())
            .ToDictionaryAsync(x => x.DevEui, StringComparer.OrdinalIgnoreCase, ct);

        var lastUc502Readings = await db.Uc502Wet150s
            .Where(x => devEuis.Contains(x.DevEui))
            .GroupBy(x => x.DevEui)
            .Select(g => g.OrderByDescending(x => x.Timestamp).First())
            .ToDictionaryAsync(x => x.DevEui, StringComparer.OrdinalIgnoreCase, ct);

        var twoHoursAgo = DateTime.UtcNow.AddHours(-2);

        return devices.Select(d =>
        {
            var lastSeen = NormalizeToUtc(d.LastSeenAt);
            if (TryParseLastSendAt(d.LastSendAt, out var parsedSendAt))
                lastSeen = parsedSendAt;

            lastEm300Readings.TryGetValue(d.DevEui, out var em300);
            lastUc502Readings.TryGetValue(d.DevEui, out var uc502);

            return new MiniAppDeviceDto(
                DevEui: d.DevEui,
                Name: d.Name,
                Description: d.Description,
                InstallationLocation: d.InstallationLocation,
                Model: d.ModeloNavegacao?.Model ?? "Unknown",
                IsOnline: lastSeen > twoHoursAgo,
                LastSeenAt: lastSeen,
                Battery: d.Battery,
                Company: d.Company?.Name,
                Variables: variablesByDevice.TryGetValue(d.DevEui, out var vars) ? vars : new List<string>(),
                LastTemperature: em300?.Temperature ?? uc502?.SoilTemperature,
                LastHumidity: em300?.Humidity ?? (uc502 != null ? (float?)null : null),
                LastVwc: uc502?.MineralVWC,
                LastEc: uc502?.MineralECp);
        }).ToList();
    }

    public async Task<MiniAppDashboardStateDto> GetDashboardStateAsync(string devEui, CancellationToken ct)
    {
        var em300 = await db.Em300ths
            .Where(x => x.DevEui == devEui)
            .OrderByDescending(x => x.Timestamp)
            .FirstOrDefaultAsync(ct);

        var uc502 = await db.Uc502Wet150s
            .Where(x => x.DevEui == devEui)
            .OrderByDescending(x => x.Timestamp)
            .FirstOrDefaultAsync(ct);

        return new MiniAppDashboardStateDto(
            Temperature: em300?.Temperature ?? uc502?.SoilTemperature,
            Humidity: em300?.Humidity ?? (uc502 != null ? (float?)null : null),
            Vwc: uc502?.MineralVWC,
            Ec: uc502?.MineralECp,
            Timestamp: em300?.Timestamp ?? uc502?.Timestamp ?? DateTime.UtcNow);
    }

    public async Task<List<MiniAppAlertDto>> GetActiveAlertsAsync(List<string> devEuis, CancellationToken ct)
    {
        return await db.AlarmRules
            .Include(a => a.Device)
            .Where(a => devEuis.Contains(a.Device.DevEui) && a.Enabled && a.IsAlarmActive)
            .OrderByDescending(a => a.Id)
            .Take(50)
            .Select(a => new MiniAppAlertDto(
                a.Id,
                a.Device.DevEui,
                a.Device.Description ?? a.Device.Name,
                a.ActiveThresholdType ?? "threshold",
                $"{a.PropertyName} {(a.ActiveThresholdType == "Low" ? "en dessous de " + a.LowValue : "au dessus de " + a.HighValue)}",
                a.IsAlarmActive,
                DateTime.UtcNow))
            .ToListAsync(ct);
    }

    public async Task<MiniAppStatsDto> GetStatsAsync(List<string> devEuis, int deviceCount, CancellationToken ct)
    {
        var activeAlerts = await db.AlarmRules
            .Include(a => a.Device)
            .CountAsync(a => devEuis.Contains(a.Device.DevEui) && a.Enabled && a.IsAlarmActive, ct);

        return new MiniAppStatsDto(
            TotalDevices: deviceCount,
            OnlineDevices: 0,
            OfflineDevices: 0,
            ActiveAlerts: activeAlerts);
    }

    public async Task<object> GetDeviceDataAsync(string devEui, string period, CancellationToken ct)
    {
        var (startDate, endDate) = GetDateRange(period);

        var wet150Data = await db.Uc502Wet150s
            .Where(d => d.DevEui == devEui && d.Timestamp >= startDate && d.Timestamp <= endDate)
            .OrderBy(d => d.Timestamp)
            .Select(d => new
            {
                time = d.Timestamp,
                temperature = d.SoilTemperature,
                vwc = d.MineralVWC,
                ec = d.MineralECp,
                battery = d.Battery
            })
            .ToListAsync(ct);

        if (wet150Data.Count > 0)
            return wet150Data;

        var em300Data = await db.Em300ths
            .Where(d => d.DevEui == devEui && d.Timestamp >= startDate && d.Timestamp <= endDate)
            .OrderBy(d => d.Timestamp)
            .Select(d => new
            {
                time = d.Timestamp,
                temperature = d.Temperature,
                humidity = d.Humidity,
                battery = d.Battery
            })
            .ToListAsync(ct);

        return em300Data;
    }

    public async Task<List<MiniAppThresholdResponse>> GetDeviceThresholdsAsync(string devEui, CancellationToken ct)
    {
        return await db.AlarmRules
            .Include(a => a.Device)
            .Include(a => a.TimePeriods)
            .Where(a => a.Device.DevEui == devEui)
            .Select(a => new MiniAppThresholdResponse(
                a.PropertyName,
                (double?)a.LowValue,
                (double?)a.HighValue,
                a.Enabled,
                a.UseTimePeriods,
                a.TimePeriods.OrderBy(p => p.DisplayOrder).Select(p => new MiniAppTimePeriodResponse(
                    p.Name,
                    p.StartTime.ToString(@"hh\:mm"),
                    p.EndTime.ToString(@"hh\:mm"),
                    (double?)p.LowValue,
                    (double?)p.HighValue,
                    p.IsEnabled
                )).ToList()
            ))
            .ToListAsync(ct);
    }

    public async Task<bool> UpdateDeviceThresholdsAsync(string devEui, List<ThresholdUpdateItem> thresholds, CancellationToken ct)
    {
        var device = await db.Devices.FirstOrDefaultAsync(d => d.DevEui == devEui, ct);
        if (device == null)
            return false;

        foreach (var threshold in thresholds)
        {
            var existing = await db.AlarmRules
                .Include(a => a.TimePeriods)
                .FirstOrDefaultAsync(a => a.DeviceId == device.Id && a.PropertyName == threshold.SensorType, ct);

            if (existing != null)
            {
                existing.LowValue = (float?)threshold.MinValue;
                existing.HighValue = (float?)threshold.MaxValue;
                existing.Enabled = threshold.IsEnabled;
                existing.UseTimePeriods = threshold.UseTimePeriods;

                db.AlarmTimePeriods.RemoveRange(existing.TimePeriods);
                if (threshold.UseTimePeriods && threshold.Periods != null)
                {
                    existing.TimePeriods = threshold.Periods.Select((p, idx) => new AlarmTimePeriod
                    {
                        Name = p.Name,
                        StartTime = TimeSpan.Parse(p.StartTime),
                        EndTime = TimeSpan.Parse(p.EndTime),
                        LowValue = (float?)p.MinValue,
                        HighValue = (float?)p.MaxValue,
                        IsEnabled = p.IsEnabled,
                        DisplayOrder = idx + 1
                    }).ToList();
                }
            }
            else
            {
                var newRule = new AlarmRule
                {
                    DeviceId = device.Id,
                    PropertyName = threshold.SensorType,
                    LowValue = (float?)threshold.MinValue,
                    HighValue = (float?)threshold.MaxValue,
                    Enabled = threshold.IsEnabled,
                    UseTimePeriods = threshold.UseTimePeriods,
                    IsAlarmActive = false,
                    ActiveThresholdType = null
                };

                if (threshold.UseTimePeriods && threshold.Periods != null)
                {
                    newRule.TimePeriods = threshold.Periods.Select((p, idx) => new AlarmTimePeriod
                    {
                        Name = p.Name,
                        StartTime = TimeSpan.Parse(p.StartTime),
                        EndTime = TimeSpan.Parse(p.EndTime),
                        LowValue = (float?)p.MinValue,
                        HighValue = (float?)p.MaxValue,
                        IsEnabled = p.IsEnabled,
                        DisplayOrder = idx + 1
                    }).ToList();
                }
                db.AlarmRules.Add(newRule);
            }
        }

        await db.SaveChangesAsync(ct);
        return true;
    }

    public async Task<List<Device>> GetDevicesForUserAsync(User user, CancellationToken ct)
    {
        if (user.Role.Equals("root", StringComparison.OrdinalIgnoreCase))
        {
            return await db.Devices
                .Include(d => d.Company)
                .Include(d => d.ModeloNavegacao)
                .Where(d => d.ActiveInKropKontrol)
                .OrderByDescending(d => d.LastSeenAt)
                .ToListAsync(ct);
        }

        var companyIds = await GetAccessibleCompanyIdsAsync(user, ct);

        return await db.Devices
            .Include(d => d.Company)
            .Include(d => d.ModeloNavegacao)
            .Where(d => d.ActiveInKropKontrol && companyIds.Contains(d.CompanyId))
            .OrderByDescending(d => d.LastSeenAt)
            .ToListAsync(ct);
    }

    public async Task<bool> UserHasAccessToDeviceAsync(User user, string devEui, CancellationToken ct)
    {
        var devices = await GetDevicesForUserAsync(user, ct);
        return devices.Any(d => d.DevEui == devEui);
    }

    // ── Private helpers ───────────────────────────────────────

    private async Task<List<int>> GetAccessibleCompanyIdsAsync(User user, CancellationToken ct)
    {
        var companyIds = new List<int> { user.CompanyId };

        if (user.AccessLevel == UserAccessLevel.CompanyAndSubsidiaries)
        {
            var subsidiaryIds = await db.Companies
                .Where(c => c.ParentCompanyId == user.CompanyId)
                .Select(c => c.Id)
                .ToListAsync(ct);

            companyIds.AddRange(subsidiaryIds);
        }

        return companyIds;
    }

    private async Task<Dictionary<string, List<string>>> GetVariablesByDeviceAsync(int userId, CancellationToken ct)
    {
        var variablesByDevice = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        var dashboard = await db.Dashboards
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.CreatedAt)
            .FirstOrDefaultAsync(ct);

        if (dashboard?.StateJson != null)
        {
            try
            {
                using var doc = JsonDocument.Parse(dashboard.StateJson);

                JsonElement dashboardState;
                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    var firstWrapper = doc.RootElement.EnumerateArray().FirstOrDefault();
                    if (firstWrapper.TryGetProperty("stateJson", out var sj))
                    {
                        using var innerDoc = JsonDocument.Parse(sj.GetString() ?? "{}");
                        innerDoc.RootElement.TryGetProperty("dashboardState", out dashboardState);
                    }
                    else
                    {
                        doc.RootElement.TryGetProperty("dashboardState", out dashboardState);
                    }
                }
                else
                {
                    doc.RootElement.TryGetProperty("dashboardState", out dashboardState);
                }

                if (dashboardState.ValueKind == JsonValueKind.Array)
                {
                    foreach (var deviceState in dashboardState.EnumerateArray())
                    {
                        if (deviceState.TryGetProperty("devEui", out var de) &&
                            deviceState.TryGetProperty("variables", out var vars) &&
                            vars.ValueKind == JsonValueKind.Array)
                        {
                            var eui = de.GetString();
                            if (string.IsNullOrEmpty(eui)) continue;

                            var varList = vars.EnumerateArray()
                                .Select(v => v.GetString() ?? "")
                                .Where(v => !string.IsNullOrEmpty(v))
                                .ToList();

                            variablesByDevice[eui] = varList;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Erreur lors du parsing du Dashboard StateJson para o usuário {UserId}", userId);
            }
        }

        if (variablesByDevice.Count == 0)
        {
            var charts = await dashboardConfigService.GetUserChartsAsync(userId, ct);
            foreach (var chart in charts)
            {
                if (string.IsNullOrWhiteSpace(chart.DevEui)) continue;
                if (!variablesByDevice.TryGetValue(chart.DevEui, out var chartVars))
                {
                    chartVars = new List<string>();
                    variablesByDevice[chart.DevEui] = chartVars;
                }

                if (chart.Variables != null)
                {
                    foreach (var v in chart.Variables)
                    {
                        if (!string.IsNullOrWhiteSpace(v) && !chartVars.Contains(v.Trim(), StringComparer.OrdinalIgnoreCase))
                            chartVars.Add(v.Trim());
                    }
                }
            }
        }

        return variablesByDevice;
    }

    internal static DateTime NormalizeToUtc(DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
        };
    }

    internal static bool TryParseLastSendAt(string? value, out DateTime utcDateTime)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dto))
            {
                utcDateTime = dto.UtcDateTime;
                return true;
            }

            string[] formats =
            [
                "yyyy-MM-dd HH:mm:ss zzz 'GMT'",
                "yyyy-MM-dd HH:mm:ss 'GMT'zzz",
                "yyyy-MM-ddTHH:mm:sszzz",
                "yyyy-MM-ddTHH:mm:ss.fffzzz",
                "yyyy-MM-dd HH:mm:ss"
            ];

            if (DateTimeOffset.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out dto))
            {
                utcDateTime = dto.UtcDateTime;
                return true;
            }
        }

        utcDateTime = default;
        return false;
    }

    internal static (DateTime Start, DateTime End) GetDateRange(string period)
    {
        var end = DateTime.UtcNow;
        var start = period switch
        {
            "24h" => end.AddHours(-24),
            "36h" => end.AddHours(-36),
            "48h" => end.AddHours(-48),
            "7d" => end.AddDays(-7),
            "30d" => end.AddDays(-30),
            _ => end.AddHours(-36)
        };
        return (start, end);
    }
}
