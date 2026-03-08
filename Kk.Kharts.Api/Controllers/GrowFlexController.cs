using Kk.Kharts.Api.DTOs.Requests;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kk.Kharts.Api.Controllers;

[ApiController]
[Produces("application/json")]
public class GrowFlexController : ControllerBase
{
    private readonly IDrainPluviometreService _drainPluviometreService;
    private readonly IUserContext _userContext;

    public GrowFlexController(
        IDrainPluviometreService drainPluviometreService,
        IUserContext userContext)
    {
        _drainPluviometreService = drainPluviometreService;
        _userContext = userContext;
    }

    /// <summary>
    /// Calcule la drainage d'une régie à partir des pulses DI et du volume d'eau utilisé (source externe).
    /// </summary>
    /// <param name="devEui">Identifiant unique du dispositif EM300-DI.</param>
    /// <param name="request">startAt, endAt, waterUsedLiters.</param>
    [Authorize]
    [HttpPost("api/v1/growflex/mustache/drain-pluviometre")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CalculateDrainage([FromQuery] string devEui, [FromBody] DrainPluviometreRequest request, CancellationToken ct)
    {
        try
        {
            HistoricalQueryRangeGuard.ValidateOrThrow(request.StartAt, request.EndAt);
            devEui = DevEuiNormalizer.Normalize(devEui);
            var authenticatedUser = await _userContext.GetUserInfoFromToken();

            var result = await _drainPluviometreService.CalculateAsync(
                devEui,
                request.StartAt,
                request.EndAt,
                request.WaterUsedLiters,
                authenticatedUser,
                ct);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }


    /// <summary>
    /// Calcule le drainage sur un intervalle (sans persistance) pour affichage front.
    /// </summary>
    /// <param name="devEui">Identifiant unique du dispositif EM300-DI.</param>
    /// <param name="startDate">Date de début (UTC ou avec offset).</param>
    /// <param name="endDate">Date de fin (UTC ou avec offset).</param>
    /// <param name="waterUsedLiters">Volume d'eau apporté (optionnel, défaut 0).</param>
    [Authorize]
    [HttpGet("api/v1/growflex/mustache/drain-pluviometre")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDrainage(
        [FromQuery] string devEui,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] double? waterUsedLiters,
        CancellationToken ct)
    {
        try
        {
            HistoricalQueryRangeGuard.ValidateOrThrow(startDate, endDate);
            devEui = DevEuiNormalizer.Normalize(devEui);
            var authenticatedUser = await _userContext.GetUserInfoFromToken();

            var result = await _drainPluviometreService.CalculateAsync(
                devEui,
                startDate,
                endDate,
                waterUsedLiters ?? 0,
                authenticatedUser,
                ct);

            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
