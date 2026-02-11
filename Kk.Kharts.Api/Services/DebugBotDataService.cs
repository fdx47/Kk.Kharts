using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.Entities;
using Microsoft.EntityFrameworkCore;

namespace Kk.Kharts.Api.Services;

public class DebugBotDataService(AppDbContext db) : IDebugBotDataService
{
    public async Task<List<Device>> GetActiveDevicesWithCompanyAsync(CancellationToken ct)
    {
        return await db.Devices
            .Include(d => d.Company)
            .Where(d => d.ActiveInKropKontrol)
            .ToListAsync(ct);
    }

    public async Task<List<Device>> GetInactiveDevicesWithCompanyAsync(CancellationToken ct)
    {
        return await db.Devices
            .Include(d => d.Company)
            .Where(d => !d.ActiveInKropKontrol)
            .ToListAsync(ct);
    }

    public async Task<DebugBotSystemStats> GetSystemStatsAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var lastHour = now.AddHours(-1);

        var totalActiveDevices = await db.Devices.CountAsync(d => d.ActiveInKropKontrol, ct);
        var onlineDevices = await db.Devices.CountAsync(d => d.ActiveInKropKontrol && d.LastSeenAt >= lastHour, ct);
        var inactiveDevices = await db.Devices.CountAsync(d => !d.ActiveInKropKontrol, ct);
        var lowBatteryDevices = await db.Devices.CountAsync(d => d.ActiveInKropKontrol && d.Battery <= 20, ct);

        var activeAlerts = await db.AlarmRules.CountAsync(a => a.Enabled && a.IsAlarmActive, ct);
        var totalRules = await db.AlarmRules.CountAsync(ct);

        var totalUsers = await db.Users.CountAsync(ct);
        var linkedTelegramUsers = await db.Users.CountAsync(u => u.TelegramUserId != null, ct);

        var dashboards = await db.Dashboards.CountAsync(ct);

        var topCompanies = await db.Devices
            .Where(d => d.ActiveInKropKontrol)
            .GroupBy(d => d.Company!.Name ?? "N/A")
            .Select(g => new CompanyDeviceCount(g.Key, g.Count()))
            .OrderByDescending(g => g.Count)
            .Take(5)
            .ToListAsync(ct);

        return new DebugBotSystemStats(
            TotalActiveDevices: totalActiveDevices,
            OnlineDevices: onlineDevices,
            OfflineDevices: Math.Max(totalActiveDevices - onlineDevices, 0),
            InactiveDevices: inactiveDevices,
            LowBatteryDevices: lowBatteryDevices,
            ActiveAlerts: activeAlerts,
            TotalAlarmRules: totalRules,
            TotalUsers: totalUsers,
            LinkedTelegramUsers: linkedTelegramUsers,
            Dashboards: dashboards,
            TopCompanies: topCompanies);
    }
}
