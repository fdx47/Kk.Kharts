using Kk.Kharts.Api.Services.Telegram.Commands.Handlers;
using Kk.Kharts.Shared.Constants;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Kk.Kharts.Api.Services.Telegram.Commands.Callbacks;

/// <summary>
/// Handler para callbacks de seleção de tipo de dispositivo e paginação.
/// Formato: devtype:{type}:{page}
/// </summary>
public sealed class DeviceTypeCallbackHandler(
    ITelegramService telegram,
    ITelegramUserService userService) : ITelegramCallbackHandler
{
    public string CallbackPrefix => "devtype:";

    private const int DevicesPerPage = DevicesCommandHandler.DevicesPerPage;

    public async Task HandleAsync(CallbackQuery callback, CancellationToken ct = default)
    {
        var data = callback.Data ?? string.Empty;
        // Format: devtype:EM300-TH:1 ou devtype:all:2
        // O último segmento é sempre a página
        var withoutPrefix = data.Replace("devtype:", "");
        var lastColonIndex = withoutPrefix.LastIndexOf(':');

        if (lastColonIndex <= 0)
        {
            await telegram.AnswerCallbackQueryAsync(callback.Id, "⚠️ Format invalide", ct: ct);
            return;
        }

        var deviceType = withoutPrefix[..lastColonIndex];
        var page = int.TryParse(withoutPrefix[(lastColonIndex + 1)..], out var p) ? p : 1;

        await telegram.AnswerCallbackQueryAsync(callback.Id, $"Chargement page {page}...", ct: ct);

        var telegramUserId = callback.From.Id;
        var chatId = callback.Message?.Chat.Id ?? 0;
        var messageId = callback.Message?.MessageId ?? 0;

        // Obter dispositivos do utilizador
        var deviceEntities = await userService.GetUserDevicesAsync(telegramUserId, ct);

        // Filtrar por tipo se não for "all"
        var filteredDevices = deviceType.Equals("all", StringComparison.OrdinalIgnoreCase)
            ? deviceEntities
            : deviceEntities.Where(d => (d.ModeloNavegacao?.Model ?? "Autre") == deviceType).ToList();

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

        // Construir mensagem
        var deviceLines = pageDevices.Select(d =>
        {
            var statusEmoji = d.HasCommunicationAlarm
                ? TelegramConstants.Emojis.Warning
                : TelegramConstants.Emojis.Success;

            var batteryEmoji = d.Battery switch
            {
                <= 20 => "🪫",
                <= 50 => "🔋",
                _ => "🔋"
            };

            var lastSeen = d.LastSeenAt != default
                ? $"{(DateTime.UtcNow - d.LastSeenAt).TotalMinutes:F0}min"
                : "N/A";

            // Usar Description se disponível, senão Name
            var displayName = !string.IsNullOrWhiteSpace(d.Description) ? d.Description : d.Name;

            return $"{statusEmoji} <b>{telegram.EscapeHtml(displayName)}</b>\n" +
                   $"   {batteryEmoji} {d.Battery:F0}% | {TelegramConstants.Emojis.Clock} {lastSeen}";
        });

        var typeLabel = deviceType.Equals("all", StringComparison.OrdinalIgnoreCase) 
            ? "Tous les capteurs" 
            : deviceType;

        var response = $"""
            {TelegramConstants.Emojis.Device} <b>{typeLabel}</b> ({orderedDevices.Count} total)
            
            {string.Join("\n\n", deviceLines)}
            
            📄 Page {page}/{totalPages}
            """;

        // Botões para cada dispositivo (usar Description se disponível)
        var buttons = pageDevices
            .Select(d => new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    $"{(d.HasCommunicationAlarm ? "⚠️" : "📡")} {(!string.IsNullOrWhiteSpace(d.Description) ? d.Description : d.Name)}",
                    $"{TelegramConstants.Callbacks.DevicePrefix}{d.DevEui}")
            })
            .ToList();

        // Navegação
        var navButtons = new List<InlineKeyboardButton>();

        if (page > 1)
        {
            navButtons.Add(InlineKeyboardButton.WithCallbackData(
                "⬅️ Précédent",
                $"devtype:{deviceType}:{page - 1}"));
        }

        if (page < totalPages)
        {
            navButtons.Add(InlineKeyboardButton.WithCallbackData(
                "➡️ Suivant",
                $"devtype:{deviceType}:{page + 1}"));
        }

        if (navButtons.Count > 0)
        {
            buttons.Add(navButtons.ToArray());
        }

        // Botão para voltar à seleção de tipo
        buttons.Add(new[]
        {
           // InlineKeyboardButton.WithCallbackData("🔙 Tipos de capteurs", "menu:devices")
             InlineKeyboardButton.WithCallbackData("🔙 Types de capteurs", "menu:chart")
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
