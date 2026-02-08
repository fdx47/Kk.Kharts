using Kk.Kharts.Shared.Constants;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Kk.Kharts.Api.Services.Telegram.Commands.Handlers;

/// <summary>
/// Handler pour la commande /link - Lie un compte Telegram à un utilisateur du système.
/// </summary>
public sealed class LinkCommandHandler(
    ITelegramService telegram,
    ITelegramUserService userService) : ITelegramCommandHandler
{
    public string Command => TelegramConstants.Commands.Link;
    public string Description => "Lier votre compte Telegram à KropKontrol";

    public async Task HandleAsync(Message message, CancellationToken ct = default)
    {
        var chatId = message.Chat.Id;
        var telegramUserId = message.From?.Id ?? 0;
        var telegramUsername = message.From?.Username ?? message.From?.FirstName ?? "Inconnu";

        // Vérifier si déjà lié
        var existingUser = await userService.GetUserByTelegramIdAsync(telegramUserId, ct);
        if (existingUser != null)
        {
            var alreadyLinkedMsg = $"""
                {TelegramConstants.Emojis.Success} <b>Compte déjà lié!</b>
                
                Votre Telegram est lié à:
                • 👤 <b>{telegram.EscapeHtml(existingUser.Nom)}</b>
                • 📧 {telegram.EscapeHtml(existingUser.Email)}
                • 🏢 {telegram.EscapeHtml(existingUser.Company?.Name ?? "N/A")}
                
                Pour délier, utilisez /unlink
                """;

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Device} Mes Capteurs", "menu:devices"),
                    InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Robot} Menu", TelegramConstants.Callbacks.BackToMenu),
                }
            });

            await telegram.SendToChatAsync(chatId, alreadyLinkedMsg, ParseMode.Html, replyMarkup: keyboard, ct: ct);
            return;
        }

        // Parser les arguments: /link email password
        var parts = message.Text?.Split(' ', StringSplitOptions.RemoveEmptyEntries) ?? [];

        if (parts.Length < 3)
        {
            var usageMsg = $"""
                {TelegramConstants.Emojis.Key} <b>Lier votre compte KropKontrol</b>
                
                Pour accéder à vos capteurs personnels, vous devez lier votre compte Telegram à votre compte KropKontrol.
                
                <b>Usage:</b>
                <code>/link votre@email.com votreMotDePasse</code>
                
                <b>Exemple:</b>
                <code>/link jean@entreprise.fr MonMotDePasse123</code>
                
                {TelegramConstants.Emojis.Warning} <i>Ce message sera supprimé après la liaison pour protéger vos identifiants.</i>
                
                {TelegramConstants.Emojis.Info} Vous n'avez pas de compte? Contactez votre administrateur.
                """;

            await telegram.SendToChatAsync(chatId, usageMsg, ParseMode.Html, ct: ct);
            return;
        }

        var email = parts[1];
        var password = string.Join(' ', parts.Skip(2)); // Le mot de passe peut contenir des espaces

        // Tenter de lier le compte
        var (success, resultMessage, user) = await userService.LinkAccountAsync(
            telegramUserId,
            telegramUsername,
            email,
            password,
            ct);

        // Supprimer le message contenant le mot de passe pour la sécurité
        try
        {
            await telegram.DeleteMessageAsync(chatId, message.MessageId, ct);
        }
        catch
        {
            // Ignorer si on ne peut pas supprimer
        }

        if (success && user != null)
        {
            var successMsg = $"""
                {TelegramConstants.Emojis.Success} <b>Compte lié avec succès!</b>
                
                Bienvenue, <b>{telegram.EscapeHtml(user.Nom)}</b>!
                
                Votre compte Telegram est maintenant lié à:
                • 📧 {telegram.EscapeHtml(user.Email)}
                • 🏢 {telegram.EscapeHtml(user.Company?.Name ?? "N/A")}
                • 🔑 Rôle: {user.Role}
                
                Vous pouvez maintenant accéder à vos capteurs personnels!
                """;

            var keyboard = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Device} Mes Capteurs", "menu:devices"),
                },
                new[]
                {
                    InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Robot} Menu Principal", TelegramConstants.Callbacks.BackToMenu),
                }
            });

            await telegram.SendToChatAsync(chatId, successMsg, ParseMode.Html, replyMarkup: keyboard, ct: ct);
        }
        else
        {
            var errorMsg = $"""
                {TelegramConstants.Emojis.Error} <b>Échec de la liaison</b>
                
                {resultMessage}
                
                Veuillez vérifier vos identifiants et réessayer.
                
                <b>Usage:</b>
                <code>/link votre@email.com votreMotDePasse</code>
                """;

            await telegram.SendToChatAsync(chatId, errorMsg, ParseMode.Html, ct: ct);
        }
    }
}
