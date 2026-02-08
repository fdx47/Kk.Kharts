namespace Kk.Kharts.Shared.Constants;

/// <summary>
/// Constantes JWT partilhadas entre todos os projetos da solução.
/// Centraliza Issuer, Audience e outras configurações de autenticação.
/// </summary>
public static class JwtConstants
{
    /// <summary>
    /// Issuer do token JWT - identifica quem emitiu o token.
    /// </summary>
    public const string Issuer = "kk.kharts.server";

    /// <summary>
    /// Audience do token JWT - identifica para quem o token foi emitido.
    /// </summary>
    public const string Audience = "kk.kharts.client";

    /// <summary>
    /// Nome do header para API Key.
    /// </summary>
    public const string ApiKeyHeaderName = "KropKontrol";

    /// <summary>
    /// Schema de autenticação Bearer.
    /// </summary>
    public const string BearerScheme = "Bearer";
}
