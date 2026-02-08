using Kk.Kharts.Shared.Constants;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Kk.Kharts.Api.Services.Telegram.Commands.Callbacks;

/// <summary>
/// Handler para callbacks de seleção de modelo de sensor para gráficos.
/// Formato: chartmodel:{model}:{page}
/// </summary>
public sealed class ChartTypeCallbackHandler(
    ITelegramService telegram,
    ITelegramUserService userService) : ITelegramCallbackHandler
{
    public string CallbackPrefix => "chartmodel:";

    private const int DevicesPerPage = 8;

    public async Task HandleAsync(CallbackQuery callback, CancellationToken ct = default)
    {
        var data = callback.Data ?? string.Empty;
        // Format: chartmodel:EM300-TH:1 ou chartmodel:all:2
        // O último segmento é sempre a página
        var withoutPrefix = data.Replace("chartmodel:", "");
        var lastColonIndex = withoutPrefix.LastIndexOf(':');

        if (lastColonIndex <= 0)
        {
            await telegram.AnswerCallbackQueryAsync(callback.Id, "⚠️ Format invalide", ct: ct);
            return;
        }

        var model = withoutPrefix[..lastColonIndex];
        var page = int.TryParse(withoutPrefix[(lastColonIndex + 1)..], out var p) ? p : 1;

        await telegram.AnswerCallbackQueryAsync(callback.Id, $"Chargement page {page}...", ct: ct);

        var telegramUserId = callback.From.Id;
        var chatId = callback.Message?.Chat.Id ?? 0;
        var messageId = callback.Message?.MessageId ?? 0;

        // Obter dispositivos do utilizador
        var deviceEntities = await userService.GetUserDevicesAsync(telegramUserId, ct);

        // Filtrar por modelo se não for "all"
        var filteredDevices = model.Equals("all", StringComparison.OrdinalIgnoreCase)
            ? deviceEntities
            : deviceEntities.Where(d => (d.ModeloNavegacao?.Model ?? "Autre") == model).ToList();

        if (filteredDevices.Count == 0)
        {
            await telegram.EditMessageAsync(
                chatId,
                messageId,
                $"{TelegramConstants.Emojis.Warning} Aucun capteur trouvé pour ce type.",
                ct: ct);
            return;
        }

        // Ordenar por última comunicação (mais recente primeiro)
        var orderedDevices = filteredDevices
            .OrderByDescending(d => d.LastSeenAt)
            .ToList();

        var totalPages = (int)Math.Ceiling(orderedDevices.Count / (double)DevicesPerPage);
        page = Math.Clamp(page, 1, totalPages);

        var skip = (page - 1) * DevicesPerPage;
        var pageDevices = orderedDevices.Skip(skip).Take(DevicesPerPage).ToList();

        var modelLabel = model.Equals("all", StringComparison.OrdinalIgnoreCase)
            ? "Tous les capteurs"
            : model;

        var response = $"""
            {TelegramConstants.Emojis.Chart} <b>Graphiques - {modelLabel}</b>
            
            Sélectionnez un capteur ({orderedDevices.Count} total)
            📄 Page {page}/{totalPages}
            """;

        // Botões para cada dispositivo (2 colunas, usar Description se disponível)
        var buttons = new List<InlineKeyboardButton[]>();

        for (var i = 0; i < pageDevices.Count; i += 2)
        {
            var displayName1 = !string.IsNullOrWhiteSpace(pageDevices[i].Description) 
                ? pageDevices[i].Description 
                : pageDevices[i].Name;
            
            var row = new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData(
                    $"📊 {displayName1}",
                    $"chart:select:{pageDevices[i].DevEui}")
            };

            if (i + 1 < pageDevices.Count)
            {
                var displayName2 = !string.IsNullOrWhiteSpace(pageDevices[i + 1].Description) 
                    ? pageDevices[i + 1].Description 
                    : pageDevices[i + 1].Name;
                
                row.Add(InlineKeyboardButton.WithCallbackData(
                    $"📊 {displayName2}",
                    $"chart:select:{pageDevices[i + 1].DevEui}"));
            }

            buttons.Add(row.ToArray());
        }

        // Navegação
        var navButtons = new List<InlineKeyboardButton>();

        if (page > 1)
        {
            navButtons.Add(InlineKeyboardButton.WithCallbackData(
                "⬅️ Précédent",
                $"chartmodel:{model}:{page - 1}"));
        }

        if (page < totalPages)
        {
            navButtons.Add(InlineKeyboardButton.WithCallbackData(
                "➡️ Suivant",
                $"chartmodel:{model}:{page + 1}"));
        }

        if (navButtons.Count > 0)
        {
            buttons.Add(navButtons.ToArray());
        }

        // Botão para voltar à seleção de modelo
        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData("🔙 Modèles de capteurs", "menu:chart")
        });

        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Robot} Menu", TelegramConstants.Callbacks.BackToMenu)
        });

        await telegram.EditMessageAsync(
            chatId,
            messageId,
            response,
            ParseMode.Html,
            replyMarkup: new InlineKeyboardMarkup(buttons),
            ct: ct);
    }
}
