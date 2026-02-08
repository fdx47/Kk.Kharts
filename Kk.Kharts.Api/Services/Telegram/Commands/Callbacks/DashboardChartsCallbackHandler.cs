using Kk.Kharts.Shared.Constants;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Kk.Kharts.Api.Services.Telegram.Commands.Callbacks;

/// <summary>
/// Handler pour les callbacks des graphiques configurés dans le dashboard.
/// Permet d'afficher les courbes pré-configurées par l'utilisateur.
/// </summary>
public sealed class DashboardChartsCallbackHandler(
    ITelegramService telegram,
    ITelegramUserService userService,
    IDashboardConfigService dashboardService,
    ITelegramChartService chartService) : ITelegramCallbackHandler
{
    public string CallbackPrefix => "dashchart:";

    public async Task HandleAsync(CallbackQuery callback, CancellationToken ct = default)
    {
        var data = callback.Data ?? string.Empty;
        var chatId = callback.Message?.Chat.Id ?? 0;
        var messageId = callback.Message?.MessageId ?? 0;
        var telegramUserId = callback.From.Id;

        // Vérifier l'accès
        var user = await userService.GetUserByTelegramIdAsync(telegramUserId, ct);
        if (user == null)
        {
            await telegram.AnswerCallbackQueryAsync(callback.Id, "⚠️ Compte non lié", showAlert: true, ct: ct);
            return;
        }

        // Parser: dashchart:list ou dashchart:show:{chartId}
        var parts = data.Split(':');
        var action = parts.Length > 1 ? parts[1] : "list";

        switch (action)
        {
            case "list":
                await ShowDashboardChartsListAsync(chatId, messageId, user.Id, ct);
                break;

            case "show":
                if (parts.Length >= 3)
                {
                    var chartId = parts[2];
                    await GenerateDashboardChartAsync(callback, chatId, user.Id, chartId, ct);
                }
                break;

            default:
                await ShowDashboardChartsListAsync(chatId, messageId, user.Id, ct);
                break;
        }
    }

    private async Task ShowDashboardChartsListAsync(long chatId, int messageId, int userId, CancellationToken ct)
    {
        var charts = await dashboardService.GetUserChartsAsync(userId, ct);

        if (charts.Count == 0)
        {
            var noChartsMsg = $"""
                {TelegramConstants.Emojis.Chart} <b>Mes Courbes</b>
                
                {TelegramConstants.Emojis.Info} Aucune courbe configurée dans votre dashboard.
                
                Configurez vos courbes dans l'application web KropKontrol pour les retrouver ici!
                """;

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Robot} Menu", TelegramConstants.Callbacks.BackToMenu)
                }
            });

            await telegram.EditMessageAsync(chatId, messageId, noChartsMsg, ParseMode.Html, keyboard, ct);
            return;
        }

        var response = $"""
            {TelegramConstants.Emojis.Chart} <b>Mes Courbes Configurées</b>
            
            {TelegramConstants.Emojis.Info} {charts.Count} courbe(s) disponible(s)
            
            Sélectionnez une courbe pour générer le graphique:
            """;

        // Créer les boutons pour chaque courbe
        var buttons = charts
            .Take(10) // Limiter à 10 courbes
            .Select(c => new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    $"📈 {c.Title ?? c.DevEui}",
                    $"dashchart:show:{c.Id}")
            })
            .ToList();

        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Device} Capteurs", "menu:devices"),
            InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Robot} Menu", TelegramConstants.Callbacks.BackToMenu)
        });

        await telegram.EditMessageAsync(
            chatId,
            messageId,
            response,
            ParseMode.Html,
            new InlineKeyboardMarkup(buttons),
            ct);
    }

    private async Task GenerateDashboardChartAsync(
        CallbackQuery callback,
        long chatId,
        int userId,
        string chartId,
        CancellationToken ct)
    {
        await telegram.AnswerCallbackQueryAsync(callback.Id, "📊 Génération du graphique...", ct: ct);

        var charts = await dashboardService.GetUserChartsAsync(userId, ct);
        var chartConfig = charts.FirstOrDefault(c => c.Id == chartId);

        if (chartConfig == null)
        {
            await telegram.SendToChatAsync(chatId,
                $"{TelegramConstants.Emojis.Warning} Courbe non trouvée.",
                ct: ct);
            return;
        }

        // Déterminer le type de graphique basé sur les variables
        var chartType = DetermineChartType(chartConfig.Variables);
        var period = chartConfig.IntervalDays switch
        {
            <= 0.5 => "12h",
            <= 1 => "24h",
            <= 1.5 => "36h",
            <= 2 => "48h",
            <= 7 => "7d",
            _ => "30d"
        };

        var chartStream = await chartService.GenerateChartAsync(chartConfig.DevEui, chartType, period, ct);

        if (chartStream == null)
        {
            await telegram.SendToChatAsync(chatId,
                $"{TelegramConstants.Emojis.Warning} Impossible de générer le graphique. Pas de données disponibles.",
                ct: ct);
            return;
        }

        var caption = $"""
            {TelegramConstants.Emojis.Chart} <b>{chartConfig.Title ?? chartConfig.DevEui}</b>
            
            Variables: {string.Join(", ", chartConfig.Variables.Take(3))}
            Période: {chartConfig.IntervalDays} jour(s)
            """;

        var keyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("🔄 Actualiser", $"dashchart:show:{chartId}"),
                InlineKeyboardButton.WithCallbackData("📊 Autres courbes", "dashchart:list"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Robot} Menu", TelegramConstants.Callbacks.BackToMenu),
            },
        });

        await telegram.SendPhotoToChatAsync(chatId, chartStream, caption, ParseMode.Html, keyboard, ct);
    }

    private static string DetermineChartType(List<string> variables)
    {
        if (variables.Count == 0)
            return TelegramConstants.ChartTypes.Temperature;

        var firstVar = variables[0].ToLowerInvariant();

        if (firstVar.Contains("temp") || firstVar.Contains("température"))
            return TelegramConstants.ChartTypes.Temperature;

        if (firstVar.Contains("vwc") || firstVar.Contains("mineral"))
            return TelegramConstants.ChartTypes.VWC;

        if (firstVar.Contains("ec") || firstVar.Contains("conductiv"))
            return TelegramConstants.ChartTypes.EC;

        if (firstVar.Contains("humid"))
            return TelegramConstants.ChartTypes.Humidity;

        return TelegramConstants.ChartTypes.Temperature;
    }
}
