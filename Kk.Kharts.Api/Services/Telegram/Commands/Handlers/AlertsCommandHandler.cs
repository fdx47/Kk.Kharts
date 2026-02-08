using Kk.Kharts.Api.Data;
using Kk.Kharts.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Kk.Kharts.Api.Services.Telegram.Commands.Handlers;

/// <summary>
/// Handler para o comando /alerts - Lista alertas ativos.
/// </summary>
public sealed class AlertsCommandHandler(
    IServiceScopeFactory scopeFactory,
    ITelegramService telegram) : ITelegramCommandHandler
{
    public string Command => TelegramConstants.Commands.Alerts;
    public string Description => "Voir les alertes actives";

    public async Task HandleAsync(Message message, CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var activeAlarms = await db.AlarmRules
            .Include(a => a.Device)
            .ThenInclude(d => d.Company)
            .Where(a => a.Enabled && a.IsAlarmActive)
            .OrderByDescending(a => a.Id)
            .Select(a => new
            {
                a.Id,
                a.Description,
                a.PropertyName,
                a.LowValue,
                a.HighValue,
                a.ActiveThresholdType,
                DeviceName = a.Device.Name,
                DeviceDevEui = a.Device.DevEui,
                CompanyName = a.Device.Company.Name
            })
            .ToListAsync(ct);

        if (activeAlarms.Count == 0)
        {
            var successMsg = $"""
                {TelegramConstants.Emojis.Success} <b>Tout est en ordre!</b>
                
                Il n'y a aucune alerte active pour le moment.
                
                {TelegramConstants.Emojis.Bell} Vos capteurs sont dans les paramètres normaux.
                """;

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("🔄 Atualizar", "refresh:alerts"),
                    InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Device} Sensores", "menu:devices"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Robot} Menu Principal", TelegramConstants.Callbacks.BackToMenu),
                },
            });

            await telegram.SendToChatAsync(message.Chat.Id, successMsg, ParseMode.Html, replyMarkup: keyboard, ct: ct);
            return;
        }

        var response = new System.Text.StringBuilder();
        response.AppendLine($"{TelegramConstants.Emojis.Warning} <b>Alertes Actives</b> ({activeAlarms.Count})");
        response.AppendLine();

        foreach (var alarm in activeAlarms.Take(8))
        {
            var thresholdInfo = alarm.ActiveThresholdType == "Low"
                ? $"en dessous de {alarm.LowValue:F1}"
                : $"au dessus de {alarm.HighValue:F1}";

            var propertyLabel = GetPropertyLabel(alarm.PropertyName);

            response.AppendLine($"🚨 <b>{telegram.EscapeHtml(alarm.DeviceName)}</b>");
            response.AppendLine($"   {TelegramConstants.Emojis.Farm} {telegram.EscapeHtml(alarm.CompanyName)}");
            response.AppendLine($"   📊 {propertyLabel} {thresholdInfo}");
            response.AppendLine();
        }

        if (activeAlarms.Count > 8)
        {
            response.AppendLine($"<i>... et plus {activeAlarms.Count - 8} alertes</i>");
        }

        var buttons = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🔄 Actualiser", "refresh:alerts"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.BellOff} Silencier Tous", "alerts:mute:all"),
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Bell} Activer Tous", "alerts:unmute:all"),
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
            replyMarkup: buttons,
            ct: ct);
    }

    private static string GetPropertyLabel(string? propertyName) => propertyName?.ToLowerInvariant() switch
    {
        "temperature" or "soiltemperature" => "🌡️ Température",
        "humidity" => "💧 Humidité",
        "mineralvwc" or "organicvwc" => "💧 VWC",
        "mineralecp" or "organicecp" => "⚡ EC",
        "battery" => "🔋 Batterie",
        "permittivite" => "📊 Permittivité",
        _ => $"📊 {propertyName}"
    };
}
