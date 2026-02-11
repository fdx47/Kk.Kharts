using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Services.Telegram;
using Kk.Kharts.Api.Utility.Constants;
using Kk.Kharts.Shared.Constants;
using Kk.Kharts.Shared.DTOs;
using Kk.Kharts.Shared.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Kk.Kharts.Api.Controllers;

/// <summary>
/// Controller para o Bot de Debug/Interno (KropKontrolDEV).
/// Comandos administrativos: /last, /lastseen, /offline, /inactive, /stats, /createuserdemo, /help
/// Este bot é apenas para uso interno da equipa de desenvolvimento.
/// </summary>
[ApiController]
[Route("api/v1/webhooks/")]
[ApiExplorerSettings(IgnoreApi = true)]
public class DebugBotController : ControllerBase
{
    private const int DefaultTempPasswordMinutes = 20;
    private const int MinTempPasswordMinutes = 5;
    private const int MaxTempPasswordMinutes = 120;

    private enum LastSeenComparison
    {
        OlderThan,
        NewerThan
    }

    private const string StaffWebAppUrl = "https://kropkontrol.com/staff/";

    private async Task HandleStaffCommandAsync(long chatId, bool isPrivateChat, CancellationToken ct, int? topicId = null)
    {
        var kb = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                isPrivateChat
                    ? InlineKeyboardButton.WithWebApp(
                        text: "🚀 Ouvrir KropKontrol Staff",
                        webApp: new WebAppInfo { Url = StaffWebAppUrl })
                    : InlineKeyboardButton.WithUrl(
                        text: "🚀 Ouvrir KropKontrol Staff",
                        url: StaffWebAppUrl)
            }
        });

        var message = $"""
{TelegramConstants.Emojis.Rocket} <b>KropKontrol Staff</b>

Accédez au tableau de bord interne directement dans Telegram via le bouton ci-dessous.
Identifiants Root requis.
""";

        await _botClient.SendMessage(
            chatId: chatId,
            text: message,
            parseMode: ParseMode.Html,
            messageThreadId: topicId,
            replyMarkup: kb,
            cancellationToken: ct);
    }

    private async Task HandleMiniAppSetupCommandAsync(long chatId, CancellationToken ct)
    {
        try
        {
            await _telegramService.SetDebugWebAppMenuButtonAsync("KropKontrol", StaffWebAppUrl, ct);
            await SendMessageAsync(chatId,
                "✅ Le bouton de menu <b>Mini-App</b> a été configuré avec succès pour https://kropkontrol.com/staff/.",
                ct,
                null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la configuration du bouton Mini-App");
            await SendMessageAsync(chatId,
                "❌ Impossible de configurer le bouton Mini-App. Consultez les logs pour plus de détails.",
                ct,
                null);
        }
    }


    private readonly IDebugBotDataService _dataService;
    private readonly IUserService _userService;
    private readonly TelegramOptions _options;
    private readonly TelegramBotClient _botClient;
    private readonly ILogger<DebugBotController> _logger;
    private readonly string _logsDirectory;
    private readonly IKkTimeZoneService _tz;
    private readonly ITelegramService _telegramService;
    private readonly ITelegramUserService _telegramUserService;
    private readonly ITemporaryAccessTokenService _temporaryAccessTokenService;

    private sealed record DeviceTimestamp(Device Device, DateTime? LastTimestamp);

    public DebugBotController(
        IDebugBotDataService dataService,
        IUserService userService,
        IOptions<TelegramOptions> options,
        ILogger<DebugBotController> logger,
        IKkTimeZoneService tz,
        ITelegramService telegramService,
        ITelegramUserService telegramUserService,
        ITemporaryAccessTokenService temporaryAccessTokenService)
    {
        _dataService = dataService;
        _userService = userService;
        _options = options.Value;
        _botClient = new TelegramBotClient(_options.DebugBotToken);
        _logger = logger;
        _logsDirectory = Path.Combine(AppContext.BaseDirectory, GlobalConstants.LogsDirectoryName);
        _tz = tz;
        _telegramService = telegramService;
        _telegramUserService = telegramUserService;
        _temporaryAccessTokenService = temporaryAccessTokenService;
    }

    /// <summary>
    /// Webhook endpoint para o bot de debug (interno).
    /// URL: https://kropkontrol.premiumasp.net/api/v1/webhooks/debug/{secret}
    /// </summary>
    [HttpPost("debug/{secret}")]
    public async Task<IActionResult> PostDebug(string secret, [FromBody] Telegram.Bot.Types.Update update, CancellationToken ct)
    {
        // Validar secret
        if (secret != _options.DebugWebhookSecret)
        {
            _logger.LogWarning("Debug bot : tentative avec un secret de webhook invalide");
            return Unauthorized("Accès non autorisé");
        }

        // Processar apenas mensagens de texto
        if (update.Message?.Text is null)
            return Ok();

        var message = update.Message;
        var chatId = message.Chat.Id;
        var isPrivateChat = message.Chat.Type == ChatType.Private;
        var text = message.Text.Trim();

        // Capturar thread ID para grupos com tópicos (para logging)
        var messageThreadId = message.MessageThreadId;
        if (messageThreadId.HasValue && _logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Debug bot : message_thread_id détecté : {ThreadId}", messageThreadId.Value);
        }

        // Remover @BotName do comando
        if (text.StartsWith('/'))
        {
            var atIndex = text.IndexOf('@');
            if (atIndex != -1)
                text = text[..atIndex];
        }

        try
        {
            if (text.StartsWith("/start", StringComparison.OrdinalIgnoreCase))
            {
                await HandleHelpCommandAsync(chatId, ct);
                return Ok();
            }

            // Processar comandos
            var topicId = isPrivateChat ? (int?)null : _options.CmdsTopicId;

            if (text.StartsWith("/last", StringComparison.OrdinalIgnoreCase) && !text.StartsWith("/lastseen"))
                await HandleLastCommandAsync(chatId, text, ct, topicId);
            else if (text.StartsWith("/lastseen", StringComparison.OrdinalIgnoreCase))
                await HandleLastSeenCommandAsync(chatId, text, ct, topicId);
            else if (text.StartsWith("/offline", StringComparison.OrdinalIgnoreCase))
                await HandleOfflineCommandAsync(chatId, text, ct, topicId);
            else if (text.Equals("/inactive", StringComparison.OrdinalIgnoreCase))
                await HandleInactiveCommandAsync(chatId, ct, topicId);
            else if (text.StartsWith("/createuserdemo", StringComparison.OrdinalIgnoreCase))
                await HandleCreateUserDemoCommandAsync(chatId, text, ct);
            else if (text.Equals("/help", StringComparison.OrdinalIgnoreCase))
                await HandleHelpCommandAsync(chatId, ct, topicId);
            else if (text.StartsWith("/stats", StringComparison.OrdinalIgnoreCase))
                await HandleStatsCommandAsync(chatId, text, ct, topicId);
            else if (text.Equals("/staff", StringComparison.OrdinalIgnoreCase))
                await HandleStaffCommandAsync(chatId, isPrivateChat, ct, topicId);
            else if (text.StartsWith("/reply", StringComparison.OrdinalIgnoreCase))
                await HandleReplyCommandAsync(chatId, text, ct);
            else if (text.StartsWith("/generatepassword", StringComparison.OrdinalIgnoreCase))
                await HandleGeneratePasswordCommandAsync(message, ct);
            else
                return Ok("Commande non reconnue par le bot");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur du bot de debug lors du traitement de la commande : {Command}", text);
            await SendMessageAsync(chatId, $"❌ Erreur: {ex.Message}", ct);
        }

        return Ok();
    }


    private async Task HandleHelpCommandAsync(long chatId, CancellationToken ct, int? topicId = null)
    {
        var helpMessage = $"""
            🤖 <b>Bot Debug KropKontrol</b>

            Utilisez ces commandes pour administrer l'environnement de test:
            • <code>/last</code> - Dernières données reçues (Top 10 par défaut, <code>/last all</code> pour tout afficher)
            • <code>/lastseen [&lt;durée] [&lt;fenêtre]</code> - Capteurs silencieux/récents
              ↳ <code>&lt;</code> = capteurs silencieux depuis plus que la durée (ex.: <code>/lastseen &lt;30</code> ➜ >30 min sans données)
              ↳ <code>&gt;</code> = capteurs récents (ex.: <code>/lastseen &gt;5m</code> ➜ envois dans les 5 dernières minutes)
            • <code>/offline [minutes]</code> - Capteurs hors ligne
            • <code>/inactive</code> - Capteurs marqués inactifs
            • <code>/stats [ddMMyy]</code> - Statistiques système et kklogs
            • <code>/createuserdemo Nom Prénom Mdp [jours]</code> - Créer un compte démo
            • <code>/reply chatId message</code> - Répondre à un utilisateur
            • <code>/generatepassword [minutes]</code> - Token d'accès temporaire
            • <code>/staff</code> - Ouvrir la page Staff (https://kropkontrol.com/staff/)

            """;

        await _botClient.SendMessage(
            chatId: chatId,
            text: helpMessage,
            parseMode: ParseMode.Html,
            messageThreadId: topicId,
            replyMarkup: BuildPersistentKeyboard(),
            cancellationToken: ct);
    }


    // /last
    private async Task HandleLastCommandAsync(long chatId, string commandText, CancellationToken ct, int? topicId = null)
    {
        var devices = (await _dataService.GetActiveDevicesWithCompanyAsync(ct))
            .OrderBy(d => d.LastSeenAt)
            .ToList();

        var args = commandText.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var showAll = args.Length > 1 && args[1].Equals("all", StringComparison.OrdinalIgnoreCase);

        IEnumerable<Device> devicesToShow = devices;
        if (!showAll)
        {
            devicesToShow = devices
                .OrderByDescending(d => d.LastSeenAt)
                .Take(10);
        }

        var headerSuffix = showAll ? "<i>(Du plus ancien au plus récent)</i>" : "<i>(Top 10 – du plus récent au plus ancien)</i>";
        var orderedDevices = showAll
            ? devicesToShow.OrderBy(d => d.LastSeenAt)
            : devicesToShow;

        var sb = new StringBuilder();
        sb.AppendLine("📡 <b>Dernières données reçues des capteurs :</b>");
        sb.AppendLine($"{headerSuffix}\n");

        foreach (var device in orderedDevices)
        {
            sb.AppendLine($"📍 <b>{device.Name}</b> ({device.Company?.Name ?? "N/A"})");
            sb.AppendLine($"    Dernier envoi : <i>{device.LastSendAt.ToHumanizeLastSentAt()}</i>\n");
        }

        await SendMessageAsync(chatId, sb.ToString(), ct, topicId);
    }


    private async Task HandleGeneratePasswordCommandAsync(Message message, CancellationToken ct)
    {
        var chatId = message.Chat.Id;
        var threadId = message.MessageThreadId ?? _options.CmdsTopicId;
        var telegramUserId = message.From?.Id ?? 0;

        if (telegramUserId == 0)
        {
            await SendMessageAsync(chatId, "❌ Identité Telegram introuvable.", ct, threadId);
            return;
        }

        var issuer = await _telegramUserService.GetUserByTelegramIdAsync(telegramUserId, ct);
        if (issuer is null)
        {
            var notLinked = $"""
                {TelegramConstants.Emojis.Warning} <b>Compte non lié</b>

                Utilisez le bot client pour lier votre compte Root:
                <code>/link email motdepasse</code>
                """;
            await SendMessageAsync(chatId, notLinked, ct, threadId);
            return;
        }

        if (!string.Equals(issuer.Role, Roles.Root, StringComparison.OrdinalIgnoreCase))
        {
            var forbidden = $"""
                {TelegramConstants.Emojis.Lock} <b>Accès refusé</b>

                Cette commande est réservée aux administrateurs Root.
                Utilisateur actuel: <b>{issuer.Nom}</b>
                """;
            await SendMessageAsync(chatId, forbidden, ct, threadId);
            return;
        }

        var requestedMinutes = ParseMinutesArgument(message.Text);
        var lifetime = TimeSpan.FromMinutes(requestedMinutes);
        var (token, plaintext) = await _temporaryAccessTokenService.GenerateAsync(issuer, lifetime, ct);

        var response = $"""
            {TelegramConstants.Emojis.Key} <b>Mot de passe temporaire d’assistance</b>

            • <b>Token:</b> <code>{plaintext}</code>
            • <b>Valide pendant:</b> {requestedMinutes} min (jusqu'à {token.ExpiresAtUtc:dd/MM HH:mm} UTC)
            • <b>Usage:</b> Email client non Root + ce token comme mot de passe
            • <b>Notes:</b> usage unique, révocation automatique

            {TelegramConstants.Emojis.Warning} Supprimez ce message après l'avoir copié.
            """;

        await SendMessageAsync(chatId, response, ct, threadId);

        var auditMessage = $"""
            {TelegramConstants.Emojis.Key} Token d’assistance généré
            • Par: {issuer.Email} (ID {issuer.Id})
            • Expire: {token.ExpiresAtUtc:dd/MM HH:mm} UTC
            • TokenId: {token.Id}
            """;

        await _telegramService.SendToCmdsTopicAsync(auditMessage, cancellationToken: ct);
    }


    private async Task HandleLastSeenCommandAsync(long chatId, string text, CancellationToken ct, int? topicId = null)
    {
        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!TryParseLastSeenArgs(parts, out var threshold, out var maxAge, out var comparison))
        {
            await SendMessageAsync(chatId,
                "❌ Intervalle invalide. Utilisez par exemple /lastseen 30m, /lastseen &gt;45s 5m ou /lastseen &lt;30.",
                ct,
                topicId);
            return;
        }

        var now = DateTime.UtcNow;
        var cutoff = now - threshold;
        var thresholdText = FormatDurationDescription(threshold);
        var windowText = FormatDurationDescription(maxAge);

        var devices = await _dataService.GetActiveDevicesWithCompanyAsync(ct);

        var deviceTimestamps = devices
            .Select(d => new DeviceTimestamp(d, GetLastTelemetryTimestamp(d)))
            .Where(x => x.LastTimestamp.HasValue)
            .ToList();

        List<DeviceTimestamp> filtered = comparison switch
        {
            LastSeenComparison.NewerThan => [.. deviceTimestamps
                .Where(x => x.LastTimestamp!.Value >= cutoff)
                .OrderByDescending(x => x.LastTimestamp)],
            _ => [.. deviceTimestamps
                .Where(x => x.LastTimestamp!.Value < cutoff && x.LastTimestamp!.Value >= now - maxAge)
                .OrderBy(x => x.LastTimestamp)],
        };

        if (filtered.Count == 0)
        {
            var message = comparison == LastSeenComparison.NewerThan
                ? $"⚠️ Aucun capteur n'a envoyé de données dans les {thresholdText}."
                : $"✅ Tous les capteurs ont envoyé des données au cours des {thresholdText}.";

            await SendMessageAsync(chatId, message, ct, topicId);
            return;
        }

        var sb = new StringBuilder();
        if (comparison == LastSeenComparison.NewerThan)
        {
            sb.AppendLine($"🟢 <b>Capteurs ayant envoyé des données au cours des {thresholdText} :</b>");
            sb.AppendLine("<i>(Du plus récent au plus ancien)</i>\n");
        }
        else
        {
            sb.AppendLine($"🚫 <b>Capteurs n'ayant pas envoyé de données depuis plus de {thresholdText} (période max {windowText}) :</b>");
            sb.AppendLine("<i>(Du plus ancien au plus récent)</i>\n");
        }

        foreach (var entry in filtered)
        {
            var device = entry.Device;
            sb.AppendLine($"- <b>{device.Name}</b> ({device.Company?.Name ?? "N/A"})");
            sb.AppendLine($"  DevEui: {device.DevEui}");
            sb.AppendLine($"  ⚡️ Dernier envoi : <i>{device.LastSendAt.ToHumanizeLastSentAt()}</i>\n");
        }

        await SendMessageAsync(chatId, sb.ToString(), ct, topicId);
    }


    // /offline
    private async Task HandleOfflineCommandAsync(long chatId, string text, CancellationToken ct, int? topicId = null)
    {
        int minutes = 15;
        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 1 && int.TryParse(parts[1], out var parsedMinutes))
            minutes = parsedMinutes;

        var cutoff = DateTime.UtcNow.AddMinutes(-minutes);

        var devices = await _dataService.GetActiveDevicesWithCompanyAsync(ct);

        var filtered = devices
            .Where(d => DateTime.TryParse(d.LastSendAt, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var sendAt)
                && sendAt < cutoff)
            .OrderBy(d => d.LastSeenAt)
            .ToList();

        if (filtered.Count == 0)
        {
            await SendMessageAsync(chatId, $"✅ Tous les capteurs sont en ligne depuis les {minutes} dernières minutes.", ct, topicId);
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine($"⚠️ <b>Capteurs hors ligne depuis plus de {minutes} minutes :</b>");
        sb.AppendLine("<i>(Du plus ancien au plus récent)</i>\n");

        foreach (var device in filtered)
        {
            sb.AppendLine($"- <b>{device.Name}</b> ({device.Company?.Name ?? "N/A"})");
            sb.AppendLine($"  Localisation : {device.InstallationLocation}");
            sb.AppendLine($"  DevEui: {device.DevEui}");
            sb.AppendLine($"  ⚡️ Dernier envoi : <i>{device.LastSendAt.ToHumanizeLastSentAt()}</i>\n");
        }

        // Resumo por empresa
        var offlineByCompany = filtered.GroupBy(d => d.Company?.Name ?? "N/A")
            .Select(g => new { Company = g.Key, Count = g.Count() });
        var totalByCompany = devices.GroupBy(d => d.Company?.Name ?? "N/A")
            .Select(g => new { Company = g.Key, Count = g.Count() });

        sb.AppendLine("\n<b>Résumé par société :</b>");
        foreach (var offline in offlineByCompany)
        {
            var total = totalByCompany.FirstOrDefault(t => t.Company == offline.Company)?.Count ?? 0;
            sb.AppendLine($"- {offline.Company} : {offline.Count} capteurs hors ligne sur {total}");
        }

        await SendMessageAsync(chatId, sb.ToString(), ct, topicId);
    }


    // /inactive
    private async Task HandleInactiveCommandAsync(long chatId, CancellationToken ct, int? topicId = null)
    {
        var inactiveDevices = await _dataService.GetInactiveDevicesWithCompanyAsync(ct);

        if (inactiveDevices.Count == 0)
        {
            await SendMessageAsync(chatId, "✅ All devices are currently active.", ct, topicId);
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("🔕 <b>Inactive devices:</b>\n");

        foreach (var device in inactiveDevices)
        {
            sb.AppendLine($"- <b>{device.Name}</b> ({device.Company?.Name ?? "N/A"})");
            sb.AppendLine($"  DevEui: {device.DevEui}");
            sb.AppendLine($"  ⚡️ Last sent: <i>{device.LastSendAt.ToHumanizeLastSentAt()}</i>\n");
        }

        await SendMessageAsync(chatId, sb.ToString(), ct, topicId);
    }

    // /createuserdemo
    private async Task HandleCreateUserDemoCommandAsync(long chatId, string text, CancellationToken ct)
    {
        var parts = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 4 || parts.Length > 5)
        {
            await SendMessageAsync(chatId, "❌ Format incorrect.\n\nUsage: /createuserdemo FirstName LastName Password [validityDays]", ct);
            return;
        }

        var firstName = parts[1];
        var lastName = parts[2];
        var password = parts[3];
        int validadeDias = 1;

        if (parts.Length == 5)
        {
            if (!int.TryParse(parts[4], out validadeDias))
            {
                await SendMessageAsync(chatId, "❌ Le nombre de jours doit être un nombre entier.", ct);
                return;
            }
            validadeDias = Math.Clamp(validadeDias, 1, 30);
        }

        var dataValidade = DateTime.Now.AddDays(validadeDias);
        var dateValidadeString = dataValidade.ToString("ddMMyy");

        var dto = new UserCreateDTO
        {
            Nom = $"{firstName} {lastName}",
            Email = $"{firstName.ToLower()}_{dateValidadeString}@kkdemo.com",
            Password = password,
            Role = "Demo",
            CompanyId = GlobalConstants.KropKontrolCompanyId
        };

        var result = await _userService.CreateUserAsync(dto);

        if (!result.Success)
        {
            await SendMessageAsync(chatId, $"❌ Erreur: {result.ErrorMessage}", ct);
        }
        else
        {
            await SendMessageAsync(chatId, $"""
                ✅ Utilisateur créé avec succès!

                👤 Nom: {result.User.Nom}
                📧 Email: {result.User.Email}
                🔑 Password: {dto.Password}
                🛡️ Rôle: {result.User.Role}
                🗓️ Valable jusqu'au {dataValidade:dd/MM/yyyy} à 24h00 (UTC).
                """, ct);
        }
    }


    // /stats
    private async Task HandleStatsCommandAsync(long chatId, string text, CancellationToken ct, int? topicId = null)
    {
        DateOnly? requestedDate = null;
        var partsInput = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (partsInput.Length > 1 && TryParseStatsDate(partsInput[1], out var parsedDate))
        {
            requestedDate = parsedDate;
        }

        var now = DateTime.UtcNow;
        var stats = await _dataService.GetSystemStatsAsync(ct);
        var supportTickets24h = 0;

        var localNow = ConvertToParisTime(now);

        var sb = new StringBuilder();
        sb.AppendLine("📊 <b>KropKontrol - Usage stats (debug)</b>");
        sb.AppendLine($"🕒 Généré: <i>{localNow:dd/MM/yyyy HH:mm} (heure de Paris)</i>");
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine("📡 <b>Capteurs</b>");
        sb.AppendLine($"• Total actifs: <b>{stats.TotalActiveDevices}</b>");
        sb.AppendLine($"• En ligne (1h): <b>{stats.OnlineDevices}</b>");
        sb.AppendLine($"• Hors ligne estimés: <b>{stats.OfflineDevices}</b>");
        sb.AppendLine($"• Inactifs (flag): <b>{stats.InactiveDevices}</b>");
        sb.AppendLine($"• Batterie ≤20%: <b>{stats.LowBatteryDevices}</b>");
        sb.AppendLine();
        sb.AppendLine("🔔 <b>Alertes</b>");
        sb.AppendLine($"• Règles actives: <b>{stats.TotalAlarmRules}</b>");
        sb.AppendLine($"• Alertes déclenchées: <b>{stats.ActiveAlerts}</b>");
        sb.AppendLine();
        sb.AppendLine("👥 <b>Utilisateurs</b>");
        sb.AppendLine($"• Total: <b>{stats.TotalUsers}</b>");
        sb.AppendLine($"• Telegram liés: <b>{stats.LinkedTelegramUsers}</b>");
        var ratio = stats.TotalUsers > 0 ? stats.LinkedTelegramUsers * 100.0 / stats.TotalUsers : 0;
        sb.AppendLine($"• Taux de liaison: <b>{ratio:F1}%</b>");
        sb.AppendLine();
        sb.AppendLine("🗂️ <b>Dashboard & Support</b>");
        sb.AppendLine($"• Dashboards sauvegardés: <b>{stats.Dashboards}</b>");
        sb.AppendLine($"• Tickets support 24h: <b>{supportTickets24h}</b>");

        if (stats.TopCompanies.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("🏢 <b>Top 5 sociétés (capteurs actifs)</b>");
            foreach (var entry in stats.TopCompanies)
            {
                sb.AppendLine($"• {entry.Company}: <b>{entry.Count}</b>");
            }
        }

        if (requestedDate.HasValue)
        {
            var logPath = GetLogFilePath(requestedDate.Value);
            if (System.IO.File.Exists(logPath))
            {
                await using var logStream = System.IO.File.OpenRead(logPath);
                await _telegramService.SendDocumentToCmdsTopicAsync(
                    logStream,
                    Path.GetFileName(logPath),
                    caption: $"kklogs {requestedDate:dd/MM/yy}",
                    cancellationToken: ct);
            }
            else
            {
                await _telegramService.SendToCmdsTopicAsync(
                    $"⚠️ Aucun fichier kklogs trouvé pour {requestedDate:dd/MM/yy}.",
                    cancellationToken: ct);
            }

            return;
        }

        var userActivity = await LoadUserLogStatsAsync(
            daysToInspect: 2,
            ct,
            specificDate: null);

        sb.AppendLine();
        var activityDay = DateOnly.FromDateTime(DateTime.UtcNow);
        sb.AppendLine("🧑‍💻 <b>Activité utilisateurs (kklogs.txt)</b>");
        sb.AppendLine($"↳ {activityDay:dd/MM/yy}");

        var todayActivity = userActivity
            .Where(e => e.Value.DailyStats.ContainsKey(activityDay))
            .OrderByDescending(e => e.Value.DailyStats[activityDay].TotalRequests)
            .ThenByDescending(e => e.Value.LastSeenUtc ?? DateTime.MinValue)
            .Take(8)
            .ToList();

        if (todayActivity.Count == 0)
        {
            sb.AppendLine("• Aucun log disponible pour cette date.");
        }
        else
        {
            foreach (var entry in todayActivity)
            {
                sb.AppendLine($"• <b>{entry.Key}</b>");

                var day = entry.Value.DailyStats[activityDay];
                var methods = day.Methods
                    .OrderByDescending(m => m.Value)
                    .Select(m => $"{m.Key}: {m.Value}")
                    .ToArray();

                var firstSeen = FormatParisTime(day.FirstSeenUtc);
                var lastSeen = FormatParisTime(day.LastSeenUtc);
                var dayLabel = activityDay.ToString("dd/MM");

                sb.AppendLine($"   ↳ {dayLabel}: <b>{day.TotalRequests}</b> req ({string.Join(", ", methods)})");
                sb.AppendLine($"      ↳ Première: {firstSeen}");
                sb.AppendLine($"      ↳ Dernière: {lastSeen}");

                sb.AppendLine();
            }
        }

        await SendMessageAsync(chatId, sb.ToString(), ct, topicId);
    }


    private async Task<Dictionary<string, UserLogStats>> LoadUserLogStatsAsync(int daysToInspect, CancellationToken ct, DateOnly? specificDate = null)
    {
        var result = new Dictionary<string, UserLogStats>(StringComparer.OrdinalIgnoreCase);

        if (!Directory.Exists(_logsDirectory) || daysToInspect <= 0)
            return result;

        List<DateOnly> datesToInspect = specificDate.HasValue
            ? [specificDate.Value]
            : [.. Enumerable.Range(0, daysToInspect)
                .Select(offset => DateOnly.FromDateTime(DateTime.UtcNow.Date.AddDays(-offset)))];

        foreach (var date in datesToInspect)
        {
            ct.ThrowIfCancellationRequested();

            var filePath = GetLogFilePath(date);

            if (!System.IO.File.Exists(filePath))
                continue;

            var lines = await System.IO.File.ReadAllLinesAsync(filePath, ct);

            string? currentUser = null;
            string? currentMethod = null;

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();

                if (line.StartsWith("👤", StringComparison.Ordinal))
                {
                    currentUser = ExtractLineValue(line);
                }
                else if (line.StartsWith("📥", StringComparison.Ordinal))
                {
                    currentMethod = ExtractLineValue(line).ToUpperInvariant();
                }
                else if (line.StartsWith("🕒", StringComparison.Ordinal))
                {
                    if (!DateTime.TryParseExact(
                            ExtractLineValue(line).Replace("UTC", "", StringComparison.OrdinalIgnoreCase).Trim(),
                            "dd/MM/yy HH:mm:ss",
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                            out var timestamp))
                    {
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(currentUser) || string.IsNullOrWhiteSpace(currentMethod))
                        continue;

                    if (!result.TryGetValue(currentUser, out var stats))
                    {
                        stats = new UserLogStats();
                        result[currentUser] = stats;
                    }

                    stats.TotalRequests++;
                    stats.Methods[currentMethod] = stats.Methods.GetValueOrDefault(currentMethod) + 1;
                    stats.FirstSeenUtc = stats.FirstSeenUtc is null || timestamp < stats.FirstSeenUtc ? timestamp : stats.FirstSeenUtc;
                    stats.LastSeenUtc = stats.LastSeenUtc is null || timestamp > stats.LastSeenUtc ? timestamp : stats.LastSeenUtc;

                    // Populate DailyStats for the report
                    var logDate = DateOnly.FromDateTime(timestamp);
                    if (!stats.DailyStats.TryGetValue(logDate, out var dailyStats))
                    {
                        dailyStats = new DailyStats();
                        stats.DailyStats[logDate] = dailyStats;
                    }

                    dailyStats.TotalRequests++;
                    dailyStats.Methods[currentMethod] = dailyStats.Methods.GetValueOrDefault(currentMethod) + 1;
                    dailyStats.FirstSeenUtc = dailyStats.FirstSeenUtc is null || timestamp < dailyStats.FirstSeenUtc ? timestamp : dailyStats.FirstSeenUtc;
                    dailyStats.LastSeenUtc = dailyStats.LastSeenUtc is null || timestamp > dailyStats.LastSeenUtc ? timestamp : dailyStats.LastSeenUtc;

                }
            }
        }

        return result;
    }

    private static bool TryParseStatsDate(string input, out DateOnly date)
    {
        var acceptedFormats = new[] { "ddMMyy", "dd/MM/yy", "dd-MM-yy", "dd-MM-yyyy", "yyyy-MM-dd", "yyyy/MM/dd" };
        foreach (var format in acceptedFormats)
        {
            if (DateOnly.TryParseExact(input, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return true;
            }
        }

        date = default;
        return false;
    }

    private string GetLogFilePath(DateOnly date) => Path.Combine(_logsDirectory, date.ToString("ddMMyy", CultureInfo.InvariantCulture) + ".txt");


    private static string ExtractLineValue(string line)
    {
        var colonIndex = line.IndexOf(':');
        return colonIndex == -1 ? line : line[(colonIndex + 1)..].Trim();
    }


    private DateTime ConvertToParisTime(DateTime utcTime) => _tz.ConvertToParisTime(utcTime);

    // Overload auxiliar para chamadas com DateTime não-nullable
    private string FormatParisTime(DateTime utcTime) => FormatParisTime((DateTime?)utcTime);

    private string FormatParisTime(DateTime? utcTime)
    {
        if (utcTime is null)
            return "—";

        var local = ConvertToParisTime(utcTime.Value);
        return local.ToString("dd/MM HH:mm", CultureInfo.InvariantCulture) + " (Paris)";
    }

    private static DateTime? GetLastTelemetryTimestamp(Device device)
    {
        if (device.LastSeenAt != default)
        {
            return device.LastSeenAt;
        }

        if (DateTime.TryParse(device.LastSendAt, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var parsed))
        {
            return parsed;
        }

        return null;
    }

    private static bool TryParseLastSeenArgs(string[] parts, out TimeSpan threshold, out TimeSpan maxAge, out LastSeenComparison comparison)
    {
        const int defaultSeconds = 20 * 60;
        threshold = TimeSpan.FromSeconds(defaultSeconds);
        maxAge = TimeSpan.FromMinutes(60);
        comparison = LastSeenComparison.OlderThan;

        if (parts.Length <= 1)
        {
            return true;
        }

        var first = parts[1];
        var remaining = first;

        var windowIndex = 2;

        if (first.StartsWith('<'))
        {
            comparison = LastSeenComparison.OlderThan;
            remaining = first[1..];
            if (string.IsNullOrWhiteSpace(remaining) && parts.Length > 2)
            {
                remaining = parts[2];
                windowIndex = 3;
            }
        }
        else if (first.StartsWith('>'))
        {
            comparison = LastSeenComparison.NewerThan;
            remaining = first[1..];
            if (string.IsNullOrWhiteSpace(remaining) && parts.Length > 2)
            {
                remaining = parts[2];
                windowIndex = 3;
            }
        }
        else
        {
            windowIndex = 2;
        }

        if (!TryParseDurationToken(remaining, out threshold))
        {
            return false;
        }

        if (comparison == LastSeenComparison.NewerThan)
        {
            maxAge = TimeSpan.FromDays(365);
            return true;
        }

        if (parts.Length > windowIndex)
        {
            if (!TryParseDurationToken(parts[windowIndex], out maxAge))
            {
                return false;
            }
        }

        return true;
    }

    private static bool TryParseDurationToken(string raw, out TimeSpan result)
    {
        result = TimeSpan.Zero;
        if (string.IsNullOrWhiteSpace(raw))
        {
            return false;
        }

        raw = raw.Trim().ToLowerInvariant();
        double multiplier = 60;
        var unit = raw[^1];
        var numericSpan = raw;

        switch (unit)
        {
            case 's':
                numericSpan = raw[..^1];
                multiplier = 1;
                break;
            case 'm':
                numericSpan = raw[..^1];
                multiplier = 60;
                break;
            case 'h':
                numericSpan = raw[..^1];
                multiplier = 3600;
                break;
            default:
                // Sem sufixo: interpretamos como minutos para alinhar com /lastseen <30 do help.
                break;
        }

        if (!double.TryParse(numericSpan, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            return false;
        }

        if (value <= 0)
        {
            return false;
        }

        result = TimeSpan.FromSeconds(value * multiplier);

        return true;
    }

    private static string FormatDurationDescription(TimeSpan duration)
    {
        var seconds = (int)duration.TotalSeconds;
        var parts = new List<string> { $"{seconds} secondes" };

        if (seconds % 60 == 0)
        {
            var minutes = seconds / 60;
            parts.Add($"{minutes} min");

            if (minutes % 60 == 0)
            {
                var hours = minutes / 60;
                parts.Add($"{hours} h");
            }
        }

        return string.Join(" / ", parts);
    }


    /// <summary>
    /// Répondre à un utilisateur depuis le canal de debug.
    /// Usage: /reply {chatId} message
    /// </summary>
    private async Task HandleReplyCommandAsync(long adminChatId, string text, CancellationToken ct)
    {
        // Format: /reply 123456789 Votre message ici
        var parts = text.Split(' ', 3, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 3)
        {
            await SendMessageAsync(adminChatId, """
                ❓ <b>Usage:</b>
                <code>/reply {chatId} votre message</code>
                
                <i>Exemple:</i>
                <code>/reply 123456789 Bonjour, voici la solution...</code>
                """, ct);
            return;
        }

        if (!long.TryParse(parts[1], out var targetChatId))
        {
            await SendMessageAsync(adminChatId, "❌ Chat ID invalide. Doit être un nombre.", ct);
            return;
        }

        var replyMessage = parts[2];

        try
        {
            // Confirmer dans le topic Support
            var confirmMsg = $"✅ <b>Réponse envoyée</b>\n\n🆔 Chat ID: <code>{targetChatId}</code>\n💬 Message: {replyMessage}";
            await SendToSupportTopicAsync(confirmMsg, ct);
        }
        catch (Exception ex)
        {
            // Erreur dans le topic Support
            var errorMsg = $"❌ <b>Erreur d'envoi</b>\n\n🆔 Chat ID: <code>{targetChatId}</code>\n⚠️ Erreur: {ex.Message}";
            await SendToSupportTopicAsync(errorMsg, ct);
        }
    }


    private async Task SendToSupportTopicAsync(string text, CancellationToken ct)
    {
        if (_options.SupportTopicId > 0)
        {
            await _botClient.SendMessage(
                chatId: _options.DebugChatId,
                text: text,
                parseMode: ParseMode.Html,
                messageThreadId: _options.SupportTopicId,
                cancellationToken: ct);
        }
    }


    private async Task SendMessageAsync(long chatId, string text, CancellationToken ct, int? topicId = null)
    {
        var resolvedTopicId = ShouldForceTopic(chatId, topicId)
            ? (int?)(topicId ?? _options.CmdsTopicId)
            : null;

        async Task sendChunk(string chunk) =>
            await _botClient.SendMessage(chatId,
                chunk,
                parseMode: ParseMode.Html,
                messageThreadId: resolvedTopicId,
                cancellationToken: ct);

        if (text.Length > _options.MaxMessageLength)
        {
            foreach (var chunk in SplitMessage(text, _options.MaxMessageLength))
                await sendChunk(chunk);
        }
        else
        {
            await sendChunk(text);
        }
    }

    private bool ShouldForceTopic(long chatId, int? topicId)
    {
        if (!long.TryParse(_options.DebugChatId, out var configuredChatId))
        {
            return false;
        }

        if (chatId != configuredChatId)
        {
            return false;
        }

        return (topicId ?? _options.CmdsTopicId) > 0;
    }


    private static ReplyKeyboardMarkup BuildPersistentKeyboard()
    {
        return new ReplyKeyboardMarkup(
        [
            [new KeyboardButton("/last"), new KeyboardButton("/lastseen 20"), new KeyboardButton("/offline 15")],
            [new KeyboardButton("/inactive"), new KeyboardButton("/createuserdemo"), new KeyboardButton("/stats")],
            [new KeyboardButton("/reply"), new KeyboardButton("/generatepassword")],
            [new KeyboardButton("/help")]
        ])
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = false,
            InputFieldPlaceholder = "Choisissez l'action debug"
        };
    }


    private static List<string> SplitMessage(string text, int maxLength)
    {
        var chunks = new List<string>();
        var lines = text.Split('\n');
        var current = new StringBuilder();

        foreach (var line in lines)
        {
            if (current.Length + line.Length + 1 > maxLength)
            {
                chunks.Add(current.ToString());
                current.Clear();
            }
            current.AppendLine(line);
        }

        if (current.Length > 0)
            chunks.Add(current.ToString());

        return chunks;
    }


    private static int ParseMinutesArgument(string? text)
    {
        var parts = text?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];
        if (parts.Length < 2)
        {
            return DefaultTempPasswordMinutes;
        }

        if (int.TryParse(parts[1], out var minutes))
        {
            return Math.Clamp(minutes, MinTempPasswordMinutes, MaxTempPasswordMinutes);
        }

        return DefaultTempPasswordMinutes;
    }




    //=======================================================================
    //     Classes auxiliares para estatísticas de logs de utilizadores
    //=======================================================================

    private sealed class UserLogStats
    {
        public int TotalRequests { get; set; }
        public Dictionary<string, int> Methods { get; } = new(StringComparer.OrdinalIgnoreCase);
        public DateTime? FirstSeenUtc { get; set; }
        public DateTime? LastSeenUtc { get; set; }
        public Dictionary<DateOnly, DailyStats> DailyStats { get; } = [];
    }

    private sealed class DailyStats
    {
        public int TotalRequests { get; set; }
        public Dictionary<string, int> Methods { get; } = new(StringComparer.OrdinalIgnoreCase);
        public DateTime? FirstSeenUtc { get; set; }
        public DateTime? LastSeenUtc { get; set; }
    }

}
