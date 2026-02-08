using Kk.Kharts.Shared.Constants;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Kk.Kharts.Api.Services.Telegram.Commands.Handlers;

/// <summary>
/// Handler para o comando /start - Boas-vindas e menu principal.
/// </summary>
public sealed class StartCommandHandler(ITelegramService telegram) : ITelegramCommandHandler
{
    public string Command => TelegramConstants.Commands.Start;
    public string Description => "Démarrer le bot et voir le menu principal";

    public async Task HandleAsync(Message message, CancellationToken ct = default)
    {
        var userName = message.From?.FirstName ?? "Utilisateur";

        var welcomeMessage = $"""
            {TelegramConstants.Emojis.Plant} <b>Bienvenue sur KropKontrol, {telegram.EscapeHtml(userName)}!</b>
            
            {TelegramConstants.Emojis.Tractor} La plateforme IoT leader en agriculture de précision.
            
            {TelegramConstants.Emojis.Gear} <b>Ce que je peux faire pour vous:</b>
            
            {TelegramConstants.Emojis.Device} <b>Surveillance</b>
            • Voir l'état de vos capteurs en temps réel
            • Recevoir des alertes pour les valeurs anormales
            • Consulter l'historique des mesures
            
            {TelegramConstants.Emojis.Chart} <b>Analyse</b>
            • Graphiques de température, humidité et VWC
            • Rapports de consommation d'eau
            • Analyse de fertilisation
            
            {TelegramConstants.Emojis.Bell} <b>Alertes</b>
            • Configurer les seuils d'alarme
            • Notifications instantanées
            • Résumé quotidien de l'état des capteurs
            
            Utilisez les boutons ci-dessous ou tapez /help pour voir toutes les commandes.
            """;

        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Device} Mes Capteurs", "menu:devices"),
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Chart} Graphiques", "menu:charts"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Lightning} État Rapide", "menu:status"),
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Bell} Alertes", "menu:alerts"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Battery} Batteries", "menu:battery"),
                InlineKeyboardButton.WithCallbackData($"{TelegramConstants.Emojis.Question} Aide", "menu:help"),
            },
            new[]
            {
                InlineKeyboardButton.WithWebApp(
                    $"{TelegramConstants.Emojis.Rocket} Mini-App",
                    new WebAppInfo
                    {
                        Url = "https://kropkontrol.com/miniapp/"
                    })
            },
        });

        // Envoyer le message avec le clavier inline
        await telegram.SendToChatAsync(
            message.Chat.Id, 
            welcomeMessage, 
            ParseMode.Html, 
            replyMarkup: inlineKeyboard,
            ct: ct);

        // Configurer le clavier persistant pour accès rapide
        var replyKeyboard = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton[] { $"{TelegramConstants.Emojis.Robot} Menu", $"{TelegramConstants.Emojis.Lightning} État", $"{TelegramConstants.Emojis.Lightning} Mini-App" },
            new KeyboardButton[] { $"{TelegramConstants.Emojis.Device} Capteurs", $"{TelegramConstants.Emojis.Chart} Graphiques" },
        })
        {
            ResizeKeyboard = true,
            IsPersistent = true
        };

        await telegram.SendToChatAsync(
            message.Chat.Id,
            $"{TelegramConstants.Emojis.Gear} <i>Clavier rapide activé ! Utilisez les boutons ci-dessous pour naviguer.</i>",
            ParseMode.Html,
            replyMarkup: replyKeyboard,
            ct: ct);
    }
}
