using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Shared.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Kk.Kharts.Api.Controllers
{
    //[Route("api/v1/[controller]")]
    [Route("api/v1/dashboards")]
    [ApiController]
    [Authorize]

    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IUserContext _userContext;

        public DashboardController(AppDbContext context, IUserContext userContext)
        {
            _context = context;
            _userContext = userContext;
        }


        [HttpPost("state")]
        public async Task<IActionResult> SaveDashboardState([FromBody] JsonElement stateJson)
        {
            var authenticatedUser = await _userContext.GetUserInfoFromToken();

            if (stateJson.ValueKind == JsonValueKind.Undefined || stateJson.ValueKind == JsonValueKind.Null)
                return BadRequest("Le JSON du tableau de bord est vide.");

            var rawJson = stateJson.GetRawText();

            var existingDashboard = await _context.Dashboards.FirstOrDefaultAsync(d => d.UserId == authenticatedUser.UserId);

            if (existingDashboard != null)
            {
                // Atualiza o dashboard existente
                existingDashboard.StateJson = rawJson;
                existingDashboard.CreatedAt = DateTime.UtcNow; // Atualize a data se necessário
                _context.Dashboards.Update(existingDashboard);
            }
            else
            {
                // Cria um novo dashboard
                var dashboard = new Dashboard
                {
                    StateJson = rawJson,
                    UserId = authenticatedUser.UserId,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Dashboards.Add(dashboard);
            }

            await _context.SaveChangesAsync();

            return Ok(new { Message = "État enregistré ou mis à jour avec succès." });
        }


        [HttpGet("state")]
        public async Task<IActionResult> GetMyDashboardStates()
        {
            var authenticatedUser = await _userContext.GetUserInfoFromToken();

            var states = await _context.Dashboards
                .Where(d => d.UserId == authenticatedUser.UserId)
                .OrderByDescending(d => d.CreatedAt)
                .ToListAsync();

            return Ok(states);
        }
    }
}