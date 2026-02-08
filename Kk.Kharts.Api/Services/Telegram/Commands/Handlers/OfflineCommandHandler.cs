using Kk.Kharts.Api.Data;
using Kk.Kharts.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Kk.Kharts.Api.Services.Telegram.Commands.Handlers;

/// <summary>
/// Handler para o comando /offline - Lista sensores sem comunicação.
/// </summary>
public sealed class OfflineCommandHandler(
    IServiceScopeFactory scopeFactory,
    ITelegramService telegram) : ITelegramCommandHandler
{
    public string Command => TelegramConstants.Commands.Offline;
    public string Description => "Voir les capteurs hors ligne";

    public async Task HandleAsync(Message message, CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var offlineDevices = await db.Devices
            .Include(d => d.Company)
            .Where(d => d.ActiveInKropKontrol && d.HasCommunicationAlarm)
            .OrderBy(d => d.LastSeenAt)
            .Select(d => new
            {
                d.DevEui,
                d.Name,
                d.Description,
                d.LastSeenAt,
                d.Battery,
                CompanyName = d.Company.Name
            })
            .ToListAsync(ct);

        if (offlineDevices.Count == 0)
        {
            var successMsg = $"""
                {TelegramConstants.Emojis.Success} <b>Excellent!</b>
                
                Tous les capteurs communiquent normalement.
                
                {TelegramConstants.Emojis.Device} Aucun capteur hors ligne pour le moment.
                """;

            await telegram.SendToChatAsync(message.Chat.Id, successMsg, ParseMode.Html, ct: ct);
            return;
        }

        var response = new System.Text.StringBuilder();
        response.AppendLine($"{TelegramConstants.Emojis.Warning} <b>Capteurs Hors Ligne</b> ({offlineDevices.Count})");
        response.AppendLine();

        foreach (var device in offlineDevices.Take(10))
        {
            var timeSince = device.LastSeenAt != default
                ? FormatTimeSpan(DateTime.UtcNow - device.LastSeenAt)
                : "Jamais";

            response.AppendLine($"🔴 <b>{telegram.EscapeHtml(device.Name)}</b>");
            response.AppendLine($"   {TelegramConstants.Emojis.Farm} {telegram.EscapeHtml(device.CompanyName)}");
            response.AppendLine($"   {TelegramConstants.Emojis.Clock} Dernière communication: {timeSince}");
            response.AppendLine($"   {TelegramConstants.Emojis.Battery} Batterie: {device.Battery:F0}%");
            response.AppendLine();
        }

        if (offlineDevices.Count > 10)
        {
            response.AppendLine($"<i>... et plus {offlineDevices.Count - 10} capteurs hors ligne</i>");
        }

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🔄 Actualiser", "refresh:offline"),
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Chart} État Général", "menu:status"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Robot} Menu Principal", TelegramConstants.Callbacks.BackToMenu),
            },
        });

        await telegram.SendToChatAsync(
            message.Chat.Id,
            response.ToString(),
            ParseMode.Html,
            replyMarkup: keyboard,
            ct: ct);
    }

    private static string FormatTimeSpan(TimeSpan ts)
    {
        if (ts.TotalDays >= 1) return $"{ts.TotalDays:F0} jours";
        if (ts.TotalHours >= 1) return $"{ts.TotalHours:F0}h {ts.Minutes}min";
        return $"{ts.TotalMinutes:F0} minutes";
    }
}
