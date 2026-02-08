using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Services;
using Kk.Kharts.Api.Services.Telegram;
using Kk.Kharts.Shared.Entities;
using Kk.Kharts.Shared.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Claims;
using System.Text.Json;

namespace Kk.Kharts.Api.Controllers;

/// <summary>
/// Controller pour la Telegram Mini App.
/// Fournit des endpoints pour l'authentification, les devices, les alertes et le support.
/// </summary>
[ApiController]
[Route("api/v1/miniapp")]
[Authorize]
public class MiniAppController(
    AppDbContext db,
    ITelegramUserService telegramUserService,
    ITelegramService telegramService,
    IDashboardConfigService dashboardConfigService,
    JwtService jwtService,
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

            var devices = await GetDevicesForUserAsync(user, ct);
            var variablesByDevice = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            // Obter configuração do Dashboard (StateJson)
            var dashboard = await db.Dashboards
                .Where(d => d.UserId == user.Id)
                .OrderByDescending(d => d.CreatedAt)
                .FirstOrDefaultAsync(ct);

            if (dashboard?.StateJson != null)
            {
                try
                {
                    using var doc = JsonDocument.Parse(dashboard.StateJson);
                    // O JSON pode ser uma lista de wrappers ou o state direto dependendo de como é salvo
                    // Baseado no exemplo do user: wrapper.stateJson -> dashboardState

                    JsonElement dashboardState;
                    if (doc.RootElement.ValueKind == JsonValueKind.Array)
                    {
                        // Se for uma lista de wrappers (como no exemplo do user)
                        var firstWrapper = doc.RootElement.EnumerateArray().FirstOrDefault();
                        if (firstWrapper.TryGetProperty("stateJson", out var sj))
                        {
                            using var innerDoc = JsonDocument.Parse(sj.GetString() ?? "{}");
                            innerDoc.RootElement.TryGetProperty("dashboardState", out dashboardState);
                        }
                        else
                        {
                            doc.RootElement.TryGetProperty("dashboardState", out dashboardState);
                        }
                    }
                    else
                    {
                        doc.RootElement.TryGetProperty("dashboardState", out dashboardState);
                    }

                    if (dashboardState.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var deviceState in dashboardState.EnumerateArray())
                        {
                            if (deviceState.TryGetProperty("devEui", out var de) &&
                                deviceState.TryGetProperty("variables", out var vars) &&
                                vars.ValueKind == JsonValueKind.Array)
                            {
                                var eui = de.GetString();
                                if (string.IsNullOrEmpty(eui)) continue;

                                var varList = vars.EnumerateArray()
                                    .Select(v => v.GetString() ?? "")
                                    .Where(v => !string.IsNullOrEmpty(v))
                                    .ToList();

                                variablesByDevice[eui] = varList;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Erreur lors du parsing du Dashboard StateJson para o usuário {UserId}", user.Id);
                }
            }

            // Backup/Fallback: se não encontrou no dashboard, tentar via charts service
            if (variablesByDevice.Count == 0)
            {
                var charts = await dashboardConfigService.GetUserChartsAsync(user.Id, ct);
                foreach (var chart in charts)
                {
                    if (string.IsNullOrWhiteSpace(chart.DevEui)) continue;
                    if (!variablesByDevice.TryGetValue(chart.DevEui, out var vars))
                    {
                        vars = new List<string>();
                        variablesByDevice[chart.DevEui] = vars;
                    }

                    if (chart.Variables != null)
                    {
                        foreach (var v in chart.Variables)
                        {
                            if (!string.IsNullOrWhiteSpace(v) && !vars.Contains(v.Trim(), StringComparer.OrdinalIgnoreCase))
                                vars.Add(v.Trim());
                        }
                    }
                }
            }

            // Récupérer les dernières valeurs des capteurs
            var devEuis = devices.Select(d => d.DevEui).ToList();

            var lastEm300Readings = await db.Em300ths
                .Where(x => devEuis.Contains(x.DevEui))
                .GroupBy(x => x.DevEui)
                .Select(g => g.OrderByDescending(x => x.Timestamp).First())
                .ToDictionaryAsync(x => x.DevEui, StringComparer.OrdinalIgnoreCase, ct);

            var lastUc502Readings = await db.Uc502Wet150s
                .Where(x => devEuis.Contains(x.DevEui))
                .GroupBy(x => x.DevEui)
                .Select(g => g.OrderByDescending(x => x.Timestamp).First())
                .ToDictionaryAsync(x => x.DevEui, StringComparer.OrdinalIgnoreCase, ct);

            var twoHoursAgo = DateTime.UtcNow.AddHours(-2);
            var result = devices.Select(d =>
            {
                var lastSeen = NormalizeToUtc(d.LastSeenAt);
                if (TryParseLastSendAt(d.LastSendAt, out var parsedSendAt))
                {
                    lastSeen = parsedSendAt;
                }

                lastEm300Readings.TryGetValue(d.DevEui, out var em300);
                lastUc502Readings.TryGetValue(d.DevEui, out var uc502);

                return new
                {
                    devEui = d.DevEui,
                    name = d.Name,
                    description = d.Description,
                    installationLocation = d.InstallationLocation,
                    model = d.ModeloNavegacao?.Model ?? "Unknown",
                    isOnline = lastSeen > twoHoursAgo,
                    lastSeenAt = lastSeen,
                    battery = d.Battery,
                    company = d.Company?.Name,
                    variables = variablesByDevice.TryGetValue(d.DevEui, out var monitoredVars)
                        ? monitoredVars
                        : new List<string>(),

                    // Valeurs de capteurs
                    lastTemperature = em300?.Temperature ?? uc502?.SoilTemperature,
                    lastHumidity = em300?.Humidity ?? (uc502 != null ? (float?)null : null), // Se for UC502, Humildade do ar pode ser nula
                    lastVwc = uc502?.MineralVWC,
                    lastEc = uc502?.MineralECp
                };
            });

            return Ok(result);
        }
        catch (Exception ex)
        {
            var user = await GetRequestUserAsync(ct);
            logger.LogError(ex, "Erreur lors de la récupération des devices pour l'utilisateur {UserId}", user?.Id.ToString() ?? "Anonyme");
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
            devEui = NormalizeDevEui(devEui);
            var user = await GetRequestUserAsync(ct);
            if (user is null) return Unauthorized();

            // Verificar acesso ao dispositivo
            bool hasAccess = false;
            if (HttpContext.Items.TryGetValue("TelegramId", out var tgObj) && tgObj is long telegramId && telegramId > 0)
            {
                hasAccess = await telegramUserService.HasAccessToDeviceAsync(telegramId, devEui, ct);
            }
            else
            {
                // Para usuários mobile/web, o GetDevicesForUserAsync já filtra, 
                // mas para endpoints individuais como este, precisamos validar.
                var devices = await GetDevicesForUserAsync(user, ct);
                hasAccess = devices.Any(d => d.DevEui == devEui);
            }

            if (!hasAccess)
            {
                return Unauthorized();
            }

            var em300 = await db.Em300ths
                .Where(x => x.DevEui == devEui)
                .OrderByDescending(x => x.Timestamp)
                .FirstOrDefaultAsync(ct);

            var uc502 = await db.Uc502Wet150s
                .Where(x => x.DevEui == devEui)
                .OrderByDescending(x => x.Timestamp)
                .FirstOrDefaultAsync(ct);

            return Ok(new
            {
                temperature = em300?.Temperature ?? uc502?.SoilTemperature,
                humidity = em300?.Humidity ?? (uc502 != null ? (float?)null : null),
                vwc = uc502?.MineralVWC,
                ec = uc502?.MineralECp,
                timestamp = em300?.Timestamp ?? uc502?.Timestamp ?? DateTime.UtcNow
            });
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

            var devices = await GetDevicesForUserAsync(user, ct);
            var devEuis = devices.Select(d => d.DevEui).ToList();

            var alerts = await db.AlarmRules
                .Include(a => a.Device)
                .Where(a => devEuis.Contains(a.Device.DevEui) && a.Enabled && a.IsAlarmActive)
                .OrderByDescending(a => a.Id)
                .Take(50)
                .Select(a => new
                {
                    id = a.Id,
                    devEui = a.Device.DevEui,
                    deviceName = a.Device.Description ?? a.Device.Name,
                    type = a.ActiveThresholdType ?? "threshold",
                    message = $"{a.PropertyName} {(a.ActiveThresholdType == "Low" ? "en dessous de " + a.LowValue : "au dessus de " + a.HighValue)}",
                    isActive = a.IsAlarmActive,
                    triggeredAt = DateTime.UtcNow
                })
                .ToListAsync(ct);

            return Ok(alerts);
        }
        catch (Exception ex)
        {
            var user = await GetRequestUserAsync(ct);
            logger.LogError(ex, "Erreur lors de la récupération des alertes pour l'utilisateur {UserId}", user?.Id.ToString() ?? "Anonyme");
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
            {
                return Ok(new { totalDevices = 0, onlineDevices = 0, offlineDevices = 0, activeAlerts = 0 });
            }

            var devices = await GetDevicesForUserAsync(user, ct);
            var devEuis = devices.Select(d => d.DevEui).ToList();

            var activeAlerts = await db.AlarmRules
                .Include(a => a.Device)
                .CountAsync(a => devEuis.Contains(a.Device.DevEui) && a.Enabled && a.IsAlarmActive, ct);

            // Calcular online/offline baseado em LastSeenAt (online se visto nas últimas 2 horas)
            var twoHoursAgo = DateTime.UtcNow.AddHours(-2);
            var onlineCount = devices.Count(d => NormalizeToUtc(d.LastSeenAt) > twoHoursAgo);

            return Ok(new
            {
                totalDevices = devices.Count,
                onlineDevices = onlineCount,
                offlineDevices = devices.Count - onlineCount,
                activeAlerts
            });
        }
        catch (Exception ex)
        {
            var user = await GetRequestUserAsync(ct);
            logger.LogError(ex, "Erreur lors de la récupération des stats para o usuário {UserId}", user?.Id.ToString() ?? "Anonyme");
            return Ok(new { totalDevices = 0, onlineDevices = 0, offlineDevices = 0, activeAlerts = 0 });
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
            devEui = NormalizeDevEui(devEui);
            var user = await GetRequestUserAsync(ct);
            if (user is null) return Ok(Array.Empty<object>());

            bool hasAccess = false;
            if (HttpContext.Items.TryGetValue("TelegramId", out var tgObj) && tgObj is long telegramId && telegramId > 0)
            {
                hasAccess = await telegramUserService.HasAccessToDeviceAsync(telegramId, devEui, ct);
            }
            else
            {
                var devices = await GetDevicesForUserAsync(user, ct);
                hasAccess = devices.Any(d => d.DevEui == devEui);
            }

            if (!hasAccess)
            {
                return Ok(Array.Empty<object>());
            }

            var (startDate, endDate) = GetDateRange(period);

            // Essayer d'abord les données Uc502Wet150
            var wet150Data = await db.Uc502Wet150s
                .Where(d => d.DevEui == devEui && d.Timestamp >= startDate && d.Timestamp <= endDate)
                .OrderBy(d => d.Timestamp)
                .Select(d => new
                {
                    time = d.Timestamp,
                    temperature = d.SoilTemperature,
                    vwc = d.MineralVWC,
                    ec = d.MineralECp,
                    battery = d.Battery
                })
                .ToListAsync(ct);

            if (wet150Data.Count > 0)
                return Ok(wet150Data);

            // Sinon essayer Em300Th
            var em300Data = await db.Em300ths
                .Where(d => d.DevEui == devEui && d.Timestamp >= startDate && d.Timestamp <= endDate)
                .OrderBy(d => d.Timestamp)
                .Select(d => new
                {
                    time = d.Timestamp,
                    temperature = d.Temperature,
                    humidity = d.Humidity,
                    battery = d.Battery
                })
                .ToListAsync(ct);

            return Ok(em300Data);
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
            devEui = NormalizeDevEui(devEui);
            var telegramId = (long?)(HttpContext.Items["TelegramId"]) ?? 0;
            if (telegramId == 0 || !await telegramUserService.HasAccessToDeviceAsync(telegramId, devEui, ct))
            {
                return Ok(Array.Empty<object>());
            }

            var thresholds = await db.AlarmRules
                .Include(a => a.Device)
                .Include(a => a.TimePeriods)
                .Where(a => a.Device.DevEui == devEui)
                .Select(a => new MiniAppThresholdDto(
                    a.PropertyName,
                    (double?)a.LowValue,
                    (double?)a.HighValue,
                    a.Enabled,
                    a.UseTimePeriods,
                    a.TimePeriods.OrderBy(p => p.DisplayOrder).Select(p => new MiniAppTimePeriodDto(
                        p.Name,
                        p.StartTime.ToString(@"hh\:mm"),
                        p.EndTime.ToString(@"hh\:mm"),
                        (double?)p.LowValue,
                        (double?)p.HighValue,
                        p.IsEnabled
                    )).ToList()
                ))
                .ToListAsync(ct);

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
            devEui = NormalizeDevEui(devEui);
            var user = await GetRequestUserAsync(ct);
            if (user is null) return Unauthorized();

            bool hasAccess = false;
            var telegramIdClaim = User.FindFirstValue("telegramId") ?? User.FindFirstValue("telegram_id");
            
            if (!string.IsNullOrEmpty(telegramIdClaim) && long.TryParse(telegramIdClaim, out var telegramId) && telegramId > 0)
            {
                hasAccess = await telegramUserService.HasAccessToDeviceAsync(telegramId, devEui, ct);
            }
            else
            {
                var devices = await GetDevicesForUserAsync(user, ct);
                hasAccess = devices.Any(d => d.DevEui == devEui);
            }

            if (!hasAccess)
            {
                return Forbid();
            }

            var device = await db.Devices.FirstOrDefaultAsync(d => d.DevEui == devEui, ct);
            if (device == null)
            {
                return NotFound(new { success = false, message = "Device non trouvé" });
            }

            foreach (var threshold in request.Thresholds)
            {
                var existing = await db.AlarmRules
                    .Include(a => a.TimePeriods)
                    .FirstOrDefaultAsync(a => a.DeviceId == device.Id && a.PropertyName == threshold.SensorType, ct);

                if (existing != null)
                {
                    existing.LowValue = (float?)threshold.MinValue;
                    existing.HighValue = (float?)threshold.MaxValue;
                    existing.Enabled = threshold.IsEnabled;
                    existing.UseTimePeriods = threshold.UseTimePeriods;

                    // Gerenciar períodos
                    db.AlarmTimePeriods.RemoveRange(existing.TimePeriods);
                    if (threshold.UseTimePeriods && threshold.Periods != null)
                    {
                        existing.TimePeriods = threshold.Periods.Select((p, idx) => new AlarmTimePeriod
                        {
                            Name = p.Name,
                            StartTime = TimeSpan.Parse(p.StartTime),
                            EndTime = TimeSpan.Parse(p.EndTime),
                            LowValue = (float?)p.MinValue,
                            HighValue = (float?)p.MaxValue,
                            IsEnabled = p.IsEnabled,
                            DisplayOrder = idx + 1
                        }).ToList();
                    }
                }
                else
                {
                    var newRule = new AlarmRule
                    {
                        DeviceId = device.Id,
                        PropertyName = threshold.SensorType,
                        LowValue = (float?)threshold.MinValue,
                        HighValue = (float?)threshold.MaxValue,
                        Enabled = threshold.IsEnabled,
                        UseTimePeriods = threshold.UseTimePeriods,
                        IsAlarmActive = false,
                        ActiveThresholdType = null
                    };

                    if (threshold.UseTimePeriods && threshold.Periods != null)
                    {
                        newRule.TimePeriods = threshold.Periods.Select((p, idx) => new AlarmTimePeriod
                        {
                            Name = p.Name,
                            StartTime = TimeSpan.Parse(p.StartTime),
                            EndTime = TimeSpan.Parse(p.EndTime),
                            LowValue = (float?)p.MinValue,
                            HighValue = (float?)p.MaxValue,
                            IsEnabled = p.IsEnabled,
                            DisplayOrder = idx + 1
                        }).ToList();
                    }
                    db.AlarmRules.Add(newRule);
                }
            }

            await db.SaveChangesAsync(ct);
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

        return Ok(new
        {
            id = user.Id,
            email = user.Email,
            name = user.Nom,
            company = user.Company?.Name,
            isTelegram = User.HasClaim(c => c.Type == "telegramId")
        });
    }

    private static (DateTime Start, DateTime End) GetDateRange(string period)
    {
        var end = DateTime.UtcNow;
        var start = period switch
        {
            "24h" => end.AddHours(-24),
            "36h" => end.AddHours(-36),
            "48h" => end.AddHours(-48),
            "7d" => end.AddDays(-7),
            "30d" => end.AddDays(-30),
            _ => end.AddHours(-36)
        };
        return (start, end);
    }

    private async Task<User?> GetRequestUserAsync(CancellationToken ct)
    {
        // 1. Tentar obter via Claims (Padrão JWT ASP.NET)
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdClaim, out var userId))
        {
            return await db.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == userId, ct);
        }

        // 2. Fallback para itens (caso o middleware tenha preenchido - legibilidade)
        if (HttpContext.Items.TryGetValue("UserId", out var userObj) && userObj is int ctxUserId && ctxUserId > 0)
        {
            return await db.Users
                .Include(u => u.Company)
                .FirstOrDefaultAsync(u => u.Id == ctxUserId, ct);
        }

        return null;
    }

    private async Task<List<Device>> GetDevicesForUserAsync(User user, CancellationToken ct)
    {
        if (user.Role.Equals("root", StringComparison.OrdinalIgnoreCase))
        {
            return await db.Devices
                .Include(d => d.Company)
                .Include(d => d.ModeloNavegacao)
                .Where(d => d.ActiveInKropKontrol)
                .OrderByDescending(d => d.LastSeenAt)
                .ToListAsync(ct);
        }

        var companyIds = await GetAccessibleCompanyIdsAsync(user, ct);

        return await db.Devices
            .Include(d => d.Company)
            .Include(d => d.ModeloNavegacao)
            .Where(d => d.ActiveInKropKontrol && companyIds.Contains(d.CompanyId))
            .OrderByDescending(d => d.LastSeenAt)
            .ToListAsync(ct);
    }

    private async Task<List<int>> GetAccessibleCompanyIdsAsync(User user, CancellationToken ct)
    {
        var companyIds = new List<int> { user.CompanyId };

        if (user.AccessLevel == UserAccessLevel.CompanyAndSubsidiaries)
        {
            var subsidiaryIds = await db.Companies
                .Where(c => c.ParentCompanyId == user.CompanyId)
                .Select(c => c.Id)
                .ToListAsync(ct);

            companyIds.AddRange(subsidiaryIds);
        }

        return companyIds;
    }

    private static DateTime NormalizeToUtc(DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
            _ => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)
        };
    }

    private static string NormalizeDevEui(string devEui)
    {
        return string.IsNullOrWhiteSpace(devEui) ? devEui : devEui.Trim().ToUpperInvariant();
    }

    private static bool TryParseLastSendAt(string? value, out DateTime utcDateTime)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            if (DateTimeOffset.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var dto))
            {
                utcDateTime = dto.UtcDateTime;
                return true;
            }

            string[] formats =
            [
                "yyyy-MM-dd HH:mm:ss zzz 'GMT'",
                "yyyy-MM-dd HH:mm:ss 'GMT'zzz",
                "yyyy-MM-ddTHH:mm:sszzz",
                "yyyy-MM-ddTHH:mm:ss.fffzzz",
                "yyyy-MM-dd HH:mm:ss"
            ];

            if (DateTimeOffset.TryParseExact(value, formats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out dto))
            {
                utcDateTime = dto.UtcDateTime;
                return true;
            }
        }

        utcDateTime = default;
        return false;
    }
}

// DTOs
public record MiniAppAuthRequest(long TelegramId, string? Username, string? FirstName, string? LastName, string? InitData);
public record SupportMessageRequest(string Message, long? TelegramId, string? UserName);
public record UpdateThresholdsRequest(List<MiniAppThresholdDto> Thresholds);
public record MiniAppThresholdDto(string SensorType, double? MinValue, double? MaxValue, bool IsEnabled, bool UseTimePeriods, List<MiniAppTimePeriodDto> Periods);
public record MiniAppTimePeriodDto(string Name, string StartTime, string EndTime, double? MinValue, double? MaxValue, bool IsEnabled);
