using Kk.Kharts.Shared.Constants;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Kk.Kharts.Api.Services.Telegram.Commands.Handlers;

/// <summary>
/// Handler para o comando /app - Abre a Mini App no Telegram.
/// </summary>
public sealed class AppCommandHandler(ITelegramService telegram) : ITelegramCommandHandler
{
    public string Command => TelegramConstants.Commands.App;
    public string Description => "Ouvrir l'application KropKontrol";

    public async Task HandleAsync(Message message, CancellationToken ct = default)
    {
        var appUrl = "https://kropkontrol.com/miniapp/";
        
        // Criar botão para abrir a Mini App
        var inlineKeyboard = new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithWebApp(
                    "🚀 Ouvrir KropKontrol",
                    new WebAppInfo { Url = appUrl }
                )
            }
        });

        var response = $"""
            {TelegramConstants.Emojis.Rocket} <b>KropKontrol Mini App</b>
            
            Accédez à la version Mini App de KropKontrol avec:
            • Dashboard interactif
            • Graphiques en temps réel
            • Carte des capteurs
            • Configuration avancée
            • Historique détaillé
            
            {TelegramConstants.Emojis.PointRight} Cliquez sur le bouton ci-dessous pour ouvrir l'application!
            """;

        await telegram.SendToChatAsync(
            message.Chat.Id,
            response,
            ParseMode.Html,
            replyMarkup: inlineKeyboard,
            ct: ct
        );
    }
}
