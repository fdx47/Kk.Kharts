namespace Kk.Kharts.Api.Utils;

/// <summary>
/// Contrat minimal exposant un code d'erreur SQL pour simplifier les tests et extensions.
/// </summary>
public interface ISqlExceptionMetadata
{
    /// <summary>
    /// Numéro d'erreur SQL Server (ex.: 2627, 2601).
    /// </summary>
    int Number { get; }
}
