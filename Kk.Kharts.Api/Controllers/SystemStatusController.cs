using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kk.Kharts.Api.Controllers;

/// <summary>
/// API pour le statut du système KropKontrol.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/system")]
public class SystemStatusController : ControllerBase
{
    private readonly ISystemStatusService _systemStatusService;

    public SystemStatusController(ISystemStatusService systemStatusService)
    {
        _systemStatusService = systemStatusService;
    }

    /// <summary>
    /// Récupère le statut actuel du système.
    /// </summary>
    /// <returns>Statut du système avec toutes les métriques.</returns>
    /// <response code="200">Statut récupéré avec succès.</response>
    /// <response code="401">Non autorisé - Token JWT manquant ou invalide.</response>
    [HttpGet("status")]
    [ProducesResponseType(typeof(SystemStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetSystemStatus()
    {
        try
        {
            var status = await _systemStatusService.GetSystemStatusAsync();
            return Ok(status);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
