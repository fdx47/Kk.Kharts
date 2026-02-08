using Kk.Kharts.Api.Data;
using Kk.Kharts.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Kk.Kharts.Api.Services.Telegram.Commands.Handlers;

/// <summary>
/// Handler para o comando /battery - Estado das baterias de todos os sensores.
/// </summary>
public sealed class BatteryCommandHandler(
    IServiceScopeFactory scopeFactory,
    ITelegramService telegram) : ITelegramCommandHandler
{
    public string Command => TelegramConstants.Commands.Battery;
    public string Description => "Voir l'état des batteries";

    public async Task HandleAsync(Message message, CancellationToken ct = default)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var devices = await db.Devices
            .Include(d => d.Company)
            .Where(d => d.ActiveInKropKontrol)
            .OrderBy(d => d.Battery)
            .Select(d => new
            {
                d.Name,
                d.Battery,
                d.LastSeenAt,
                CompanyName = d.Company.Name
            })
            .ToListAsync(ct);

        if (devices.Count == 0)
        {
            await telegram.SendToChatAsync(message.Chat.Id,
                $"{TelegramConstants.Emojis.Warning} Aucun capteur trouvé.", ct: ct);
            return;
        }

        var critical = devices.Where(d => d.Battery <= 10).ToList();
        var low = devices.Where(d => d.Battery > 10 && d.Battery <= 25).ToList();
        var medium = devices.Where(d => d.Battery > 25 && d.Battery <= 50).ToList();
        var good = devices.Where(d => d.Battery > 50).ToList();

        var response = new System.Text.StringBuilder();
        response.AppendLine($"{TelegramConstants.Emojis.Battery} <b>État des Batteries</b>");
        response.AppendLine();

        if (critical.Count > 0)
        {
            response.AppendLine($"🔴 <b>CRITIQUE (≤10%)</b> - {critical.Count} capteurs");
            foreach (var d in critical.Take(5))
            {
                response.AppendLine($"   • {telegram.EscapeHtml(d.Name)}: <b>{d.Battery:F0}%</b>");
            }
            if (critical.Count > 5) response.AppendLine($"   <i>... et plus {critical.Count - 5}</i>");
            response.AppendLine();
        }

        if (low.Count > 0)
        {
            response.AppendLine($"🟠 <b>FAIBLE (11-25%)</b> - {low.Count} capteurs");
            foreach (var d in low.Take(5))
            {
                response.AppendLine($"   • {telegram.EscapeHtml(d.Name)}: <b>{d.Battery:F0}%</b>");
            }
            if (low.Count > 5) response.AppendLine($"   <i>... et plus {low.Count - 5}</i>");
            response.AppendLine();
        }

        if (medium.Count > 0)
        {
            response.AppendLine($"🟡 <b>MOYEN (26-50%)</b> - {medium.Count} capteurs");
            response.AppendLine();
        }

        if (good.Count > 0)
        {
            response.AppendLine($"🟢 <b>BON (>50%)</b> - {good.Count} capteurs");
            response.AppendLine();
        }

        response.AppendLine("━━━━━━━━━━━━━━━━━━━━━━");
        response.AppendLine();
        response.AppendLine($"{TelegramConstants.Emojis.Chart} <b>Résumé:</b>");
        response.AppendLine($"• Total: {devices.Count}");
        response.AppendLine($"• Moyenne: {devices.Average(d => d.Battery):F0}%");
        response.AppendLine($"• Minimum: {devices.Min(d => d.Battery):F0}%");

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🔄 Actualiser", "refresh:battery"),
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Device} Capteurs", "menu:devices"),
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
}
