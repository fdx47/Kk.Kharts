using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.Entities;
using Kk.Kharts.Shared.Extensions;
using Microsoft.EntityFrameworkCore;

namespace Kk.Kharts.Api.Services;

public class VpnProfileService : IVpnProfileService
{
    private readonly AppDbContext _context;
    private readonly ILogger<VpnProfileService> _logger;

    public VpnProfileService(AppDbContext context, ILogger<VpnProfileService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<VpnProfileDto>> GetAllAsync(CancellationToken ct = default)
    {
        var profiles = await _context.VpnProfiles
            .Include(v => v.AssignedUser)
            .Include(v => v.AssignedCompany)
            .OrderBy(v => v.CommonName)
            .ToListAsync(ct);

        return profiles.Select(p => Kk.Kharts.Shared.Extensions.VpnProfileExtensions.ToDto(p));
    }

    public async Task<VpnProfileDto?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var profile = await _context.VpnProfiles
            .Include(v => v.AssignedUser)
            .Include(v => v.AssignedCompany)
            .FirstOrDefaultAsync(v => v.Id == id, ct);

        return profile != null ? Kk.Kharts.Shared.Extensions.VpnProfileExtensions.ToDto(profile) : null;
    }

    public async Task<VpnProfileDto> CreateAsync(VpnProfileCreateDto dto, CancellationToken ct = default)
    {
        var profile = new VpnProfile
        {
            Type = dto.Type,
            CommonName = dto.CommonName,
            VpnIp = dto.VpnIp,
            Notes = dto.Notes,
            InstallationLocation = dto.InstallationLocation,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _context.VpnProfiles.Add(profile);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Perfil VPN criado: {CommonName} ({VpnIp})", profile.CommonName, profile.VpnIp);

        return Kk.Kharts.Shared.Extensions.VpnProfileExtensions.ToDto(profile);
    }

    public async Task<VpnProfileDto> UpdateAsync(int id, VpnProfileUpdateDto dto, CancellationToken ct = default)
    {
        var profile = await _context.VpnProfiles.FindAsync(new object[] { id }, ct);
        if (profile == null)
        {
            throw new KeyNotFoundException($"Perfil VPN {id} não encontrado.");
        }

        profile.Type = dto.Type;
        profile.CommonName = dto.CommonName;

        if (dto.Notes != null)
            profile.Notes = dto.Notes;

        if (dto.InstallationLocation != null)
            profile.InstallationLocation = dto.InstallationLocation;

        if (dto.IsActive.HasValue)
            profile.IsActive = dto.IsActive.Value;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Perfil VPN atualizado: {CommonName}", profile.CommonName);

        return Kk.Kharts.Shared.Extensions.VpnProfileExtensions.ToDto(profile);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var profile = await _context.VpnProfiles.FindAsync(new object[] { id }, ct);
        if (profile == null)
        {
            throw new KeyNotFoundException($"Perfil VPN {id} não encontrado.");
        }

        _context.VpnProfiles.Remove(profile);
        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Perfil VPN eliminado: {CommonName}", profile.CommonName);
    }

    public async Task<VpnProfileDto> AssignToUserAsync(int id, VpnProfileAssignDto dto, CancellationToken ct = default)
    {
        var profile = await _context.VpnProfiles
            .Include(v => v.AssignedUser)
            .Include(v => v.AssignedCompany)
            .FirstOrDefaultAsync(v => v.Id == id, ct);

        if (profile == null)
        {
            throw new KeyNotFoundException($"Perfil VPN {id} não encontrado.");
        }

        // Validar regras de negócio
        if (dto.CompanyId.HasValue && dto.UserId.HasValue)
        {
            throw new InvalidOperationException("Não é possível atribuir o perfil VPN a uma Company e a um User simultaneamente.");
        }

        if (!dto.CompanyId.HasValue && !dto.UserId.HasValue)
        {
            throw new InvalidOperationException("É necessário especificar CompanyId ou UserId para atribuição.");
        }

        // Regra: UG65 só pode ser atribuído a Company, PC só pode ser atribuído a User
        if (profile.Type.ToLower() == "ug65" && dto.UserId.HasValue)
        {
            throw new InvalidOperationException("Perfis UG65 só podem ser atribuídos a Companies (gateways).");
        }

        if (profile.Type.ToLower() == "pc" && dto.CompanyId.HasValue)
        {
            throw new InvalidOperationException("Perfis PC só podem ser atribuídos a Users.");
        }

        // Atribuição a Company (para UG65)
        if (dto.CompanyId.HasValue)
        {
            var company = await _context.Companies.FindAsync(new object[] { dto.CompanyId.Value }, ct);
            if (company == null)
            {
                throw new KeyNotFoundException($"Company {dto.CompanyId.Value} não encontrada.");
            }

            profile.AssignedCompanyId = dto.CompanyId.Value;
            profile.AssignedUserId = null; // Limpar atribuição anterior
            profile.AssignedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(dto.InstallationLocation))
            {
                profile.InstallationLocation = dto.InstallationLocation;
            }

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Perfil VPN {CommonName} ({Type}) atribuído à Company {CompanyName} ({CompanyId})",
                profile.CommonName, profile.Type, company.Name, company.Id);

            return Kk.Kharts.Shared.Extensions.VpnProfileExtensions.ToDto(profile);
        }

