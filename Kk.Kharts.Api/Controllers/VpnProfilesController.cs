using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Utility.Constants;
using Kk.Kharts.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text;

namespace Kk.Kharts.Api.Controllers;

/// <summary>
/// API de gestion des profils VPN (gateways UG65 et PCs).
/// Réservé aux utilisateurs Root.
/// </summary>
[ApiController]
[Route("api/v1/vpn-profiles")]
[Authorize(Roles = Roles.Root)]
public class VpnProfilesController : ControllerBase
{
    private readonly IVpnProfileService _service;
    private readonly ILogger<VpnProfilesController> _logger;

    public VpnProfilesController(IVpnProfileService service, ILogger<VpnProfilesController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Liste tous les profils VPN.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var profiles = await _service.GetAllAsync(ct);
        return Ok(profiles);
    }

    /// <summary>
    /// Récupère un profil VPN par ID.
    /// </summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var profile = await _service.GetByIdAsync(id, ct);
        return profile == null ? NotFound($"Profil VPN {id} introuvable.") : Ok(profile);
    }

    /// <summary>
    /// Crée un nouveau profil VPN.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] VpnProfileCreateDto dto, CancellationToken ct)
    {
        var profile = await _service.CreateAsync(dto, ct);
        return CreatedAtAction(nameof(GetById), new { id = profile.Id }, profile);
    }

    /// <summary>
    /// Met à jour un profil VPN existant.
    /// </summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] VpnProfileUpdateDto dto, CancellationToken ct)
    {
        try
        {
            var profile = await _service.UpdateAsync(id, dto, ct);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Supprime un profil VPN.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        try
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Attribue un profil VPN à un utilisateur.
    /// </summary>
    [HttpPost("{id:int}/assign")]
    public async Task<IActionResult> AssignToUser(int id, [FromBody] VpnProfileAssignDto dto, CancellationToken ct)
    {
        try
        {
            var profile = await _service.AssignToUserAsync(id, dto, ct);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Retire l'attribution d'un profil VPN.
    /// </summary>
    [HttpPost("{id:int}/unassign")]
    public async Task<IActionResult> UnassignFromUser(int id, CancellationToken ct)
    {
        try
        {
            var profile = await _service.UnassignFromUserAsync(id, ct);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Téléverse un fichier .ovpn pour un profil VPN.
    /// </summary>
    [HttpPost("{id:int}/upload-ovpn")]
    public async Task<IActionResult> UploadOvpnFile(int id, IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Fichier non fourni ou vide.");
        }

        if (!file.FileName.EndsWith(".ovpn", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Seuls les fichiers .ovpn sont autorisés.");
        }

        try
        {
            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync(ct);

            var profile = await _service.UploadOvpnFileAsync(id, file.FileName, content, ct);
            return Ok(profile);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
    }

    /// <summary>
    /// Télécharge le fichier .ovpn d'un profil VPN.
    /// </summary>
    [HttpGet("{id:int}/download-ovpn")]
    public async Task<IActionResult> DownloadOvpnFile(int id, CancellationToken ct)
    {
        try
        {
            var content = await _service.DownloadOvpnFileAsync(id, ct);
            var profile = await _service.GetByIdAsync(id, ct);

            var fileName = profile?.OvpnFileName ?? $"{profile?.CommonName}.ovpn";
            var bytes = Encoding.UTF8.GetBytes(content);

            return File(bytes, "application/x-openvpn-profile", fileName);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    /// <summary>
    /// Importe des profils VPN à partir d'un fichier CSV.
    /// </summary>
    [HttpPost("import-csv")]
    public async Task<IActionResult> ImportFromCsv(IFormFile file, CancellationToken ct)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Fichier CSV non fourni ou vide.");
        }

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest("Seuls les fichiers .csv sont autorisés.");
        }

        try
        {
            var profiles = new List<VpnProfileImportDto>();

            using var reader = new StreamReader(file.OpenReadStream());
            
            // Skip header
            await reader.ReadLineAsync(ct);

            while (await reader.ReadLineAsync(ct) is { } line)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = line.Split(',');
                if (parts.Length < 5)
                    continue;

                var profile = new VpnProfileImportDto
                {
                    Type = parts[0].Trim(),
                    CommonName = parts[1].Trim(),
                    VpnIp = parts[2].Trim(),
                    Notes = parts[3].Trim('"'),
                    CreatedAt = DateTime.TryParse(parts[4].Trim(), CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var date)
                        ? date
                        : DateTime.UtcNow
                };

                profiles.Add(profile);
            }

            var count = await _service.ImportFromCsvAsync(profiles, ct);

            return Ok(new { ImportedCount = count, TotalLines = profiles.Count });
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Erro ao processar ficheiro: {ex.Message}");
        }
    }
}
