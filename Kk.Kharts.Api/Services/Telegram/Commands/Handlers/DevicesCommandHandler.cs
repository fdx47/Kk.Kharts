using Kk.Kharts.Shared.Constants;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Kk.Kharts.Api.Services.Telegram.Commands.Handlers;

/// <summary>
/// Handler para o comando /devices - Lista sensores por tipo.
/// Primeiro mostra os tipos disponíveis, depois pagina os dispositivos.
/// </summary>
public sealed class DevicesCommandHandler(
    ITelegramService telegram,
    ITelegramUserService userService) : ITelegramCommandHandler
{
    public string Command => TelegramConstants.Commands.Devices;
    public string Description => "Voir la liste des capteurs";

    public const int DevicesPerPage = 8;

    public async Task HandleAsync(Message message, CancellationToken ct = default)
    {
        var telegramUserId = message.From?.Id ?? 0;
        
        // Vérifier si l'utilisateur est lié
        var user = await userService.GetUserByTelegramIdAsync(telegramUserId, ct);
        
        if (user == null)
        {
            var notLinkedMsg = $"""
                {TelegramConstants.Emojis.Warning} <b>Compte non lié</b>
                
                Pour voir vos capteurs personnels, vous devez d'abord lier votre compte Telegram à votre compte KropKontrol.
                
                <b>Usage:</b>
                <code>/link votre@email.com votreMotDePasse</code>
                """;

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Robot} Menu", TelegramConstants.Callbacks.BackToMenu)
                }
            });

            await telegram.SendToChatAsync(message.Chat.Id, notLinkedMsg, ParseMode.Html, replyMarkup: keyboard, ct: ct);
            return;
        }

        // Récupérer les devices accessibles par cet utilisateur
        var deviceEntities = await userService.GetUserDevicesAsync(telegramUserId, ct);

        if (deviceEntities.Count == 0)
        {
            await telegram.SendToChatAsync(
                message.Chat.Id,
                $"{TelegramConstants.Emojis.Warning} Aucun capteur trouvé.",
                ct: ct);
            return;
        }

        // Agrupar por tipo de modelo
        var devicesByType = deviceEntities
            .GroupBy(d => d.ModeloNavegacao?.Model ?? "Autre")
            .OrderByDescending(g => g.Count())
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToList();

        // Mostrar seleção de tipo
        var response = $"""
            {TelegramConstants.Emojis.Device} <b>Sélectionnez le type de capteur</b>
            
            {TelegramConstants.Emojis.Info} Total: {deviceEntities.Count} capteurs actifs
            """;

        var buttons = devicesByType
            .Select(t => new[]
            {
                InlineKeyboardButton.WithCallbackData(
                    $"📡 {t.Type} ({t.Count})",
                    $"devtype:{t.Type}:1")
            })
            .ToList();

        // Botão para ver todos
        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData(
                $"📋 Tous les capteurs ({deviceEntities.Count})",
                "devtype:all:1")
        });

        buttons.Add(new[]
        {
            InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Robot} Menu", TelegramConstants.Callbacks.BackToMenu)
        });

        await telegram.SendToChatAsync(
            message.Chat.Id,
            response,
            ParseMode.Html,
            replyMarkup: new InlineKeyboardMarkup(buttons),
            ct: ct);
    }
}