        // Atribuição a User (para PC) - apenas Users Root
        if (dto.UserId.HasValue)
        {
            var user = await _context.Users.FindAsync(new object[] { dto.UserId.Value }, ct);
            if (user == null)
            {
                throw new KeyNotFoundException($"User {dto.UserId.Value} não encontrado.");
            }

            // Validar se o user é Root
            if (user.Role?.ToLower() != "root")
            {
                throw new InvalidOperationException("Perfis PC só podem ser atribuídos a Users com papel Root.");
            }

            profile.AssignedUserId = dto.UserId.Value;
            profile.AssignedCompanyId = null; // Limpar atribuição anterior
            profile.AssignedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(dto.InstallationLocation))
            {
                profile.InstallationLocation = dto.InstallationLocation;
            }

            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Perfil VPN {CommonName} ({Type}) atribuído ao User {UserName} ({UserId})",
                profile.CommonName, profile.Type, user.Nom, user.Id);

            return Kk.Kharts.Shared.Extensions.VpnProfileExtensions.ToDto(profile);
        }

        throw new InvalidOperationException("Operação de atribuição inválida.");
    }

    public async Task<VpnProfileDto> UnassignFromUserAsync(int id, CancellationToken ct = default)
    {
        var profile = await _context.VpnProfiles
            .Include(v => v.AssignedUser)
            .Include(v => v.AssignedCompany)
            .FirstOrDefaultAsync(v => v.Id == id, ct);

        if (profile == null)
        {
            throw new KeyNotFoundException($"Perfil VPN {id} não encontrado.");
        }

        var previousUserId = profile.AssignedUserId;
        var previousCompanyId = profile.AssignedCompanyId;
        var previousEntityName = previousCompanyId.HasValue ? 
            profile.AssignedCompany?.Name : 
            profile.AssignedUser?.Nom;

        // Limpar ambas as atribuições
        profile.AssignedUserId = null;
        profile.AssignedCompanyId = null;
        profile.AssignedAt = null;

        await _context.SaveChangesAsync(ct);

        if (previousCompanyId.HasValue)
        {
            _logger.LogInformation("Perfil VPN {CommonName} desatribuído da Company {EntityName} ({CompanyId})",
                profile.CommonName, previousEntityName, previousCompanyId.Value);
        }
        else if (previousUserId.HasValue)
        {
            _logger.LogInformation("Perfil VPN {CommonName} desatribuído do User {EntityName} ({UserId})",
                profile.CommonName, previousEntityName, previousUserId.Value);
            return Kk.Kharts.Shared.Extensions.VpnProfileExtensions.ToDto(profile);
        }

        return Kk.Kharts.Shared.Extensions.VpnProfileExtensions.ToDto(profile);
    }

    public async Task<VpnProfileDto> UploadOvpnFileAsync(int id, string fileName, string fileContent, CancellationToken ct = default)
    {
        var profile = await _context.VpnProfiles.FindAsync(new object[] { id }, ct);
        if (profile == null)
        {
            throw new KeyNotFoundException($"Perfil VPN {id} não encontrado.");
        }

        profile.OvpnFileName = fileName;
        profile.OvpnContent = fileContent;

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Ficheiro OVPN carregado para o perfil {CommonName}: {FileName}",
            profile.CommonName, fileName);

        return Kk.Kharts.Shared.Extensions.VpnProfileExtensions.ToDto(profile);
    }

    public async Task<string> DownloadOvpnFileAsync(int id, CancellationToken ct = default)
    {
        var profile = await _context.VpnProfiles.FindAsync(new object[] { id }, ct);
        if (profile == null)
        {
            throw new KeyNotFoundException($"Perfil VPN {id} não encontrado.");
        }

        if (string.IsNullOrWhiteSpace(profile.OvpnContent))
        {
            throw new InvalidOperationException("Nenhum ficheiro OVPN encontrado para este perfil.");
        }

        return profile.OvpnContent;
    }

    public async Task<int> ImportFromCsvAsync(IEnumerable<VpnProfileImportDto> profiles, CancellationToken ct = default)
    {
        var count = 0;

        foreach (var dto in profiles)
        {
            var exists = await _context.VpnProfiles
                .AnyAsync(v => v.CommonName == dto.CommonName || v.VpnIp == dto.VpnIp, ct);

            if (exists)
            {
                _logger.LogWarning("Perfil VPN duplicado ignorado: {CommonName} ({VpnIp})", dto.CommonName, dto.VpnIp);
                continue;
            }

            var profile = new VpnProfile
            {
                Type = dto.Type,
                CommonName = dto.CommonName,
                VpnIp = dto.VpnIp,
                Notes = dto.Notes,
                CreatedAt = dto.CreatedAt,
                IsActive = true
            };

            _context.VpnProfiles.Add(profile);
            count++;
        }

        await _context.SaveChangesAsync(ct);

        _logger.LogInformation("Importados {Count} perfis VPN a partir do CSV", count);

        return count;
    }
}
