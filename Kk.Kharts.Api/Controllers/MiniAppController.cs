using Kk.Kharts.Api.DTOs.MiniApp;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Services.Telegram;
using Kk.Kharts.Api.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Kk.Kharts.Api.Controllers;

/// <summary>
/// Controller pour la Telegram Mini App.
/// Fournit des endpoints pour l'authentification, les devices, les alertes et le support.
/// </summary>
[ApiController]
[Route("api/v1/miniapp")]
[Authorize]
public class MiniAppController(
    IMiniAppService miniAppService,
    ITelegramUserService telegramUserService,
    ITelegramService telegramService,
    IJwtService jwtService,
    ILogger<MiniAppController> logger) : ControllerBase
{
    /// <summary>
    /// Authentifie un utilisateur via son Telegram ID.
    /// </summary>
    [HttpPost("auth")]
    [AllowAnonymous]
    public async Task<IActionResult> Authenticate([FromBody] MiniAppAuthRequest request, CancellationToken ct)
    {
        try
        {
            if (request.TelegramId <= 0)
            {
                return Ok(new { success = false, isLinked = false, message = "Telegram ID invalide" });
            }

            var user = await telegramUserService.GetUserByTelegramIdAsync(request.TelegramId, ct);

            if (user == null)
            {
                return Ok(new
                {
                    success = true,
                    isLinked = false,
                    message = "Compte non lié. Utilisez /link dans le bot."
                });
            }

            var token = jwtService.GenerateJwtToken(user, request.TelegramId);

            return Ok(new
            {
                success = true,
                isLinked = true,
                token = token,
                user = new
                {
                    id = user.Id,
                    email = user.Email,
                    name = user.Nom,
                    company = user.Company?.Name
                }
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de l'authentification Mini App pour TelegramId {TelegramId}", request.TelegramId);
            return Ok(new { success = false, isLinked = false, message = "Erreur serveur" });
        }
    }

    /// <summary>
    /// Récupère les devices de l'utilisateur.
    /// </summary>
    [HttpGet("devices")]
    public async Task<IActionResult> GetDevices(CancellationToken ct)
    {
        try
        {
            var user = await GetRequestUserAsync(ct);
            if (user is null)
                return Ok(Array.Empty<object>());

            var result = await miniAppService.GetDevicesWithReadingsAsync(user, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la récupération des devices");
            return Ok(Array.Empty<object>());
        }
    }

    /// <summary>
    /// Récupère l'état du dashboard para um dispositivo específico.
    /// Chamado pelo frontend para obter valores em tempo real.
    /// </summary>
    [HttpGet("dashboard/state")]
    public async Task<IActionResult> GetDashboardState([FromQuery] string devEui, CancellationToken ct)
    {
        try
        {
            devEui = DevEuiNormalizer.Normalize(devEui);
            var user = await GetRequestUserAsync(ct);
            if (user is null) return Unauthorized();

            if (!await HasAccessToDeviceAsync(user, devEui, ct))
                return Unauthorized();

            var result = await miniAppService.GetDashboardStateAsync(devEui, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la récupération de l'état du dashboard pour {DevEui}", devEui);
            return Ok(new { });
        }
    }

    /// <summary>
    /// Récupère les alertes actives de l'utilisateur.
    /// </summary>
    [HttpGet("alerts")]
    public async Task<IActionResult> GetAlerts(CancellationToken ct)
    {
        try
        {
            var user = await GetRequestUserAsync(ct);
            if (user is null)
                return Ok(Array.Empty<object>());

            var devices = await miniAppService.GetDevicesForUserAsync(user, ct);
            var devEuis = devices.Select(d => d.DevEui).ToList();

            var alerts = await miniAppService.GetActiveAlertsAsync(devEuis, ct);
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la récupération des alertes");
            return Ok(Array.Empty<object>());
        }
    }

    /// <summary>
    /// Récupère les statistiques de l'utilisateur.
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats(CancellationToken ct)
    {
        try
        {
            var user = await GetRequestUserAsync(ct);
            if (user is null)
                return Ok(new MiniAppStatsDto(0, 0, 0, 0));

            var devices = await miniAppService.GetDevicesForUserAsync(user, ct);
            var devEuis = devices.Select(d => d.DevEui).ToList();

            var twoHoursAgo = DateTime.UtcNow.AddHours(-2);
            var onlineCount = devices.Count(d => Services.MiniAppService.NormalizeToUtc(d.LastSeenAt) > twoHoursAgo);

            var stats = await miniAppService.GetStatsAsync(devEuis, devices.Count, ct);

            return Ok(stats with
            {
                OnlineDevices = onlineCount,
                OfflineDevices = devices.Count - onlineCount
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la récupération des stats");
            return Ok(new MiniAppStatsDto(0, 0, 0, 0));
        }
    }

    /// <summary>
    /// Envoie un message au support.
    /// </summary>
    [HttpPost("support")]
    public async Task<IActionResult> SendSupportMessage([FromBody] SupportMessageRequest request, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return Ok(new { success = false, message = "Message vide" });
            }

            var message = request.Message.Trim();

            if (message.Length > 5000)
            {
                return Ok(new { success = false, message = "Message trop long (max 5000 caractères)" });
            }

            if (message.Length < 10)
            {
                return Ok(new { success = false, message = "Message trop court (min 10 caractères)" });
            }

            var user = await GetRequestUserAsync(ct);
            var telegramId = (long?)(HttpContext.Items["TelegramId"]) ?? request.TelegramId; // Fallback para ID do form se necessário, mas preferir token
            var userName = user?.Nom ?? request.UserName ?? "Utilisateur Mini App";
            var userEmail = user?.Email ?? "N/A";

            var supportMessage = $"""
                📱 <b>Message envoyé depuis la Mini-App</b>
                
                - Demande d'assistance:

                👤 <b>De:</b> {telegramService.EscapeHtml(userName)}
                📧 <b>Email:</b> {telegramService.EscapeHtml(userEmail)}
                🆔 <b>Telegram ID:</b> {request.TelegramId?.ToString() ?? user?.TelegramUserId?.ToString() ?? "N/A"}
                
                💬 <b>Message:</b>
                {telegramService.EscapeHtml(message)}
                """;

            await telegramService.SendToDebugTopicAsync(supportMessage, ct: ct);

            return Ok(new { success = true, message = "Votre message a bien été envoyé au support technique !" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de l'envoi du message au support");
            return Ok(new { success = false, message = "Impossible d'envoyer le message. Veuillez réessayer." });

        }
    }

    /// <summary>
    /// Récupère les données historiques d'un device (UC502 Wet150).
    /// </summary>
    [HttpGet("devices/{devEui}/data")]
    public async Task<IActionResult> GetDeviceData(
        string devEui,
        [FromQuery] string period = "36h",
        CancellationToken ct = default)
    {
        try
        {
            devEui = DevEuiNormalizer.Normalize(devEui);
            var user = await GetRequestUserAsync(ct);
            if (user is null) return Ok(Array.Empty<object>());

            if (!await HasAccessToDeviceAsync(user, devEui, ct))
                return Ok(Array.Empty<object>());

            var result = await miniAppService.GetDeviceDataAsync(devEui, period, ct);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la récupération des données pour {DevEui}", devEui);
            return Ok(Array.Empty<object>());
        }
    }

    /// <summary>
    /// Récupère les seuils d'alarme d'un device via AlarmRules.
    /// </summary>
    [HttpGet("devices/{devEui}/thresholds")]
    public async Task<IActionResult> GetDeviceThresholds(
        string devEui,
        CancellationToken ct)
    {
        try
        {
            devEui = DevEuiNormalizer.Normalize(devEui);
            var telegramId = (long?)(HttpContext.Items["TelegramId"]) ?? 0;
            if (telegramId == 0 || !await telegramUserService.HasAccessToDeviceAsync(telegramId, devEui, ct))
                return Ok(Array.Empty<object>());

            var thresholds = await miniAppService.GetDeviceThresholdsAsync(devEui, ct);
            return Ok(thresholds);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la récupération des seuils pour {DevEui}", devEui);
            return Ok(Array.Empty<object>());
        }
    }

    /// <summary>
    /// Met à jour les seuils d'alarme d'un device via AlarmRules.
    /// </summary>
    [HttpPut("devices/{devEui}/thresholds")]
    public async Task<IActionResult> UpdateDeviceThresholds(
        string devEui,
        [FromBody] UpdateThresholdsRequest request,
        CancellationToken ct)
    {
        try
        {
            devEui = DevEuiNormalizer.Normalize(devEui);
            var user = await GetRequestUserAsync(ct);
            if (user is null) return Unauthorized();

            if (!await HasAccessToDeviceAsync(user, devEui, ct))
                return Forbid();

            var updated = await miniAppService.UpdateDeviceThresholdsAsync(devEui, request.Thresholds, ct);
            if (!updated)
                return NotFound(new { success = false, message = "Device non trouvé" });

            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors de la mise à jour des seuils pour {DevEui}", devEui);
            return Ok(new { success = false, message = "Erreur lors de la mise à jour" });
        }
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var user = await GetRequestUserAsync(ct);
        if (user == null) return Unauthorized();

        return Ok(new MiniAppProfileDto(
            user.Id,
            user.Email,
            user.Nom,
            user.Company?.Name,
            User.HasClaim(c => c.Type == "telegramId")));
    }

    // ── Private helpers (HTTP-layer concerns only) ────────────

    private async Task<Shared.Entities.User?> GetRequestUserAsync(CancellationToken ct)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdClaim, out var userId))
            return await miniAppService.GetUserByIdWithCompanyAsync(userId, ct);

        if (HttpContext.Items.TryGetValue("UserId", out var userObj) && userObj is int ctxUserId && ctxUserId > 0)
            return await miniAppService.GetUserByIdWithCompanyAsync(ctxUserId, ct);

        return null;
    }

    private async Task<bool> HasAccessToDeviceAsync(Shared.Entities.User user, string devEui, CancellationToken ct)
    {
        if (HttpContext.Items.TryGetValue("TelegramId", out var tgObj) && tgObj is long telegramId && telegramId > 0)
            return await telegramUserService.HasAccessToDeviceAsync(telegramId, devEui, ct);

        return await miniAppService.UserHasAccessToDeviceAsync(user, devEui, ct);
    }
}
