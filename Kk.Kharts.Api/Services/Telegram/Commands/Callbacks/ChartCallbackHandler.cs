using Kk.Kharts.Shared.Constants;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Kk.Kharts.Api.Services.Telegram.Commands.Callbacks;

/// <summary>
/// Handler pour les callbacks de graphiques.
/// </summary>
public sealed class ChartCallbackHandler(
    ITelegramService telegram,
    ITelegramChartService chartService,
    ITelegramUserService userService) : ITelegramCallbackHandler
{
    public string CallbackPrefix => "chart:";

    public async Task HandleAsync(CallbackQuery callback, CancellationToken ct = default)
    {
        var data = callback.Data ?? string.Empty;
        var chatId = callback.Message?.Chat.Id ?? 0;
        var telegramUserId = callback.From.Id;

        // Vérifier l'accès
        var user = await userService.GetUserByTelegramIdAsync(telegramUserId, ct);
        if (user == null)
        {
            await telegram.AnswerCallbackQueryAsync(callback.Id, "⚠️ Compte non lié", showAlert: true, ct: ct);
            return;
        }

        // Parser le callback: chart:type:devEui:chartType:period ou chart:select:devEui
        var parts = data.Split(':');

        if (parts.Length < 3)
        {
            await telegram.AnswerCallbackQueryAsync(callback.Id, "⚠️ Format invalide", ct: ct);
            return;
        }

        var action = parts[1];
        var devEui = parts[2];

        // Vérifier l'accès au device
        var hasAccess = await userService.HasAccessToDeviceAsync(telegramUserId, devEui, ct);
        if (!hasAccess)
        {
            await telegram.AnswerCallbackQueryAsync(callback.Id, "🔒 Accès refusé", showAlert: true, ct: ct);
            return;
        }

        switch (action)
        {
            case "select":
                await ShowChartOptionsAsync(chatId, devEui, ct);
                break;

            case "type":
                if (parts.Length >= 5)
                {
                    var chartType = parts[3];
                    var period = parts[4];
                    await GenerateAndSendChartAsync(callback, chatId, devEui, chartType, period, ct);
                }
                break;

            case "period":
                if (parts.Length >= 4)
                {
                    var period = parts[3];
                    await GenerateAndSendChartAsync(callback, chatId, devEui, TelegramConstants.ChartTypes.Temperature, period, ct);
                }
                break;

            default:
                await ShowChartOptionsAsync(chatId, devEui, ct);
                break;
        }
    }

    private async Task ShowChartOptionsAsync(long chatId, string devEui, CancellationToken ct)
    {
        var response = $"""
            {TelegramConstants.Emojis.Chart} <b>Options du graphique</b>
            
            Sélectionnez le type de données et la période:
            """;

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Thermometer} Temp 36h", $"chart:type:{devEui}:temp:36h"),
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Droplet} VWC 36h", $"chart:type:{devEui}:vwc:36h"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Lightning} EC 36h", $"chart:type:{devEui}:ec:36h"),
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Thermometer} Temp 7j", $"chart:type:{devEui}:temp:7d"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Droplet} VWC 7j", $"chart:type:{devEui}:vwc:7d"),
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Lightning} EC 7j", $"chart:type:{devEui}:ec:7d"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Device} Retour Capteurs", TelegramConstants.Callbacks.BackToDevices),
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Robot} Menu", TelegramConstants.Callbacks.BackToMenu),
            },
        });

        await telegram.SendToChatAsync(chatId, response, ParseMode.Html, replyMarkup: keyboard, ct: ct);
    }

    private async Task GenerateAndSendChartAsync(
        CallbackQuery callback,
        long chatId,
        string devEui,
        string chartType,
        string period,
        CancellationToken ct)
    {
        await telegram.AnswerCallbackQueryAsync(callback.Id, "📊 Génération du graphique...", ct: ct);

        var chartStream = await chartService.GenerateChartAsync(devEui, chartType, period, ct);

        if (chartStream == null)
        {
            await telegram.SendToChatAsync(chatId,
                $"{TelegramConstants.Emojis.Warning} Impossible de générer le graphique. Pas de données disponibles.",
                ct: ct);
            return;
        }

        var periodLabel = period switch
        {
            "6h" => "6 heures",
            "12h" => "12 heures",
            "24h" => "24 heures",
            "36h" => "36 heures",
            "48h" => "48 heures",
            "7d" => "7 jours",
            "30d" => "30 jours",
            _ => period
        };

        var typeLabel = chartType switch
        {
            "temp" => "Température",
            "vwc" => "VWC",
            "ec" => "EC",
            "humidity" => "Humidité",
            _ => chartType
        };

        var caption = $"{TelegramConstants.Emojis.Chart} <b>{typeLabel}</b> - Dernières {periodLabel}";

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🔄 Actualiser", $"chart:type:{devEui}:{chartType}:{period}"),
                InlineKeyboardButton.WithCallbackData("📊 Autres", $"chart:select:{devEui}"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Device} Capteurs", TelegramConstants.Callbacks.BackToDevices),
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Robot} Menu", TelegramConstants.Callbacks.BackToMenu),
            },
        });

        await telegram.SendPhotoToChatAsync(chatId, chartStream, caption, ParseMode.Html, keyboard, ct);
    }
}
