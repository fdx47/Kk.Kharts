namespace Kk.Kharts.Api.Services.Telegram;

/// <summary>
/// Configurações dos Telegram Bots carregadas do appsettings.json.
/// Suporta 2 bots separados: DebugBot (interno) e ClientBot (utilizadores).
/// </summary>
public sealed class TelegramOptions
{
    public const string SectionName = "Telegram";

    // ===========================================================================
    //                       DEBUG BOT
    //
    // (KropKontrolDEV) - Bot interno para equipa
    // Envia: Alertas de sistema, erros, status de dispositivos, startup
    // Recebe: Comandos administrativos (/last, /offline, /createuserdemo)
    //
    // ============================================================================
    
    /// <summary>
    /// Token do Bot Debug (@KropKontrolDEV_bot) - uso interno.
    /// </summary>
    public required string DebugBotToken { get; init; }

    /// <summary>
    /// ID do grupo/canal para mensagens do bot debug.
    /// </summary>
    public required string DebugChatId { get; init; }

    /// <summary>
    /// Secret para validação do webhook do bot debug.
    /// </summary>
    public string? DebugWebhookSecret { get; init; }

    /// <summary>
    /// ID do tópico para erros SDI-12.
    /// </summary>
    public int ErrorsTopicId { get; init; }

    /// <summary>
    /// ID do tópico para debug/logs.
    /// </summary>
    public int DebugTopicId { get; init; }

    /// <summary>
    /// ID do tópico (thread) dentro do chat para status de dispositivos.
    /// </summary>
    public int DeviceStatusTopicId { get; init; }

    /// <summary>
    /// ID do tópico para mensagens de suporte dos utilizadores.
    /// </summary>
    public int SupportTopicId { get; init; }

    /// <summary>
    /// ID do tópico para comandos/ficheiros administrativos ("cmds bots").
    /// </summary>
    public int CmdsTopicId { get; init; }

    /// <summary>
    /// ID do tópico telegram para mensagens de duplicados (doublons) dos dispositivos.
    /// </summary>
    public int DoublonsTopicId { get; init; }



    // =========================================================================
    //
    //                           CLIENT BOT
    //
    //     CLIENT BOT (kropkontrol_bot) - Bot para utilizadores finais
    //              Envia: Respostas aos comandos do utilizador
    //     Recebe: Comandos de utilizador (/start, /devices, /chart, /link)
    //
    // =========================================================================


    /// <summary>
    /// Token do Bot Cliente (@kropkontrol_bot) - utilizadores finais.
    /// </summary>
    public string? ClientBotToken { get; init; }

    /// <summary>
    /// Secret para validação do webhook do bot cliente.
    /// </summary>
    public string? ClientWebhookSecret { get; init; }



    // ============================================================
    // CONFIGURAÇÕES COMUNS
    // ============================================================

    /// <summary>
    /// Tamanho máximo de mensagem (Telegram aceita até 4096).
    /// </summary>
    public int MaxMessageLength { get; init; } = 4000;
}
