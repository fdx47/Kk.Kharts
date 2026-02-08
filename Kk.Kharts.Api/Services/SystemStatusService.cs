using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.DTOs;
using Microsoft.EntityFrameworkCore;

namespace Kk.Kharts.Api.Services;

public class SystemStatusService : ISystemStatusService
{
    private readonly AppDbContext _context;
    private readonly ILogger<SystemStatusService> _logger;

    public SystemStatusService(AppDbContext context, ILogger<SystemStatusService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SystemStatusDto> GetSystemStatusAsync()
    {
        var status = new SystemStatusDto
        {
            LastCheck = DateTime.UtcNow,
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            ApiVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "Unknown"
        };

        // Verificar API (se estamos rodando, está online)
        try
        {
            // Verificar se há conexões ativas ao banco de dados
            var connectionTest = await _context.Database.CanConnectAsync();
            status.DatabaseConnected = connectionTest;
            status.ApiOnline = true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Erro ao verificar status do banco de dados: {0}", ex.Message);
            status.DatabaseConnected = false;
            status.ApiOnline = false;
        }

        // Verificar serviços (contar entidades principais)
        try
        {
            status.ServicesActive = true;
            
            // Contar dispositivos
            status.TotalDevices = await _context.Devices.CountAsync();
            status.OnlineDevices = await _context.Devices
                .Where(d => d.ActiveInKropKontrol == true && d.LastSendAt != "??")
                .CountAsync();
            
            // Contar usuários
            status.TotalUsers = await _context.Users.CountAsync();
            status.ActiveUsers = await _context.Users
                .Where(u => u.Role == "Root" || u.Role == "Admin")
                .CountAsync();
            
            // Contar perfis VPN
            status.TotalVpnProfiles = await _context.VpnProfiles.CountAsync();
            status.AssignedVpnProfiles = await _context.VpnProfiles
                .Where(v => v.AssignedCompanyId != null || v.AssignedUserId != null)
                .CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError("Erro ao contar entidades: {0}", ex.Message);
            status.ServicesActive = false;
        }

        // Simular verificação de espaço em disco (pode ser implementado com System.IO)
        try
        {
            var driveInfo = new System.IO.DriveInfo("C:\\");
            var totalSpace = driveInfo.TotalSize;
            var freeSpace = driveInfo.AvailableFreeSpace;
            status.DiskUsagePercentage = Math.Round(((double)(totalSpace - freeSpace) / totalSpace) * 100);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Não foi possível verificar espaço em disco: {0}", ex.Message);
            status.DiskUsagePercentage = 0;
        }

        return status;
    }
}
