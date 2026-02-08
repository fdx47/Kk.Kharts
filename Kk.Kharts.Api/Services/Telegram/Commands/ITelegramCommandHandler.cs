using Telegram.Bot.Types;

namespace Kk.Kharts.Api.Services.Telegram.Commands;

/// <summary>
/// Interface base para handlers de comandos Telegram.
/// </summary>
public interface ITelegramCommandHandler
{
    /// <summary>
    /// Comando que este handler processa (ex: "/start", "/help").
    /// </summary>
    string Command { get; }

    /// <summary>
    /// Descrição do comando para o menu de ajuda.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Indica se o comando requer autenticação/autorização.
    /// </summary>
    bool RequiresAuth => false;

    /// <summary>
    /// Processa o comando recebido.
    /// </summary>
    Task HandleAsync(Message message, CancellationToken ct = default);
}

/// <summary>
/// Interface para handlers de callbacks de inline keyboards.
/// </summary>
public interface ITelegramCallbackHandler
{
    /// <summary>
    /// Prefixo do callback que este handler processa.
    /// </summary>
    string CallbackPrefix { get; }

    /// <summary>
    /// Processa o callback recebido.
    /// </summary>
    Task HandleAsync(CallbackQuery callbackQuery, CancellationToken ct = default);
}
