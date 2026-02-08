using Kk.Kharts.Shared.Constants;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Kk.Kharts.Api.Services.Telegram.Commands.Handlers;

/// <summary>
/// Handler para o comando /help - Lista todos os comandos disponíveis.
/// </summary>
public sealed class HelpCommandHandler(ITelegramService telegram) : ITelegramCommandHandler
{
    public string Command => TelegramConstants.Commands.Help;
    public string Description => "Voir la liste des commandes disponibles";

    public async Task HandleAsync(Message message, CancellationToken ct = default)
    {
        var helpMessage = $"""
            {TelegramConstants.Emojis.Robot} <b>Bot - Assistant KropKontrol - Commandes Disponibles</b>
            
            {string.Join("\n", GetStaticCommandList())}
            """;

        await telegram.SendToChatAsync(message.Chat.Id, helpMessage, ParseMode.Html, ct: ct);
    }

    private static string[] GetStaticCommandList()
    {
        return [
            $"{TelegramConstants.Emojis.Key} <b>Compte</b>",
            "• <code>/lier email mot_de_passe</code> - Associer votre compte",
            "• <code>/delier</code> - Désassocier votre compte",
            "",
            $"{TelegramConstants.Emojis.Info} <b>Informations Générales</b>",
            "• <code>/start</code> - Ouvrir le menu principal",
            "• <code>/aide</code> - Afficher ce guide",
            "• <code>/statut</code> - Résumé rapide de l'état du système",
            "• <code>/usersstats</code> - Statistiques détaillées d'utilisation",
            "",
            $"{TelegramConstants.Emojis.Device} <b>Capteurs</b>",
            "• <code>/capteurs</code> - Voir tous vos capteurs",
            "• <code>/dernier</code> - Consulter la dernière lecture d'un capteur",
            "• <code>/horsligne</code> - Lister les capteurs hors ligne",
            "• <code>/batterie</code> - État des batteries",
            "",
            $"{TelegramConstants.Emojis.Chart} <b>Graphiques</b>",
            "• <code>/graphique</code> - Générer un graphique d'un capteur",
            "",
            $"{TelegramConstants.Emojis.Bell} <b>Alertes</b>",
            "• <code>/alertes</code> - Voir les alertes actives",
            "",
            $"{TelegramConstants.Emojis.Question} <b>Support</b>",
            "• <code>/support message</code> - Contacter le support",
            "",
            $"{TelegramConstants.Emojis.Rocket} <b>Application</b>",
            "• <code>/app</code> - Ouvrir la Mini-App KropKontrol",
            "",
            $"{TelegramConstants.Emojis.Warning} <b>Important:</b> Liez votre compte avec /lier pour accéder à vos capteurs personnels!"
        ];
    }
}
