using Kk.Kharts.Api.Data;
using Kk.Kharts.Api.Services.Telegram.Commands.Handlers;
using Kk.Kharts.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Kk.Kharts.Api.Services.Telegram.Commands.Callbacks;

/// <summary>
/// Handler para callbacks de alertas (silenciar, ativar, etc.).
/// Formato: alerts:{action}:{target}
/// </summary>
public sealed class AlertsCallbackHandler(
    IServiceScopeFactory scopeFactory,
    IServiceProvider serviceProvider,
    ITelegramService telegram) : ITelegramCallbackHandler
{
    public string CallbackPrefix => TelegramConstants.Callbacks.AlertPrefix;

    public async Task HandleAsync(CallbackQuery callback, CancellationToken ct = default)
    {
        var data = callback.Data ?? string.Empty;
        // Format: alerts:mute:all ou alerts:unmute:all ou alerts:mute:{id}
        var parts = data.Replace("alert:", "").Split(':');

        if (parts.Length < 2)
        {
            await telegram.AnswerCallbackQueryAsync(callback.Id, "⚠️ Format invalide", true, ct: ct);
            return;
        }

        var action = parts[0];
        var target = parts[1];
        var chatId = callback.Message?.Chat.Id ?? 0;
        var messageId = callback.Message?.MessageId ?? 0;

        switch (action)
        {
            case "mute":
                await HandleMuteAsync(callback, target, chatId, messageId, ct);
                break;

            case "unmute":
                await HandleUnmuteAsync(callback, target, chatId, messageId, ct);
                break;

            default:
                await telegram.AnswerCallbackQueryAsync(callback.Id, "⚠️ Action non reconnue", true, ct: ct);
                break;
        }
    }

    private async Task HandleMuteAsync(CallbackQuery callback, string target, long chatId, int messageId, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        int mutedCount;

        if (target == "all")
        {
            // Silenciar todos os alertas ativos
            var activeAlarms = await db.AlarmRules
                .Where(a => a.Enabled && a.IsAlarmActive)
                .ToListAsync(ct);

            foreach (var alarm in activeAlarms)
            {
                alarm.IsAlarmHandled = true;
            }

            mutedCount = activeAlarms.Count;
            await db.SaveChangesAsync(ct);

            await telegram.AnswerCallbackQueryAsync(callback.Id, $"🔕 {mutedCount} alertes silenciées", ct: ct);
        }
        else if (int.TryParse(target, out var alarmId))
        {
            var alarm = await db.AlarmRules.FindAsync([alarmId], ct);
            if (alarm != null)
            {
                alarm.IsAlarmHandled = true;
                await db.SaveChangesAsync(ct);
                mutedCount = 1;
                await telegram.AnswerCallbackQueryAsync(callback.Id, "🔕 Alerte silenciée", ct: ct);
            }
            else
            {
                await telegram.AnswerCallbackQueryAsync(callback.Id, "⚠️ Alerte non trouvée", true, ct: ct);
                return;
            }
        }
        else
        {
            await telegram.AnswerCallbackQueryAsync(callback.Id, "⚠️ Format invalide", true, ct: ct);
            return;
        }

        // Atualizar a mensagem com botão para reativar
        var response = $"""
            {TelegramConstants.Emojis.BellOff} <b>Alertes Silenciées</b>
            
            Les alertes ont été marquées comme traitées.
            Elles ne seront plus notifiées jusqu'à ce qu'elles soient réactivées ou qu'une nouvelle alerte se déclenche.
            """;

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Bell} Réactiver Tous", "alert:unmute:all"),
                InlineKeyboardButton.WithCallbackData("🔄 Actualiser", "refresh:alerts"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Robot} Menu Principal", TelegramConstants.Callbacks.BackToMenu),
            },
        });

        await telegram.EditMessageAsync(chatId, messageId, response, ParseMode.Html, keyboard, ct);
    }

    private async Task HandleUnmuteAsync(CallbackQuery callback, string target, long chatId, int messageId, CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        int unmutedCount;

        if (target == "all")
        {
            // Reativar todos os alertas silenciados
            var handledAlarms = await db.AlarmRules
                .Where(a => a.Enabled && a.IsAlarmHandled)
                .ToListAsync(ct);

            foreach (var alarm in handledAlarms)
            {
                alarm.IsAlarmHandled = false;
            }

            unmutedCount = handledAlarms.Count;
            await db.SaveChangesAsync(ct);

            await telegram.AnswerCallbackQueryAsync(callback.Id, $"🔔 {unmutedCount} alertes réactivées", ct: ct);
        }
        else if (int.TryParse(target, out var alarmId))
        {
            var alarm = await db.AlarmRules.FindAsync([alarmId], ct);
            if (alarm != null)
            {
                alarm.IsAlarmHandled = false;
                await db.SaveChangesAsync(ct);
                unmutedCount = 1;
                await telegram.AnswerCallbackQueryAsync(callback.Id, "🔔 Alerte réactivée", ct: ct);
            }
            else
            {
                await telegram.AnswerCallbackQueryAsync(callback.Id, "⚠️ Alerte non trouvée", true, ct: ct);
                return;
            }
        }
        else
        {
            await telegram.AnswerCallbackQueryAsync(callback.Id, "⚠️ Format invalide", true, ct: ct);
            return;
        }

        // Redirecionar para a lista de alertas
        var fakeMessage = new Message
        {
            Chat = callback.Message?.Chat!,
            From = callback.From,
            Text = "/alerts"
        };

        var alertsHandler = serviceProvider.GetRequiredService<AlertsCommandHandler>();
        await alertsHandler.HandleAsync(fakeMessage, ct);
    }
}
