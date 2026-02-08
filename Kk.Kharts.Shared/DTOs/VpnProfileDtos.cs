using System.ComponentModel.DataAnnotations;
using Kk.Kharts.Shared.Entities;

namespace Kk.Kharts.Shared.DTOs;

/// <summary>
/// DTO para listar perfis VPN.
/// </summary>
public class VpnProfileDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string CommonName { get; set; } = string.Empty;
    public string VpnIp { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? OvpnFileName { get; set; }
    
    // Atribuição - pode ser para Company ou User
    public int? AssignedUserId { get; set; }
    public string? AssignedUserName { get; set; }
    public int? AssignedCompanyId { get; set; }
    public string? AssignedCompanyName { get; set; }
    
    // Propriedades computadas para facilitar o frontend
    public string? AssignedEntityName => 
        AssignedCompanyId != null ? AssignedCompanyName : 
        AssignedUserId != null ? AssignedUserName : null;
        
    public string? AssignedEntityType =>
        AssignedCompanyId != null ? "Company" :
        AssignedUserId != null ? "User" : null;
    
    public string? InstallationLocation { get; set; }
    public DateTime? AssignedAt { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// DTO para criar um novo perfil VPN.
/// </summary>
public class VpnProfileCreateDto
{
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string CommonName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string VpnIp { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }

    [MaxLength(500)]
    public string? InstallationLocation { get; set; }
}

/// <summary>
/// DTO para atualizar um perfil VPN existente.
/// </summary>
public class VpnProfileUpdateDto
{
    [Required]
    [MaxLength(50)]
    public string Type { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string CommonName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }

    [MaxLength(500)]
    public string? InstallationLocation { get; set; }

    public bool? IsActive { get; set; }
}

/// <summary>
/// DTO para atribuir um perfil VPN (para Company ou User).
/// </summary>
public class VpnProfileAssignDto
{
    // Para atribuição a Company (gateways UG65)
    public int? CompanyId { get; set; }
    
    // Para atribuição a User (apenas PCs, users Root)
    public int? UserId { get; set; }
    
    [MaxLength(500)]
    public string? InstallationLocation { get; set; }
}

public class VpnProfileClaimDto
{
    [Required]
    public int ProfileId { get; set; }

    [MaxLength(500)]
    public string? InstallationLocation { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}

public class NextVpnProfileDto
{
    public int ProfileId { get; set; }
    public string VpnIp { get; set; } = string.Empty;
    public string CommonName { get; set; } = string.Empty;
    public int CompanyId { get; set; }
    public string? CompanyName { get; set; }
    public string? OvpnFileName { get; set; }
}

public class VpnInventoryUploadResultDto
{
    public int Imported { get; set; }
    public int Updated { get; set; }
    public int Skipped { get; set; }
}

/// <summary>
/// DTO para importar perfis VPN em lote a partir do CSV.
/// </summary>
public class VpnProfileImportDto
{
    [Required]
    public string Type { get; set; } = string.Empty;

    [Required]
    public string CommonName { get; set; } = string.Empty;

    [Required]
    public string VpnIp { get; set; } = string.Empty;

    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

public static class VpnProfileMappingExtensions
{
    public static VpnProfileDto ToDto(this VpnProfile profile)
    {
        return new VpnProfileDto
        {
            Id = profile.Id,
            Type = profile.Type,
            CommonName = profile.CommonName,
            VpnIp = profile.VpnIp,
            Notes = profile.Notes,
            CreatedAt = profile.CreatedAt,
            OvpnFileName = profile.OvpnFileName,
            AssignedUserId = profile.AssignedUserId,
            AssignedUserName = profile.AssignedUser?.Nom,
            AssignedCompanyId = profile.AssignedCompanyId,
            AssignedCompanyName = profile.AssignedCompany?.Name,
            InstallationLocation = profile.InstallationLocation,
            AssignedAt = profile.AssignedAt,
            IsActive = profile.IsActive
        };
    }
}
