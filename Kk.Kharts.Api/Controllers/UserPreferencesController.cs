using System.Security.Claims;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Utility.Constants;
using Kk.Kharts.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kk.Kharts.Api.Controllers;

/// <summary>
/// API dédiée à la gestion des préférences utilisateurs (canaux, accès, Pushover).
/// Toutes les réponses sont rédigées en français pour être directement consommées par l'UI.
/// </summary>
[ApiController]
[Route("api/v1/user-preferences")]
public class UserPreferencesController(IUserPreferencesService service, ILogger<UserPreferencesController> logger) : ControllerBase
{
    private readonly IUserPreferencesService _service = service;
    private readonly ILogger<UserPreferencesController> _logger = logger;

    /// <summary>
    /// Retourne les préférences d'un utilisateur (réservé aux administrateurs).
    /// </summary>
    [Authorize(Roles = Roles.Root + "," + Roles.SuperAdmin + "," + Roles.Admin)]
    [HttpGet("{userId:int}")]
    public async Task<IActionResult> Get(int userId, CancellationToken ct)
    {
        var prefs = await _service.GetAsync(userId, ct);
        return prefs is null ? NotFound("Utilisateur introuvable.") : Ok(prefs);
    }

    /// <summary>
    /// Retourne les options son/priorité Pushover ainsi que les contraintes retry/expire pour le front.
    /// </summary>
    [Authorize]
    [HttpGet("pushover/metadata")]
    public async Task<IActionResult> GetPushoverMetadata(CancellationToken ct)
    {
        var metadata = await _service.GetPushoverMetadataAsync(ct);
        return Ok(metadata);
    }

    /// <summary>
    /// Retourne les préférences de l'utilisateur connecté.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetForCurrentUser(CancellationToken ct)
    {
        var prefs = await _service.GetAsync(GetAuthenticatedUserId(), ct);
        return prefs is null ? NotFound("Utilisateur introuvable.") : Ok(prefs);
    }

    /// <summary>
    /// Met à jour le canal de notification d'un utilisateur (admin/support).
    /// </summary>
    [Authorize(Roles = Roles.Root + "," + Roles.SuperAdmin + "," + Roles.Admin)]
    [HttpPut("{userId:int}/notification-channel")]
    public async Task<IActionResult> UpdateNotificationChannel(int userId, [FromBody] UserNotificationPreferenceUpdateDto dto, CancellationToken ct)
    {
        var updated = await _service.UpdateNotificationPreferenceAsync(userId, dto, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Met à jour le propre canal de notification de l'utilisateur connecté.
    /// </summary>
    [Authorize]
    [HttpPut("me/notification-channel")]
    public async Task<IActionResult> UpdateNotificationChannelForCurrentUser([FromBody] UserNotificationPreferenceUpdateDto dto, CancellationToken ct)
    {
        var updated = await _service.UpdateNotificationPreferenceAsync(GetAuthenticatedUserId(), dto, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Ajuste la portée d'accès (entreprise / filiales) d'un utilisateur.
    /// </summary>
    [Authorize(Roles = Roles.Root + "," + Roles.SuperAdmin)]
    [HttpPut("{userId:int}/access-scope")]
    public async Task<IActionResult> UpdateAccessScope(int userId, [FromBody] UserAccessScopeUpdateDto dto, CancellationToken ct)
    {
        var updated = await _service.UpdateAccessScopeAsync(userId, dto, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Met à jour les paramètres Pushover d'un utilisateur (admin/support).
    /// </summary>
    [Authorize(Roles = Roles.Root + "," + Roles.SuperAdmin + "," + Roles.Admin + "," + Roles.UserRW)]
    [HttpPut("{userId:int}/pushover")]
    public async Task<IActionResult> UpdatePushover(int userId, [FromBody] UserPushoverSettingsUpdateDto dto, CancellationToken ct)
    {
        var updated = await _service.UpdatePushoverSettingsAsync(userId, dto, ct);
        return Ok(updated);
    }

    /// <summary>
    /// Permet à l'utilisateur connecté de gérer ses propres clés Pushover.
    /// </summary>
    [Authorize]
    [HttpPut("me/pushover")]
    public async Task<IActionResult> UpdatePushoverForCurrentUser([FromBody] UserPushoverSettingsUpdateDto dto, CancellationToken ct)
    {
        var updated = await _service.UpdatePushoverSettingsAsync(GetAuthenticatedUserId(), dto, ct);
        return Ok(updated);
    }

    private int GetAuthenticatedUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value
            ?? throw new InvalidOperationException("Jeton invalide: identifiant utilisateur manquant.");

        return int.Parse(idClaim);
    }
}
