using Kk.Kharts.Shared.Constants;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Kk.Kharts.Api.Services.Telegram.Commands.Handlers;

/// <summary>
/// Handler pour la commande /chart - Affiche les graphiques des capteurs.
/// Montre automatiquement une liste de capteurs si aucun devEui n'est fourni.
/// </summary>
public sealed class ChartCommandHandler(
    ITelegramService telegram,
    ITelegramUserService userService) : ITelegramCommandHandler
{
    public string Command => TelegramConstants.Commands.Chart;
    public string Description => "Voir le graphique d'un capteur";

    public async Task HandleAsync(Message message, CancellationToken ct = default)
    {
        var telegramUserId = message.From?.Id ?? 0;
        var chatId = message.Chat.Id;

        // Vérifier si l'utilisateur est lié
        var user = await userService.GetUserByTelegramIdAsync(telegramUserId, ct);
        if (user == null)
        {
            await SendNotLinkedMessageAsync(chatId, ct);
            return;
        }

        var parts = message.Text?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];

        if (parts.Length >= 2)
        {
            var devEui = parts[1].ToUpperInvariant();
            
            // Vérifier l'accès au device
            if (!await userService.HasAccessToDeviceAsync(telegramUserId, devEui, ct))
            {
                await telegram.SendToChatAsync(chatId,
                    $"{TelegramConstants.Emojis.Lock} <b>Accès refusé</b>\n\nVous n'avez pas accès à ce capteur.",
                    ParseMode.Html, ct: ct);
                return;
            }
            
            await ShowChartOptionsAsync(chatId, devEui, ct);
            return;
        }

        // Afficher la liste des capteurs de l'utilisateur
        await ShowDeviceSelectionAsync(chatId, telegramUserId, ct);
    }

    private async Task SendNotLinkedMessageAsync(long chatId, CancellationToken ct)
    {
        var message = $"""
            {TelegramConstants.Emojis.Warning} <b>Compte non lié</b>
            
            Pour voir les graphiques de vos capteurs, vous devez d'abord lier votre compte.
            
            <b>Usage:</b> <code>/link votre@email.com votreMotDePasse</code>
            """;

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[] { InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Robot} Menu", TelegramConstants.Callbacks.BackToMenu) }
        });

        await telegram.SendToChatAsync(chatId, message, ParseMode.Html, replyMarkup: keyboard, ct: ct);
    }

    private async Task ShowDeviceSelectionAsync(long chatId, long telegramUserId, CancellationToken ct)
    {
        var devices = await userService.GetUserDevicesAsync(telegramUserId, ct);

        if (devices.Count == 0)
        {
            await telegram.SendToChatAsync(chatId,
                $"{TelegramConstants.Emojis.Warning} Aucun capteur trouvé.", ct: ct);
            return;
        }

        // Agrupar por modelo (model field)
        var devicesByModel = devices
            .GroupBy(d => d.ModeloNavegacao?.Model ?? "Autre")
            .OrderByDescending(g => g.Count())
            .Select(g => new { Model = g.Key, Count = g.Count() })
            .ToList();

        var response = $"""
            {TelegramConstants.Emojis.Chart} <b>Sélectionnez le modèle de capteur</b>
            
            {TelegramConstants.Emojis.Info} Total: {devices.Count} capteurs disponibles
            
            Choisissez un modèle pour voir les capteurs correspondants:
            """;

        var buttons = devicesByModel
            .Select(m => new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    $"📊 {m.Model} ({m.Count})",
                    $"chartmodel:{m.Model}:1")
            })
            .ToList();

        // Botão para ver todos
        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData(
                $"📋 Tous les capteurs ({devices.Count})",
                "chartmodel:all:1")
        });

        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Robot} Menu", TelegramConstants.Callbacks.BackToMenu)
        });

        await telegram.SendToChatAsync(
            chatId,
            response,
            ParseMode.Html,
            replyMarkup: new InlineKeyboardMarkup(buttons),
            ct: ct);
    }

    public async Task ShowChartOptionsAsync(long chatId, string devEui, CancellationToken ct)
    {
        var response = $"""
            {TelegramConstants.Emojis.Chart} <b>Graphique du capteur</b>
            
            Sélectionnez le type de données et la période:
            """;

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            // Período
            new[]
            {
                InlineKeyboardButton.WithCallbackData("36h", $"chart:period:{devEui}:36h"),
                InlineKeyboardButton.WithCallbackData("72h", $"chart:period:{devEui}:72h"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("7 jours", $"chart:period:{devEui}:7d"),
                InlineKeyboardButton.WithCallbackData("30 jours", $"chart:period:{devEui}:30d"),
            },
            // Tipo de dados
            new[]
            {
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Thermometer} Temp", $"chart:type:{devEui}:temp:24h"),
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Droplet} VWC", $"chart:type:{devEui}:vwc:24h"),
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Lightning} EC", $"chart:type:{devEui}:ec:24h"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Device} Autres Capteurs", TelegramConstants.Callbacks.BackToDevices),
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Robot} Menu", TelegramConstants.Callbacks.BackToMenu),
            },
        });

        await telegram.SendToChatAsync(chatId, response, ParseMode.Html, replyMarkup: keyboard, ct: ct);
    }
}
