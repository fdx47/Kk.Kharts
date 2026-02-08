using Kk.Kharts.Api.Utils;

namespace Kk.Kharts.Api.Tests.Helpers;

/// <summary>
/// Exception simulée implémentant <see cref="ISqlExceptionMetadata"/> pour tester les scénarios d'idempotence sans refléter <see cref="SqlException"/>.
/// </summary>
public sealed class FakeSqlException : Exception, ISqlExceptionMetadata
{
    public FakeSqlException(int number) : base($"Erreur SQL simulée {number}")
    {
        Number = number;
    }

    public int Number { get; }
}
