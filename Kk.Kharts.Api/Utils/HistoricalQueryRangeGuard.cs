using System.Globalization;

namespace Kk.Kharts.Api.Utils;

public static class HistoricalQueryRangeGuard
{
    public static readonly TimeSpan MaxRange = TimeSpan.FromDays(31);

    public static void ValidateOrThrow(DateTime startDate, DateTime endDate)
    {
        if (startDate == default || endDate == default)
        {
            throw new ArgumentException("Les paramètres startDate et endDate sont requis.");
        }

        var startUtc = startDate.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(startDate, DateTimeKind.Utc) : startDate.ToUniversalTime();
        var endUtc = endDate.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(endDate, DateTimeKind.Utc) : endDate.ToUniversalTime();

        if (endUtc < startUtc)
        {
            throw new ArgumentException("La date de fin doit être supérieure ou égale à la date de début.");
        }

        if ((endUtc - startUtc) > MaxRange)
        {
            throw new ArgumentException($"La plage maximale autorisée est de {(int)MaxRange.TotalDays} jours. Réduisez l'intervalle demandé.");
        }
    }
}
