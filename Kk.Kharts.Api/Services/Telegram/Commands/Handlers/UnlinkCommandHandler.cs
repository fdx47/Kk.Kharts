using Kk.Kharts.Shared.Constants;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Kk.Kharts.Api.Services.Telegram.Commands.Handlers;

/// <summary>
/// Handler pour la commande /unlink - Délie un compte Telegram d'un utilisateur du système.
/// </summary>
public sealed class UnlinkCommandHandler(
    ITelegramService telegram,
    ITelegramUserService userService) : ITelegramCommandHandler
{
    public string Command => TelegramConstants.Commands.Unlink;
    public string Description => "Délier votre compte Telegram de KropKontrol";

    public async Task HandleAsync(Message message, CancellationToken ct = default)
    {
        var chatId = message.Chat.Id;
        var telegramUserId = message.From?.Id ?? 0;

        // Vérifier si lié
        var user = await userService.GetUserByTelegramIdAsync(telegramUserId, ct);
        if (user == null)
        {
            var notLinkedMsg = $"""
                {TelegramConstants.Emojis.Warning} <b>Compte non lié</b>
                
                Votre Telegram n'est lié à aucun compte KropKontrol.
                
                Utilisez /link pour lier votre compte.
                """;

            await telegram.SendToChatAsync(chatId, notLinkedMsg, ParseMode.Html, ct: ct);
            return;
        }

        // Délier le compte
        var success = await userService.UnlinkAccountAsync(telegramUserId, ct);

        if (success)
        {
            var successMsg = $"""
                {TelegramConstants.Emojis.Success} <b>Compte délié</b>
                
                Votre compte Telegram a été délié de <b>{telegram.EscapeHtml(user.Email)}</b>.
                
                Vous pouvez toujours utiliser les commandes publiques, mais vous n'aurez plus accès à vos capteurs personnels.
                
                Pour lier à nouveau, utilisez /link
                """;

            await telegram.SendToChatAsync(chatId, successMsg, ParseMode.Html, ct: ct);
        }
        else
        {
            var errorMsg = $"""
                {TelegramConstants.Emojis.Error} <b>Erreur</b>
                
                Une erreur s'est produite lors de la déliaison.
                Veuillez réessayer plus tard.
                """;

            await telegram.SendToChatAsync(chatId, errorMsg, ParseMode.Html, ct: ct);
        }
    }
}
