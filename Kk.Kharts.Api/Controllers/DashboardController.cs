using Kk.Kharts.Api.Services.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Kk.Kharts.Api.Controllers
{
    [Route("api/v1/dashboards")]
    [ApiController]
    [Authorize]

    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;
        private readonly IUserContext _userContext;

        public DashboardController(IDashboardService dashboardService, IUserContext userContext)
        {
            _dashboardService = dashboardService;
            _userContext = userContext;
        }


        [HttpPost("state")]
        public async Task<IActionResult> SaveDashboardState([FromBody] JsonElement stateJson)
        {
            var authenticatedUser = await _userContext.GetUserInfoFromToken();

            if (stateJson.ValueKind == JsonValueKind.Undefined || stateJson.ValueKind == JsonValueKind.Null)
                return BadRequest("Le JSON du tableau de bord est vide.");

            var rawJson = stateJson.GetRawText();
            await _dashboardService.SaveStateAsync(authenticatedUser.UserId, rawJson);

            return Ok(new { Message = "État enregistré ou mis à jour avec succès." });
        }


        [HttpGet("state")]
        public async Task<IActionResult> GetMyDashboardStates()
        {
            var authenticatedUser = await _userContext.GetUserInfoFromToken();
            var states = await _dashboardService.GetAllByUserIdAsync(authenticatedUser.UserId);
            return Ok(states);
        }
    }
}