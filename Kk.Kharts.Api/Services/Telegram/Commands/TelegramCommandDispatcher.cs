using Kk.Kharts.Shared.Constants;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Kk.Kharts.Api.Services.Telegram.Commands;

/// <summary>
/// Dispatcher central para comandos e callbacks do Telegram.
/// Roteia mensagens para os handlers apropriados.
/// </summary>
public sealed class TelegramCommandDispatcher(
    IEnumerable<ITelegramCommandHandler> commandHandlers,
    IEnumerable<ITelegramCallbackHandler> callbackHandlers,
    ITelegramService telegramService,
    ILogger<TelegramCommandDispatcher> logger)
{
    private readonly Dictionary<string, ITelegramCommandHandler> _commands = 
        commandHandlers.ToDictionary(h => h.Command.ToLowerInvariant(), h => h);
    
    private readonly List<ITelegramCallbackHandler> _callbacks = callbackHandlers.ToList();

    /// <summary>
    /// Processa uma atualização do Telegram.
    /// </summary>
    public async Task ProcessUpdateAsync(Update update, CancellationToken ct = default)
    {
        try
        {
            switch (update.Type)
            {
                case UpdateType.Message when update.Message?.Text is not null:
                    await ProcessMessageAsync(update.Message, ct);
                    break;

                case UpdateType.CallbackQuery when update.CallbackQuery is not null:
                    await ProcessCallbackAsync(update.CallbackQuery, ct);
                    break;

                case UpdateType.EditedMessage:
                    logger.LogDebug("Message édité ignoré: {MessageId}", update.EditedMessage?.MessageId);
                    break;

                default:
                    logger.LogDebug("Type de update non supporté: {Type}", update.Type);
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Erreur lors du traitement du update {UpdateId}", update.Id);
            await NotifyErrorAsync(update, ex, ct);
        }
    }

    private async Task ProcessMessageAsync(Message message, CancellationToken ct)
    {
        var text = message.Text?.Trim() ?? string.Empty;
        
        if (!text.StartsWith('/'))
        {
            await HandleNonCommandMessageAsync(message, ct);
            return;
        }

        var parts = text.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var command = parts[0].ToLowerInvariant().Split('@')[0]; // Remove @botname si présent

        if (_commands.TryGetValue(command, out var handler))
        {
            logger.LogInformation("Commande {Command} de {User} ({ChatId})", 
                command, message.From?.Username ?? "unknown", message.Chat.Id);
            
            await handler.HandleAsync(message, ct);
        }
        else
        {
            await HandleUnknownCommandAsync(message, command, ct);
        }
    }

    private async Task ProcessCallbackAsync(CallbackQuery callback, CancellationToken ct)
    {
        var data = callback.Data ?? string.Empty;
        
        logger.LogInformation("Callback {Data} de {User}", 
            data, callback.From.Username ?? "unknown");

        var handler = _callbacks.FirstOrDefault(h => data.StartsWith(h.CallbackPrefix));
        
        if (handler is not null)
        {
            await handler.HandleAsync(callback, ct);
        }
        else
        {
            logger.LogWarning("Callback non reconnu: {Data}", data);
            await telegramService.AnswerCallbackQueryAsync(callback.Id, "⚠️ Action non reconnue", ct: ct);
        }
    }

    private async Task HandleNonCommandMessageAsync(Message message, CancellationToken ct)
    {
        var text = message.Text?.Trim() ?? string.Empty;

        // Gérer les boutons du clavier persistant
        var keyboardCommand = text switch
        {
            var t when t.Contains("Menu") => "/start",
            var t when t.Contains("État") => "/status",
            var t when t.Contains("Capteurs") => "/devices",
            var t when t.Contains("Graphiques") => "/chart",
            var t when t.Contains("App") => "/app",
            _ => null
        };

        if (keyboardCommand != null && _commands.TryGetValue(keyboardCommand, out var handler))
        {
            var fakeMessage = new Message
            {
                Chat = message.Chat,
                From = message.From,
                Text = keyboardCommand
            };
            await handler.HandleAsync(fakeMessage, ct);
            return;
        }

        // Réponse amicale pour les messages non reconnus
        var response = $"""
            {TelegramConstants.Emojis.Robot} Bonjour ! Je suis le bot KropKontrol.
            
            Utilisez les boutons du clavier ou tapez /help pour voir les commandes.
            """;

        await telegramService.SendToChatAsync(message.Chat.Id, response, ct: ct);
    }

    private async Task HandleUnknownCommandAsync(Message message, string command, CancellationToken ct)
    {
        var availableCommands = string.Join(", ", _commands.Keys.Take(5));
        
        var response = $"""
            {TelegramConstants.Emojis.Question} Commande <b>{telegramService.EscapeHtml(command)}</b> non reconnue.
            
            Commandes disponibles: {availableCommands}...
            
            Utilisez /help pour voir toutes les commandes.
            """;

        await telegramService.SendToChatAsync(message.Chat.Id, response, ParseMode.Html, ct: ct);
    }

    private async Task NotifyErrorAsync(Update update, Exception ex, CancellationToken ct)
    {
        var chatId = update.Message?.Chat.Id ?? update.CallbackQuery?.Message?.Chat.Id;
        
        if (chatId.HasValue)
        {
            var errorMsg = $"""
                {TelegramConstants.Emojis.Error} Une erreur s'est produite lors du traitement de votre demande.
                
                Veuillez réessayer plus tard ou contacter le support.
                """;

            try
            {
                await telegramService.SendToChatAsync(chatId.Value, errorMsg, ct: ct);
            }
            catch
            {
                // Ignorer les erreurs lors de l'envoi du message d'erreur
            }
        }

        // Notifier l'équipe de debug
        await telegramService.SendToDebugTopicAsync(
            $"❌ Erreur dans le bot:\n• Update: {update.Id}\n• Erreur: {ex.Message}", 
            ct: ct);
    }
}
