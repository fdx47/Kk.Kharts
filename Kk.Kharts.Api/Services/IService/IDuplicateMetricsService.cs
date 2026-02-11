namespace Kk.Kharts.Api.Services.IService
{
    public interface IDuplicateMetricsService
    {
        /// <summary>
        /// Enregistre un événement de doublon dans le fichier du jour.
        /// </summary>
        Task RecordDuplicateAsync(string devEui, string? deviceName, string? companyName, string endpoint);

        /// <summary>
        /// Récupère les statistiques de doublons pour une période donnée.
        /// </summary>
        Task<DuplicateStatsResult> GetStatsAsync(DateOnly from, DateOnly to);

        /// <summary>
        /// Récupère les statistiques de doublons pour un capteur spécifique sur une période.
        /// </summary>
        Task<DuplicateDeviceStatsResult> GetDeviceStatsAsync(string devEui, DateOnly from, DateOnly to);

        /// <summary>
        /// Liste les fichiers de métriques disponibles.
        /// </summary>
        List<DateOnly> GetAvailableDates();
    }

    public class DuplicateEntry
    {
        public required string Time { get; init; }
        public required string DevEui { get; init; }
        public string? DeviceName { get; init; }
        public string? CompanyName { get; init; }
        public required string Endpoint { get; init; }
        public required DateOnly Date { get; init; }
    }

    public class DuplicateDeviceSummary
    {
        public required string DevEui { get; init; }
        public string? DeviceName { get; init; }
        public string? CompanyName { get; init; }
        public int Count { get; init; }
    }

    public class DuplicateDailySummary
    {
        public required DateOnly Date { get; init; }
        public int TotalCount { get; init; }
        public required List<DuplicateDeviceSummary> Devices { get; init; }
    }

    public class DuplicateStatsResult
    {
        public required DateOnly From { get; init; }
        public required DateOnly To { get; init; }
        public int TotalDuplicates { get; init; }
        public required List<DuplicateDeviceSummary> TopDevices { get; init; }
        public required List<DuplicateDailySummary> DailyBreakdown { get; init; }
    }

    public class DuplicateDeviceStatsResult
    {
        public required string DevEui { get; init; }
        public string? DeviceName { get; init; }
        public string? CompanyName { get; init; }
        public required DateOnly From { get; init; }
        public required DateOnly To { get; init; }
        public int TotalDuplicates { get; init; }
        public required List<DuplicateDailyCount> DailyBreakdown { get; init; }
        public required List<DuplicateHourlyCount> HourlyBreakdown { get; init; }
    }

    public class DuplicateDailyCount
    {
        public required DateOnly Date { get; init; }
        public int Count { get; init; }
    }

    public class DuplicateHourlyCount
    {
        public int Hour { get; init; }
        public int Count { get; init; }
    }
}
