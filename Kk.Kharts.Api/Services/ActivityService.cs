using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Kk.Kharts.Api.Services;

public class ActivityService : IActivityService
{
    private readonly AppDbContext _context;

    public ActivityService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<RecentActivityDto>> GetRecentActivitiesAsync(int count = 10)
    {
        var activities = new List<RecentActivityDto>();

        // 1. Perfis VPN recentes
        var recentVpnProfiles = await _context.VpnProfiles
            .OrderByDescending(v => v.CreatedAt)
            .Take(count / 2)
            .Select(v => new RecentActivityDto
            {
                Id = v.Id,
                Type = "vpn_profile",
                Description = "Nouveau profil VPN créé",
                EntityName = v.CommonName,
                CreatedAt = v.CreatedAt,
                UserName = "System",
                Details = v.Type.ToUpper() + " - " + v.VpnIp
            })
            .ToListAsync();

        activities.AddRange(recentVpnProfiles);

        // 2. Dispositivos recentes (baseado em LastSendAt)
        var recentDevices = await _context.Devices
            .Include(d => d.Company)
            .Where(d => d.LastSendAt != "??")
            .OrderByDescending(d => d.LastSendAt)
            .Take(count / 2)
            .Select(d => new RecentActivityDto
            {
                Id = d.Id,
                Type = "device",
                Description = "Dispositif mis à jour",
                EntityName = d.Name,
                CreatedAt = DateTime.Parse(d.LastSendAt),
                UserName = "System",
                Details = d.DevEui + (d.Company != null ? " - " + d.Company.Name : string.Empty)
            })
            .ToListAsync();

        activities.AddRange(recentDevices);

        // 3. Usuários recentes
        var recentUsers = await _context.Users
            .OrderByDescending(u => u.SignupDate)
            .Take(3)
            .Select(u => new RecentActivityDto
            {
                Id = u.Id,
                Type = "user",
                Description = "Nouvel utilisateur enregistré",
                EntityName = u.Nom,
                CreatedAt = u.SignupDate,
                UserName = "System",
                Details = u.Role + " - " + u.Email
            })
            .ToListAsync();

        activities.AddRange(recentUsers);

        // Ordenar por data e limitar ao count
        return activities
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToList();
    }
}
