using Kk.Kharts.Shared.Constants;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Kk.Kharts.Api.Services.Telegram.Commands.Handlers;

/// <summary>
/// Handler pour la commande /support - Envoyer un message au support.
/// Le message est transmis au canal de debug et les admins peuvent répondre.
/// </summary>
public sealed class SupportCommandHandler(
    ITelegramService telegram,
    ITelegramUserService userService) : ITelegramCommandHandler
{
    public string Command => "/support";
    public string Description => "Contacter le support technique";

    public async Task HandleAsync(Message message, CancellationToken ct = default)
    {
        var parts = message.Text?.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries) ?? [];
        
        if (parts.Length < 2)
        {
            var helpMessage = $"""
                {TelegramConstants.Emojis.Question} <b>Contacter le Support</b>
                
                Pour envoyer un message au support, utilisez:
                <code>/support votre message ici</code>
                
                <i>Exemple:</i>
                <code>/support Je n'arrive pas à voir mes capteurs</code>
                
                Notre équipe vous répondra dès que possible.
                """;
            
            await telegram.SendToChatAsync(message.Chat.Id, helpMessage, ParseMode.Html, ct: ct);
            return;
        }

        var supportMessage = parts[1];
        var userId = message.From?.Id ?? 0;
        var username = message.From?.Username ?? "inconnu";
        var firstName = message.From?.FirstName ?? "";
        var lastName = message.From?.LastName ?? "";
        var fullName = $"{firstName} {lastName}".Trim();

        // Obtenir les infos de l'utilisateur lié
        var user = await userService.GetUserByTelegramIdAsync(userId, ct);
        var userInfo = user != null 
            ? $"<b>Compte:</b> {telegram.EscapeHtml(user.Email ?? "N/A")}"
            : "<i>Compte non lié</i>";

        // Envoyer au tópico de support avec les infos pour répondre
        var supportNotification = $"""
            {TelegramConstants.Emojis.Question} <b>Nouvelle demande de support</b>
            
            {TelegramConstants.Emojis.User} <b>Utilisateur:</b> {telegram.EscapeHtml(fullName)}
            📱 <b>Username:</b> @{telegram.EscapeHtml(username)}
            🆔 <b>Chat ID:</b> <code>{message.Chat.Id}</code>
            {userInfo}
            
            ━━━━━━━━━━━━━━━━━━━━━━
            
            💬 <b>Message:</b>
            {telegram.EscapeHtml(supportMessage)}
            
            ━━━━━━━━━━━━━━━━━━━━━━
            
            <i>Pour répondre, utilisez:</i>
            <code>/reply {message.Chat.Id} votre réponse</code>
            """;

        await telegram.SendToSupportTopicAsync(supportNotification, ParseMode.Html, ct);

        // Confirmer à l'utilisateur
        var confirmMessage = $"""
            {TelegramConstants.Emojis.Success} <b>Message envoyé !</b>
            
            Votre demande a été transmise à notre équipe de support.
            Nous vous répondrons dès que possible.
            
            <i>Merci de votre patience.</i>
            """;

        await telegram.SendToChatAsync(message.Chat.Id, confirmMessage, ParseMode.Html, ct: ct);
    }
}
