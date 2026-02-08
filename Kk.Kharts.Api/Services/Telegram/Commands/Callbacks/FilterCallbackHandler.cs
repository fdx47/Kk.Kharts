using Kk.Kharts.Api.Data;
using Kk.Kharts.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Kk.Kharts.Api.Services.Telegram.Commands.Callbacks;

/// <summary>
/// Handler pour les callbacks de filtrage (offline, lowbattery, etc.).
/// Format: filter:{type}:{page}
/// </summary>
public sealed class FilterCallbackHandler(
    IServiceScopeFactory scopeFactory,
    ITelegramService telegram) : ITelegramCallbackHandler
{
    public string CallbackPrefix => "filter:";

    private const int DevicesPerPage = 8;

    public async Task HandleAsync(CallbackQuery callback, CancellationToken ct = default)
    {
        var data = callback.Data ?? string.Empty;
        var withoutPrefix = data.Replace("filter:", "");
        
        // Format: filter:offline ou filter:offline:2
        var parts = withoutPrefix.Split(':');
        var filterType = parts[0];
        var page = parts.Length > 1 && int.TryParse(parts[1], out var p) ? p : 1;

        var chatId = callback.Message?.Chat.Id ?? 0;
        var messageId = callback.Message?.MessageId ?? 0;

        await telegram.AnswerCallbackQueryAsync(callback.Id, "Chargement...", ct: ct);

        switch (filterType)
        {
            case "offline":
                await ShowOfflineDevicesAsync(chatId, messageId, page, ct);
                break;

            case "lowbattery":
                await ShowLowBatteryDevicesAsync(chatId, messageId, page, ct);
                break;

            default:
                await telegram.AnswerCallbackQueryAsync(callback.Id, "⚠️ Filtre non reconnu", true, ct: ct);
                break;
        }
    }

    private async Task ShowOfflineDevicesAsync(long chatId, int messageId, int page, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var devices = await db.Devices
            .Include(d => d.Company)
            .Where(d => d.ActiveInKropKontrol && d.HasCommunicationAlarm)
            .OrderBy(d => d.LastSeenAt)
            .ToListAsync(ct);

        if (devices.Count == 0)
        {
            var noDevicesMsg = $"""
                {TelegramConstants.Emojis.Success} <b>Excellent !</b>
                
                Tous les capteurs sont en ligne.
                """;

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🔄 Actualiser", "filter:offline"),
                    InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Lightning} État", "refresh:status"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Robot} Menu", TelegramConstants.Callbacks.BackToMenu),
                },
            });

            await telegram.EditMessageAsync(chatId, messageId, noDevicesMsg, ParseMode.Html, keyboard, ct);
            return;
        }

        var totalPages = (int)Math.Ceiling(devices.Count / (double)DevicesPerPage);
        page = Math.Clamp(page, 1, totalPages);

        var skip = (page - 1) * DevicesPerPage;
        var pageDevices = devices.Skip(skip).Take(DevicesPerPage).ToList();

        var deviceLines = pageDevices.Select(d =>
        {
            var displayName = !string.IsNullOrWhiteSpace(d.Description) ? d.Description : d.Name;
            var lastSeen = FormatTimeAgo(d.LastSeenAt);
            return $"• {TelegramConstants.Emojis.Warning} <b>{telegram.EscapeHtml(displayName)}</b>\n   {TelegramConstants.Emojis.Clock} {lastSeen}";
        });

        var response = $"""
            {TelegramConstants.Emojis.Warning} <b>Capteurs Hors Ligne</b> ({devices.Count})
            
            {string.Join("\n\n", deviceLines)}
            
            📄 Page {page}/{totalPages}
            """;

        var buttons = BuildPaginationButtons("filter:offline", page, totalPages);

        await telegram.EditMessageAsync(chatId, messageId, response, ParseMode.Html, new InlineKeyboardMarkup(buttons), ct);
    }

    private async Task ShowLowBatteryDevicesAsync(long chatId, int messageId, int page, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var devices = await db.Devices
            .Include(d => d.Company)
            .Where(d => d.ActiveInKropKontrol && d.Battery <= 20)
            .OrderBy(d => d.Battery)
            .ToListAsync(ct);

        if (devices.Count == 0)
        {
            var noDevicesMsg = $"""
                {TelegramConstants.Emojis.Success} <b>Excellent !</b>
                
                Tous les capteurs ont une batterie suffisante.
                """;

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🔄 Actualiser", "filter:lowbattery"),
                    InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Lightning} État", "refresh:status"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Robot} Menu", TelegramConstants.Callbacks.BackToMenu),
                },
            });

            await telegram.EditMessageAsync(chatId, messageId, noDevicesMsg, ParseMode.Html, keyboard, ct);
            return;
        }

        var totalPages = (int)Math.Ceiling(devices.Count / (double)DevicesPerPage);
        page = Math.Clamp(page, 1, totalPages);

        var skip = (page - 1) * DevicesPerPage;
        var pageDevices = devices.Skip(skip).Take(DevicesPerPage).ToList();

        var deviceLines = pageDevices.Select(d =>
        {
            var displayName = !string.IsNullOrWhiteSpace(d.Description) ? d.Description : d.Name;
            var batteryEmoji = d.Battery <= 10 ? "🪫" : "🔋";
            return $"• {batteryEmoji} <b>{telegram.EscapeHtml(displayName)}</b>\n   Batterie: {d.Battery:F0}%";
        });

        var response = $"""
            🪫 <b>Capteurs Batterie Faible</b> ({devices.Count})
            
            {string.Join("\n\n", deviceLines)}
            
            📄 Page {page}/{totalPages}
            """;

        var buttons = BuildPaginationButtons("filter:lowbattery", page, totalPages);

        await telegram.EditMessageAsync(chatId, messageId, response, ParseMode.Html, new InlineKeyboardMarkup(buttons), ct);
    }

    private static List<InlineKeyboardButton[]> BuildPaginationButtons(string prefix, int page, int totalPages)
    {
        var buttons = new List<InlineKeyboardButton[]>();

        // Navigation
        var navButtons = new List<InlineKeyboardButton>();

        if (page > 1)
        {
            navButtons.Add(InlineKeyboardButton.WithCallbackData("⬅️ Précédent", $"{prefix}:{page - 1}"));
        }

        if (page < totalPages)
        {
            navButtons.Add(InlineKeyboardButton.WithCallbackData("➡️ Suivant", $"{prefix}:{page + 1}"));
        }

        if (navButtons.Count > 0)
        {
            buttons.Add(navButtons.ToArray());
        }

        // Boutons de retour
        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("🔄 Actualiser", $"{prefix}:1"),
            InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Lightning} État", "refresh:status"),
        });

        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Robot} Menu", TelegramConstants.Callbacks.BackToMenu),
        });

        return buttons;
    }

    private static string FormatTimeAgo(DateTime? dateTime)
    {
        if (!dateTime.HasValue || dateTime.Value == default)
            return "Jamais";

        var ts = DateTime.UtcNow - dateTime.Value;

        //if (ts.TotalSeconds < 30) return "À l'instant";
        if (ts.TotalMinutes < 1) return $"{ts.Seconds} secondes";
        if (ts.TotalMinutes < 2) return "1 minute";
        if (ts.TotalHours < 1) return $"{(int)ts.TotalMinutes} minutes";
        if (ts.TotalHours < 2) return "1 heure";
        if (ts.TotalDays < 1) return $"{(int)ts.TotalHours} heures";
        if (ts.TotalDays < 2) return "1 jour";
        if (ts.TotalDays < 30) return $"{(int)ts.TotalDays} jours";
        if (ts.TotalDays < 60) return "1 mois";
        if (ts.TotalDays < 365) return $"{(int)(ts.TotalDays / 30)} mois";
        if (ts.TotalDays < 730) return "1 an";
        return $"{(int)(ts.TotalDays / 365)} ans";
    }
}
