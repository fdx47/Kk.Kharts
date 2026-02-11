public static class HumanReadableTimeExtensions
{

    public static string ToHumanReadableTime(this DateTime dateTime)
    {
        var now = DateTime.UtcNow;
        var isFuture = dateTime > now;
        var diff = isFuture ? dateTime - now : now - dateTime;
        var seconds = (int)Math.Floor(diff.TotalSeconds);
        var minutes = (int)Math.Floor(diff.TotalMinutes);
        var hours = (int)Math.Floor(diff.TotalHours);
        var days = (int)Math.Floor(diff.TotalDays);
        var months = (int)Math.Floor(diff.TotalDays / 30);
        var years = (int)Math.Floor(diff.TotalDays / 365);

        string Format(string value, string singular, string plural)
        {
            var unit = value == "1" ? singular : plural;
            return isFuture
                ? $"Dans {value} {unit}"
                : $"Il y a {value} {unit}";
        }

        if (seconds < 60) return Format(seconds.ToString(), "seconde", "secondes");
        if (minutes < 2) return isFuture ? "Dans 1 minute" : "Il y a 1 minute";
        if (minutes < 60) return Format(minutes.ToString(), "minute", "minutes");
        if (hours < 2) return isFuture ? "Dans 1 heure" : "Il y a 1 heure";
        if (hours < 24) return Format(hours.ToString(), "heure", "heures");
        if (days < 2) return isFuture ? "Dans 1 jour" : "Il y a 1 jour";
        if (days < 30) return Format(days.ToString(), "jour", "jours");
        if (months < 2) return isFuture ? "Dans 1 mois" : "Il y a 1 mois";
        if (months < 12) return Format(months.ToString(), "mois", "mois");
        if (years < 2) return isFuture ? "Dans 1 an" : "Il y a 1 an";

        return Format(years.ToString(), "an", "ans");
    }


    public static string ToHumanizeLastSentAt(this string lastSentAt)
    {
        if (string.IsNullOrWhiteSpace(lastSentAt))
        {
            return "Jamais";
        }

        var normalized = lastSentAt.Replace(" GMT", string.Empty, StringComparison.OrdinalIgnoreCase);

        return DateTimeOffset.TryParse(normalized, out var parsed)
            ? parsed.UtcDateTime.ToHumanReadableTime()
            : "Date invalide";
    }
}
