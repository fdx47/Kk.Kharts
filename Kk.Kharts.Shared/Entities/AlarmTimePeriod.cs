namespace Kk.Kharts.Shared.Entities;

/// <summary>
/// Représente une période horaire avec des seuils d'alarme spécifiques.
/// Permet de définir jusqu'à 8 périodes par jour avec des seuils différents.
/// </summary>
public class AlarmTimePeriod
{
    public int Id { get; set; }

    /// <summary>
    /// Référence vers la règle d'alarme parente.
    /// </summary>
    public int AlarmRuleId { get; set; }
    public AlarmRule? AlarmRule { get; set; }

    /// <summary>
    /// Nom descriptif de la période (ex: "Nuit", "Matin", "Après-midi").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Heure de début de la période (ex: 06:00).
    /// </summary>
    public TimeSpan StartTime { get; set; }

    /// <summary>
    /// Heure de fin de la période (ex: 12:00).
    /// </summary>
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// Seuil bas spécifique à cette période.
    /// </summary>
    public float? LowValue { get; set; }

    /// <summary>
    /// Seuil haut spécifique à cette période.
    /// </summary>
    public float? HighValue { get; set; }

    /// <summary>
    /// Indique si cette période est active.
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Ordre d'affichage (1-8).
    /// </summary>
    public int DisplayOrder { get; set; }

    /// <summary>
    /// Vérifie si l'heure actuelle est dans cette période.
    /// </summary>
    public bool IsCurrentlyActive()
    {
        var now = DateTime.UtcNow.TimeOfDay;
        
        // Gère le cas où la période traverse minuit (ex: 22:00 - 06:00)
        if (EndTime < StartTime)
        {
            return now >= StartTime || now < EndTime;
        }
        
        return now >= StartTime && now < EndTime;
    }

    /// <summary>
    /// Vérifie si une heure donnée est dans cette période.
    /// </summary>
    public bool IsTimeInPeriod(TimeSpan time)
    {
        if (EndTime < StartTime)
        {
            return time >= StartTime || time < EndTime;
        }
        
        return time >= StartTime && time < EndTime;
    }
}
