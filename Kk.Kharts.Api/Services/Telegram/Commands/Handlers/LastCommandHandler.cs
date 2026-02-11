using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Utils;
using Kk.Kharts.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Kk.Kharts.Api.Services.Telegram.Commands.Handlers;

/// <summary>
/// Handler para o comando /last - Última leitura de um sensor.
/// </summary>
public sealed class LastCommandHandler(
    IServiceScopeFactory scopeFactory,
    ITelegramService telegram,
    ITelegramUserService userService) : ITelegramCommandHandler
{
    public string Command => TelegramConstants.Commands.Last;
    public string Description => "Voir la dernière lecture d'un capteur";

    public async Task HandleAsync(Message message, CancellationToken ct = default)
    {
        var telegramUserId = message.From?.Id ?? 0;
        var parts = message.Text?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];
        
        if (parts.Length < 2)
        {
            await ShowDeviceSelectionAsync(message.Chat.Id, telegramUserId, ct);
            return;
        }

        var devEui = DevEuiNormalizer.Normalize(parts[1]);
        await ShowLastReadingAsync(message.Chat.Id, devEui, telegramUserId, ct);
    }

    private async Task ShowDeviceSelectionAsync(long chatId, long telegramUserId, CancellationToken ct)
    {
        // Vérifier si l'utilisateur est lié
        var user = await userService.GetUserByTelegramIdAsync(telegramUserId, ct);
        
        if (user == null)
        {
            var notLinkedMsg = $"""
                {TelegramConstants.Emojis.Warning} <b>Compte non lié</b>
                
                Pour voir vos capteurs, vous devez d'abord lier votre compte.
                
                <b>Usage:</b> <code>/link votre@email.com votreMotDePasse</code>
                """;
            await telegram.SendToChatAsync(chatId, notLinkedMsg, ParseMode.Html, ct: ct);
            return;
        }

        var deviceEntities = await userService.GetUserDevicesAsync(telegramUserId, ct);
        var devices = deviceEntities.Take(10).Select(d => new { d.DevEui, d.Name }).ToList();

        if (devices.Count == 0)
        {
            await telegram.SendToChatAsync(chatId, 
                $"{TelegramConstants.Emojis.Warning} Aucun capteur trouvé.", ct: ct);
            return;
        }

        var buttons = devices
            .Select(d => new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    $"{TelegramConstants.Emojis.Device} {d.Name}",
                    $"last:{d.DevEui}")
            })
            .ToList();

        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Robot} Menu", TelegramConstants.Callbacks.BackToMenu)
        });

        await telegram.SendToChatAsync(
            chatId,
            $"{TelegramConstants.Emojis.Device} <b>Sélectionnez un capteur:</b>",
            ParseMode.Html,
            replyMarkup: new InlineKeyboardMarkup(buttons),
            ct: ct);
    }

    public async Task ShowLastReadingAsync(long chatId, string devEui, long telegramUserId = 0, CancellationToken ct = default)
    {
        // Vérifier l'accès si telegramUserId est fourni
        if (telegramUserId > 0)
        {
            var hasAccess = await userService.HasAccessToDeviceAsync(telegramUserId, devEui, ct);
            if (!hasAccess)
            {
                await telegram.SendToChatAsync(chatId,
                    $"{TelegramConstants.Emojis.Lock} <b>Accès refusé</b>\n\nVous n'avez pas accès à ce capteur.",
                    ParseMode.Html, ct: ct);
                return;
            }
        }

        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var device = await db.Devices
            .Include(d => d.Company)
            .FirstOrDefaultAsync(d => d.DevEui == devEui, ct);

        if (device == null)
        {
            await telegram.SendToChatAsync(chatId,
                $"{TelegramConstants.Emojis.Warning} Capteur non trouvé.",
                ParseMode.Html, ct: ct);
            return;
        }

        // Tentar obter última leitura de UC502 (WET150)
        var wet150Reading = await db.Uc502Wet150s
            .Where(r => r.DevEui == devEui)
            .OrderByDescending(r => r.Timestamp)
            .FirstOrDefaultAsync(ct);

        if (wet150Reading != null)
        {
            var timeSince = DateTime.UtcNow - wet150Reading.Timestamp;
            var displayName = !string.IsNullOrWhiteSpace(device.Description) ? device.Description : device.Name;
            var response = $"""
                {TelegramConstants.Emojis.Device} <b>{telegram.EscapeHtml(displayName)}</b>
                {TelegramConstants.Emojis.Farm} {telegram.EscapeHtml(device.Company?.Name ?? "N/A")}
                
                ━━━━━━━━━━━━━━━━━━━━━━
                
                {TelegramConstants.Emojis.Thermometer} <b>Température Sol:</b> {wet150Reading.SoilTemperature:F1}°C
                {TelegramConstants.Emojis.Droplet} <b>VWC Minéral:</b> {wet150Reading.MineralVWC:F1}%
                {TelegramConstants.Emojis.Wave} <b>EC Minéral:</b> {wet150Reading.MineralECp:F2} mS/cm
                {TelegramConstants.Emojis.Lightning} <b>Permittivité:</b> {wet150Reading.Permittivite:F1}
                {TelegramConstants.Emojis.Battery} <b>Batterie:</b> {device.Battery:F0}%
                
                ━━━━━━━━━━━━━━━━━━━━━━
                
                {TelegramConstants.Emojis.Clock} <b>Dernière lecture:</b> {wet150Reading.Timestamp:dd/MM/yyyy HH:mm} UTC
                {TelegramConstants.Emojis.Calendar} <i>Il y a {FormatTimeSpan(timeSince)}</i>
                """;

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Chart} Voir Graphique", $"chart:select:{devEui}"),
                    InlineKeyboardButton.WithCallbackData("🔄 Actualiser", $"last:{devEui}"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Device} Autres Capteurs", TelegramConstants.Callbacks.BackToDevices),
                },
            });

            await telegram.SendToChatAsync(chatId, response, ParseMode.Html, replyMarkup: keyboard, ct: ct);
            return;
        }

        // Tentar EM300-TH
        var em300ThReading = await db.Em300ths
            .Where(r => r.DevEui == devEui)
            .OrderByDescending(r => r.Timestamp)
            .FirstOrDefaultAsync(ct);

        if (em300ThReading != null)
        {
            var timeSince = DateTime.UtcNow - em300ThReading.Timestamp;
            var displayName = !string.IsNullOrWhiteSpace(device.Description) ? device.Description : device.Name;
            var response = $"""
                {TelegramConstants.Emojis.Device} <b>{telegram.EscapeHtml(displayName)}</b>
                {TelegramConstants.Emojis.Farm} {telegram.EscapeHtml(device.Company?.Name ?? "N/A")}
                
                ━━━━━━━━━━━━━━━━━━━━━━
                
                {TelegramConstants.Emojis.Thermometer} <b>Température:</b> {em300ThReading.Temperature:F1}°C
                {TelegramConstants.Emojis.Droplet} <b>Humidité:</b> {em300ThReading.Humidity:F1}%
                {TelegramConstants.Emojis.Battery} <b>Batterie:</b> {em300ThReading.Battery:F0}%
                
                ━━━━━━━━━━━━━━━━━━━━━━
                
                {TelegramConstants.Emojis.Clock} <b>Dernière lecture:</b> {em300ThReading.Timestamp:dd/MM/yyyy HH:mm} UTC
                {TelegramConstants.Emojis.Calendar} <i>Il y a {FormatTimeSpan(timeSince)}</i>
                """;

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Chart} Voir Graphique", $"chart:select:{devEui}"),
                    InlineKeyboardButton.WithCallbackData("🔄 Actualiser", $"last:{devEui}"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Device} Autres Capteurs", TelegramConstants.Callbacks.BackToDevices),
                },
            });

            await telegram.SendToChatAsync(chatId, response, ParseMode.Html, replyMarkup: keyboard, ct: ct);
            return;
        }

        await telegram.SendToChatAsync(chatId,
            $"{TelegramConstants.Emojis.Warning} Aucune lecture pour le capteur <b>{telegram.EscapeHtml(device.Name)}</b>.",
            ParseMode.Html, ct: ct);
    }

    private static string FormatTimeSpan(TimeSpan ts)
    {

        if (ts.TotalMinutes < 1) return $"{ts.Seconds} secondes";
        if (ts.TotalMinutes < 2) return "1 minute";
        if (ts.TotalHours < 1) return $"{(int)ts.TotalMinutes} minutes";
        if (ts.TotalHours < 24) return $"{(int)ts.TotalHours}h {ts.Minutes:D2}m";
        if (ts.TotalDays < 2) return "1 jour";
        if (ts.TotalDays < 30) return $"{(int)ts.TotalDays} jours";
        if (ts.TotalDays < 60) return "1 mois";
        if (ts.TotalDays < 365) return $"{(int)(ts.TotalDays / 30)} mois";
        if (ts.TotalDays < 730) return "1 an";
        return $"{(int)(ts.TotalDays / 365)} ans";
    }
}
