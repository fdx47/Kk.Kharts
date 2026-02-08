using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kk.Kharts.Api.Controllers;

/// <summary>
/// API pour les activités récentes du système KropKontrol.
/// </summary>
[ApiController]
[Authorize]
[Route("api/v1/activities")]
public class ActivityController : ControllerBase
{
    private readonly IActivityService _activityService;

    public ActivityController(IActivityService activityService)
    {
        _activityService = activityService;
    }

    /// <summary>
    /// Récupère les activités récentes du système.
    /// </summary>
    /// <param name="count">Nombre d'activités à récupérer (défaut: 10)</param>
    /// <returns>Liste des activités récentes.</returns>
    /// <response code="200">Activités récupérées avec succès.</response>
    /// <response code="401">Non autorisé - Token JWT manquant ou invalide.</response>
    [HttpGet]
    [ProducesResponseType(typeof(List<RecentActivityDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRecentActivities([FromQuery] int count = 10)
    {
        try
        {
            var activities = await _activityService.GetRecentActivitiesAsync(count);
            return Ok(activities);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
