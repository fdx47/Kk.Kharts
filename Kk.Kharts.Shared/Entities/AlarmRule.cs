using System.ComponentModel.DataAnnotations.Schema;

namespace Kk.Kharts.Shared.Entities
{
    public class AlarmRule
    {
        public int Id { get; set; }
        public string? DevEui { get; set; }
        public int DeviceId { get; set; }       
        public string Description { get; set; } = "";
        public string PropertyName { get; set; } = "";
        
        /// <summary>
        /// Seuil bas par défaut (utilisé si aucune période n'est active).
        /// </summary>
        public float? LowValue { get; set; }
        
        /// <summary>
        /// Seuil haut par défaut (utilisé si aucune période n'est active).
        /// </summary>
        public float? HighValue { get; set; }
        public float? Hysteresis { get; set; }
        public bool Enabled { get; set; } = true;
        public int? DeviceModel { get; set; }
        public bool IsAlarmActive { get; set; } = false;
        public bool IsAlarmHandled { get; set; }
        public string? ActiveThresholdType { get; set; }

        /// <summary>
        /// Indique si cette règle utilise des périodes horaires.
        /// Si false, utilise LowValue/HighValue par défaut.
        /// </summary>
        public bool UseTimePeriods { get; set; } = false;

        public Device Device { get; set; } = default!;
        public List<UserAlarmRule> UserAlarmRules { get; set; } = new();
        
        /// <summary>
        /// Périodes horaires avec seuils spécifiques (max 8).
        /// </summary>
        public List<AlarmTimePeriod> TimePeriods { get; set; } = new();

        /// <summary>
        /// Obtient les seuils actifs en fonction de l'heure actuelle.
        /// </summary>
        public (float? Low, float? High) GetActiveThresholds()
        {
            if (!UseTimePeriods || TimePeriods.Count == 0)
            {
                return (LowValue, HighValue);
            }

            var activePeriod = TimePeriods
                .Where(p => p.IsEnabled)
                .FirstOrDefault(p => p.IsCurrentlyActive());

            if (activePeriod != null)
            {
                return (activePeriod.LowValue, activePeriod.HighValue);
            }

            return (LowValue, HighValue);
        }
    }
}
