using System.ComponentModel.DataAnnotations;

namespace Kk.Kharts.Shared.Entities;

/// <summary>
/// Representa um perfil VPN (gateway UG65 ou PC) com ficheiro .ovpn associado.
/// </summary>
public class VpnProfile
{
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public required string Type { get; set; } = string.Empty; // 'ug65' ou 'pc'

    [Required]
    [MaxLength(100)]
    public required string CommonName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public required string VpnIp { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Conteúdo do ficheiro .ovpn (armazenado como texto).
    /// </summary>
    public string? OvpnContent { get; set; }

    /// <summary>
    /// Nome original do ficheiro .ovpn.
    /// </summary>
    [MaxLength(200)]
    public string? OvpnFileName { get; set; }

    /// <summary>
    /// Utilizador a quem este perfil VPN foi atribuído.
    /// </summary>
    public int? AssignedUserId { get; set; }
    public User? AssignedUser { get; set; }

    /// <summary>
    /// Empresa a quem este perfil VPN foi atribuído.
    /// </summary>
    public int? AssignedCompanyId { get; set; }
    public Company? AssignedCompany { get; set; }

    /// <summary>
    /// Localização/instalação onde o gateway está implantado.
    /// </summary>
    [MaxLength(500)]
    public string? InstallationLocation { get; set; }

    /// <summary>
    /// Data de atribuição ao utilizador.
    /// </summary>
    public DateTime? AssignedAt { get; set; }

    /// <summary>
    /// Indica se o perfil está ativo/em uso.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
