using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

using Kk.Kharts.Api.DependencyInjection;

namespace Kk.Kharts.Api.Services.Telegram;

/// <summary>
/// Implementação do serviço de Telegram usando o package oficial Telegram.Bot.
/// Suporta 2 bots: DebugBot (mensagens de sistema) e ClientBot (respostas a utilizadores).
/// </summary>
[SingletonService]
public sealed class TelegramService : ITelegramService
{
    private readonly TelegramBotClient _debugBotClient;
    private readonly TelegramBotClient _clientBotClient;
    private readonly TelegramOptions _options;
    private readonly ILogger<TelegramService> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;

    /// <summary>
    /// Bot para respostas aos utilizadores (ClientBot).
    /// </summary>
    public TelegramBotClient BotClient => _clientBotClient;

    public TelegramService(IOptions<TelegramOptions> options, ILogger<TelegramService> logger)
    {
        _options = options.Value;
        _logger = logger;
        
        // Debug Bot - para mensagens de sistema (startup, erros, alertas)
        _debugBotClient = new TelegramBotClient(_options.DebugBotToken);
        
        // Client Bot - para respostas aos utilizadores
        var clientToken = _options.ClientBotToken ?? _options.DebugBotToken;
        _clientBotClient = new TelegramBotClient(clientToken);

        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, timeSpan, retryCount, _) =>
                {
                    _logger.LogWarning(
                        exception,
                        "Telegram retry {RetryCount} after {Delay}s",
                        retryCount,
                        timeSpan.TotalSeconds);
                });
    }

    /// <summary>
    /// Envia mensagem para o canal de debug (usa DebugBot).
    /// Para mensagens de sistema, startup, erros, alertas.
    /// </summary>
    public async Task SendMessageAsync(
        string message,
        ParseMode parseMode = ParseMode.None,
        int? messageThreadId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        await _retryPolicy.ExecuteAsync(async () =>
        {
            // Usa DebugBot para mensagens de sistema
            await _debugBotClient.SendMessage(
                chatId: _options.DebugChatId,
                text: message,
                parseMode: parseMode,
                messageThreadId: messageThreadId,
                cancellationToken: cancellationToken);
        });

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Debug bot: message sent to topic {TopicId}", messageThreadId);
        }
    }

    /// <summary>
    /// Envia mensagem para um chat específico (usa ClientBot).
    /// Para respostas aos utilizadores.
    /// </summary>
    public async Task SendToChatAsync(
        long chatId,
        string message,
        ParseMode parseMode = ParseMode.Html,
        int? messageThreadId = null,
        ReplyMarkup? replyMarkup = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        await _retryPolicy.ExecuteAsync(async () =>
        {
            // Usa ClientBot para respostas aos utilizadores
            await _clientBotClient.SendMessage(
                chatId: chatId,
                text: message,
                parseMode: parseMode,
                messageThreadId: messageThreadId,
                replyMarkup: replyMarkup,
                cancellationToken: ct);
        });

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Client bot: message sent to chat {ChatId}", chatId);
        }
    }

    public async Task SendLongMessageAsync(
        string message,
        ParseMode parseMode = ParseMode.None,
        int? messageThreadId = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        var chunks = SplitMessage(message, _options.MaxMessageLength);

        foreach (var chunk in chunks)
        {
            await SendMessageAsync(chunk, parseMode, messageThreadId, cancellationToken);
            
            if (chunks.Count > 1)
                await Task.Delay(100, cancellationToken);
        }
    }

    /// <summary>
    /// Envia documento para o canal de debug (usa DebugBot).
    /// </summary>
    public async Task SendDocumentAsync(
        Stream documentStream,
        string fileName,
        string? caption = null,
        int? messageThreadId = null,
        CancellationToken cancellationToken = default)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            // Usa DebugBot para documentos de sistema
            await _debugBotClient.SendDocument(
                chatId: _options.DebugChatId,
                document: InputFile.FromStream(documentStream, fileName),
                caption: caption,
                messageThreadId: messageThreadId,
                cancellationToken: cancellationToken);
        });

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Debug bot: document '{FileName}' sent", fileName);
        }
    }

    public Task SendDocumentToCmdsTopicAsync(
        Stream documentStream,
        string fileName,
        string? caption = null,
        CancellationToken cancellationToken = default)
        => SendDocumentAsync(documentStream, fileName, caption, _options.CmdsTopicId, cancellationToken);

    /// <summary>
    /// Envia documento para um chat específico (usa ClientBot).
    /// </summary>
    public async Task SendDocumentToChatAsync(
        long chatId,
        Stream documentStream,
        string fileName,
        string? caption = null,
        CancellationToken cancellationToken = default)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            // Usa ClientBot para documentos aos utilizadores
            await _clientBotClient.SendDocument(
                chatId: chatId,
                document: InputFile.FromStream(documentStream, fileName),
                caption: caption,
                cancellationToken: cancellationToken);
        });

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Client bot: document '{FileName}' sent to chat {ChatId}", fileName, chatId);
        }
    }

    public Task SendToDebugTopicAsync(
        string message,
        ParseMode parseMode = ParseMode.Html,
        CancellationToken cancellationToken = default)
        => SendLongMessageAsync(message, parseMode, _options.DebugTopicId, cancellationToken);

    public Task SendToErrorsTopicAsync(
        string message,
        ParseMode parseMode = ParseMode.Html,
        CancellationToken cancellationToken = default)
        => SendLongMessageAsync(message, parseMode, _options.ErrorsTopicId, cancellationToken);

    public Task SendToDeviceStatusTopicAsync(
        string message,
        ParseMode parseMode = ParseMode.Html,
        CancellationToken cancellationToken = default)
        => SendLongMessageAsync(message, parseMode, _options.DeviceStatusTopicId, cancellationToken);

    public Task SendToCmdsTopicAsync(
        string message,
        ParseMode parseMode = ParseMode.Html,
        CancellationToken cancellationToken = default)
        => SendLongMessageAsync(message, parseMode, _options.CmdsTopicId, cancellationToken);

    public async Task<int?> SendToDeviceStatusTopicWithIdAsync( string message, ParseMode parseMode = ParseMode.Html, int? messageThreadId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(message))
            return null;

        var threadId = messageThreadId ?? _options.DeviceStatusTopicId;

        var sent = await _debugBotClient.SendMessage(
            chatId: _options.DebugChatId,
            text: message,
            parseMode: parseMode,
            messageThreadId: threadId,
            cancellationToken: cancellationToken);

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Client bot: message {MessageId} sent to device status topic", sent.MessageId);
        }
        return sent.MessageId;
    }

    public async Task DeleteFromDeviceStatusTopicAsync(int messageId, CancellationToken ct = default)
    {
        try
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                await _debugBotClient.DeleteMessage(
                    chatId: long.Parse(_options.DebugChatId),
                    messageId: messageId,
                    cancellationToken: ct);
            });

            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Debug bot: message {MessageId} deleted from device status topic", messageId);
            }
        }
        catch (ApiRequestException ex) when (ex.ErrorCode == 400 && ex.Message.Contains("message to delete not found", StringComparison.OrdinalIgnoreCase))
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Telegram already removed message {MessageId} from topic", messageId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete message {MessageId} from device status topic", messageId);
            throw;
        }
    }

    public Task SendToSupportTopicAsync(
        string message,
        ParseMode parseMode = ParseMode.Html,
        CancellationToken cancellationToken = default)
        => SendLongMessageAsync(message, parseMode, _options.SupportTopicId, cancellationToken);

    public Task SendToDoublonsTopicAsync(
        string message,
        ParseMode parseMode = ParseMode.Html,
        CancellationToken cancellationToken = default)
        => SendLongMessageAsync(message, parseMode, _options.DoublonsTopicId, cancellationToken);

    /// <summary>
    /// Envia foto para um chat específico (usa ClientBot).
    /// </summary>
    public async Task SendPhotoToChatAsync(
        long chatId,
        Stream photoStream,
        string? caption = null,
        ParseMode parseMode = ParseMode.Html,
        ReplyMarkup? replyMarkup = null,
        CancellationToken ct = default)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            // Usa ClientBot para fotos aos utilizadores
            await _clientBotClient.SendPhoto(
                chatId: chatId,
                photo: InputFile.FromStream(photoStream),
                caption: caption,
                parseMode: parseMode,
                replyMarkup: replyMarkup,
                cancellationToken: ct);
        });

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Client bot: photo sent to chat {ChatId}", chatId);
        }
    }

    /// <summary>
    /// Responde a callback query (usa ClientBot).
    /// </summary>
    public async Task AnswerCallbackQueryAsync(
        string callbackQueryId,
        string? text = null,
        bool showAlert = false,
        CancellationToken ct = default)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            // Usa ClientBot para callbacks de utilizadores
            await _clientBotClient.AnswerCallbackQuery(
                callbackQueryId: callbackQueryId,
                text: text,
                showAlert: showAlert,
                cancellationToken: ct);
        });
    }

    /// <summary>
    /// Edita mensagem num chat específico (usa ClientBot).
    /// </summary>
    public async Task EditMessageAsync(
        long chatId,
        int messageId,
        string newText,
        ParseMode parseMode = ParseMode.Html,
        ReplyMarkup? replyMarkup = null,
        CancellationToken ct = default)
    {
        await _retryPolicy.ExecuteAsync(async () =>
        {
            // Usa ClientBot para editar mensagens de utilizadores
            await _clientBotClient.EditMessageText(
                chatId: chatId,
                messageId: messageId,
                text: newText,
                parseMode: parseMode,
                replyMarkup: replyMarkup as InlineKeyboardMarkup,
                cancellationToken: ct);
        });

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            _logger.LogDebug("Client bot: message {MessageId} edited in chat {ChatId}", messageId, chatId);
        }
    }

    public string EscapeHtml(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }

    /// <summary>
    /// Apaga mensagem num chat específico (usa ClientBot).
    /// </summary>
    public async Task DeleteMessageAsync(long chatId, int messageId, CancellationToken ct = default)
    {
        try
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                // Usa ClientBot para apagar mensagens de utilizadores
                await _clientBotClient.DeleteMessage(chatId, messageId, ct);
            });
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug("Client bot: message {MessageId} deleted from chat {ChatId}", messageId, chatId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to delete message {MessageId} from chat {ChatId}", messageId, chatId);
        }
    }

    /// <summary>
    /// Configure les commandes du bot visibles dans le menu Telegram.
    /// </summary>
    public async Task SetBotCommandsAsync(CancellationToken ct = default)
    {
        var commands = new BotCommand[]
        {
            new() { Command = "start", Description = "Menu principal" },
            new() { Command = "aide", Description = "Message d'aide" },
            new() { Command = "help", Description = "Message d'aide" },
            new() { Command = "statut", Description = "État général du système" },
            new() { Command = "capteurs", Description = "Liste de vos capteurs" },
            new() { Command = "dernier", Description = "Dernière lecture d'un capteur" },
            new() { Command = "horsligne", Description = "Capteurs hors ligne" },
            new() { Command = "batterie", Description = "État des batteries" },
            new() { Command = "graphique", Description = "Graphique d'un capteur" },
            new() { Command = "alertes", Description = "Alertes actives" },
            new() { Command = "lier", Description = "Lier votre compte" },
            new() { Command = "delier", Description = "Délier votre compte" },
            new() { Command = "support", Description = "Contacter le support" },
            new() { Command = "app", Description = "Ouvrir l'application" },
        };

        try
        {
            await _clientBotClient.SetMyCommands(commands, cancellationToken: ct);
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Bot commands configured successfully ({Count} commands)", commands.Length);
            }

            // Configure le botão de menu para a Mini App
            var appUrl = "https://kropkontrol.com/miniapp/"; 
            await SetWebAppMenuButtonAsync("App", appUrl, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set bot commands");
        }
    }

    public async Task SetDebugBotCommandsAsync(CancellationToken ct = default)
    {
        var commands = new BotCommand[]
        {
            new() { Command = "help", Description = "Afficher l'aide" },
            new() { Command = "staff", Description = "Ouvrir KropKontrol Staff" },
            new() { Command = "miniappsetup", Description = "Configurer la Mini-App" },
            new() { Command = "last", Description = "Dernières données" },
            new() { Command = "lastseen", Description = "Capteurs silencieux" },
            new() { Command = "offline", Description = "Capteurs hors ligne" }
        };

        try
        {
            await _debugBotClient.SetMyCommands(commands, cancellationToken: ct);
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Debug bot commands configured successfully ({Count} commands)", commands.Length);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set debug bot commands");
        }
    }

    public async Task SetWebAppMenuButtonAsync(string text, string url, CancellationToken ct = default)
    {
        try
        {
            var menuButton = new MenuButtonWebApp
            {
                Text = text,
                WebApp = new WebAppInfo { Url = url }
            };

            await _clientBotClient.SetChatMenuButton(menuButton: menuButton, cancellationToken: ct);
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Bot menu button configured for Mini App: {Url}", url);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set bot menu button");
        }
    }

    public async Task SetDebugWebAppMenuButtonAsync(string text, string url, CancellationToken ct = default)
    {
        try
        {
            var menuButton = new MenuButtonWebApp
            {
                Text = text,
                WebApp = new WebAppInfo { Url = url }
            };

            await _debugBotClient.SetChatMenuButton(menuButton: menuButton, cancellationToken: ct);
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Debug bot menu button configured for Mini App: {Url}", url);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set debug bot menu button");
        }
    }

    private static List<string> SplitMessage(string message, int maxLength)
    {
        var chunks = new List<string>();
        var remaining = message.AsSpan();

        while (remaining.Length > 0)
        {
            var length = Math.Min(maxLength, remaining.Length);

            if (length < remaining.Length)
            {
                var lastNewLine = remaining[..length].LastIndexOf('\n');
                if (lastNewLine > maxLength / 2)
                    length = lastNewLine + 1;
            }

            chunks.Add(remaining[..length].ToString());
            remaining = remaining[length..];
        }

        return chunks;
    }
}
