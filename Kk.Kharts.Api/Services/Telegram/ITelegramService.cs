using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Kk.Kharts.Api.Services.Telegram;

/// <summary>
/// Interface para o serviço de Telegram usando o package Telegram.Bot.
/// </summary>
public interface ITelegramService
{
    /// <summary>
    /// Envia uma mensagem de texto para o chat configurado.
    /// </summary>
    Task SendMessageAsync(
        string message,
        ParseMode parseMode = ParseMode.None,
        int? messageThreadId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Envia uma mensagem para um chat específico com suporte a inline keyboards.
    /// </summary>
    Task SendToChatAsync(
        long chatId,
        string message,
        ParseMode parseMode = ParseMode.Html,
        int? messageThreadId = null,
        ReplyMarkup? replyMarkup = null,
        CancellationToken ct = default);

    /// <summary>
    /// Envia uma mensagem longa dividida em chunks.
    /// </summary>
    Task SendLongMessageAsync(
        string message,
        ParseMode parseMode = ParseMode.None,
        int? messageThreadId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Envia um documento/ficheiro para o chat configurado.
    /// </summary>
    Task SendDocumentAsync(
        Stream documentStream,
        string fileName,
        string? caption = null,
        int? messageThreadId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Envia uma mensagem longa para o tópico de comandos (cmds bots).
    /// </summary>
    Task SendToCmdsTopicAsync(
        string message,
        ParseMode parseMode = ParseMode.Html,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Envia um documento diretamente para o tópico de comandos (cmds bots).
    /// </summary>
    Task SendDocumentToCmdsTopicAsync(
        Stream documentStream,
        string fileName,
        string? caption = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Envia um documento para um chat específico.
    /// </summary>
    Task SendDocumentToChatAsync(
        long chatId,
        Stream documentStream,
        string fileName,
        string? caption = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Envia uma imagem para um chat específico.
    /// </summary>
    Task SendPhotoToChatAsync(
        long chatId,
        Stream photoStream,
        string? caption = null,
        ParseMode parseMode = ParseMode.Html,
        ReplyMarkup? replyMarkup = null,
        CancellationToken ct = default);

    /// <summary>
    /// Envia uma mensagem para o chat de debug (usa DebugBot).
    /// </summary>
    Task SendToDebugTopicAsync(string message, ParseMode parseMode = ParseMode.Html, CancellationToken ct = default);

    /// <summary>
    /// Envia mensagem para o tópico de Erros SDI-12.
    /// </summary>
    Task SendToErrorsTopicAsync(string message, ParseMode parseMode = ParseMode.Html, CancellationToken ct = default);

    /// <summary>
    /// Envia mensagem para o tópico de Status de Dispositivos.
    /// </summary>
    Task SendToDeviceStatusTopicAsync(string message, ParseMode parseMode = ParseMode.Html, CancellationToken ct = default);

    /// <summary>
    /// Envia mensagem para o tópico de Status de Dispositivos e devolve o messageId.
    /// </summary>
    Task<int?> SendToDeviceStatusTopicWithIdAsync(string message, ParseMode parseMode = ParseMode.Html, int? messageThreadId = null, CancellationToken ct = default);

    /// <summary>
    /// Apaga uma mensagem do tópico de Status de Dispositivos.
    /// </summary>
    Task DeleteFromDeviceStatusTopicAsync(int messageId, CancellationToken ct = default);

    /// <summary>
    /// Envia mensagem para o tópico de Support.
    /// </summary>
    Task SendToSupportTopicAsync(string message, ParseMode parseMode = ParseMode.Html, CancellationToken ct = default);

    /// <summary>
    /// Envia mensagem para o tópico de Duplicados (Doublons).
    /// </summary>
    Task SendToDoublonsTopicAsync(string message, ParseMode parseMode = ParseMode.Html, CancellationToken ct = default);

    /// <summary>
    /// Responde a um callback query.
    /// </summary>
    Task AnswerCallbackQueryAsync(
        string callbackQueryId,
        string? text = null,
        bool showAlert = false,
        CancellationToken ct = default);

    /// <summary>
    /// Edita uma mensagem existente.
    /// </summary>
    Task EditMessageAsync(
        long chatId,
        int messageId,
        string newText,
        ParseMode parseMode = ParseMode.Html,
        ReplyMarkup? replyMarkup = null,
        CancellationToken ct = default);

    /// <summary>
    /// Escapa caracteres especiais para HTML.
    /// </summary>
    string EscapeHtml(string? text);

    /// <summary>
    /// Supprime un message.
    /// </summary>
    Task DeleteMessageAsync(long chatId, int messageId, CancellationToken ct = default);

    /// <summary>
    /// Configure les commandes du bot visibles dans le menu.
    /// </summary>
    Task SetBotCommandsAsync(CancellationToken ct = default);

    /// <summary>
    /// Configure les commandes du bot de debug (administration interne).
    /// </summary>
    Task SetDebugBotCommandsAsync(CancellationToken ct = default);

    /// <summary>
    /// Configure le bouton de menu pour ouvrir la Mini App.
    /// </summary>
    Task SetWebAppMenuButtonAsync(string text, string url, CancellationToken ct = default);

    /// <summary>
    /// Configure le bouton de menu pour la Mini App sur le bot de debug.
    /// </summary>
    Task SetDebugWebAppMenuButtonAsync(string text, string url, CancellationToken ct = default);
}
