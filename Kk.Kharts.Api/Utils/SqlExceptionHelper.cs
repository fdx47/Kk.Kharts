using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Kk.Kharts.Api.Utils;

/// <summary>
/// Utility helpers around SQL Server exceptions so we can keep persistence layers small and explicit.
/// </summary>
public static class SqlExceptionHelper
{
    private static readonly HashSet<int> UniqueConstraintErrorCodes = new() { 2627, 2601 };

    /// <summary>
    /// Returns true when the supplied exception represents a unique constraint violation raised by SQL Server.
    /// Accepts either the real <see cref="SqlException"/> or any exception exposing <see cref="ISqlExceptionMetadata"/>
    /// (useful for deterministic unit tests sans SQL Server internals).
    /// </summary>
    public static bool IsUniqueConstraintViolation(DbUpdateException exception)
    {
        if (exception.InnerException is SqlException sqlException)
        {
            return UniqueConstraintErrorCodes.Contains(sqlException.Number);
        }

        if (exception.InnerException is ISqlExceptionMetadata metadata)
        {
            return UniqueConstraintErrorCodes.Contains(metadata.Number);
        }

        return false;
    }
}
