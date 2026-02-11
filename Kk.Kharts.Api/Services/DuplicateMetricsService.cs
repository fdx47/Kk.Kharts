using Kk.Kharts.Api.DependencyInjection;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Utility.Constants;
using System.Collections.Concurrent;
using System.Globalization;

namespace Kk.Kharts.Api.Services
{
    /// <summary>
    /// Service de métriques de doublons basé sur des fichiers texte.
    /// Stocke les événements dans kklogs/doublons/ddMMyy.txt (un fichier par jour).
    /// Format par ligne : HH:mm:ss|DevEUI|DeviceName|CompanyName|Endpoint
    /// </summary>
    [SingletonService]
    public class DuplicateMetricsService : IDuplicateMetricsService
    {
        private const string DoublonsSubDir = "doublons";
        private const string DateFormat = "ddMMyy";
        private const char Separator = '|';

        private readonly string _doublonsDirectory;
        private readonly ILogger<DuplicateMetricsService> _logger;
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _fileLocks = new();

        public DuplicateMetricsService(ILogger<DuplicateMetricsService> logger)
        {
            _logger = logger;
            _doublonsDirectory = Path.Combine(AppContext.BaseDirectory, GlobalConstants.LogsDirectoryName, DoublonsSubDir);

            if (!Directory.Exists(_doublonsDirectory))
            {
                Directory.CreateDirectory(_doublonsDirectory);
                _logger.LogInformation("Répertoire doublons créé en {Directory}", _doublonsDirectory);
            }
        }

        public async Task RecordDuplicateAsync(string devEui, string? deviceName, string? companyName, string endpoint)
        {
            try
            {
                var now = DateTime.UtcNow;
                var fileName = now.ToString(DateFormat, CultureInfo.InvariantCulture) + ".txt";
                var filePath = Path.Combine(_doublonsDirectory, fileName);

                // Sanitize fields (remove separator char)
                var safeName = (deviceName ?? "N/A").Replace(Separator, '-');
                var safeCompany = (companyName ?? "N/A").Replace(Separator, '-');
                var safeEndpoint = endpoint.Replace(Separator, '-');

                var line = $"{now:HH:mm:ss}{Separator}{devEui}{Separator}{safeName}{Separator}{safeCompany}{Separator}{safeEndpoint}";

                // Thread-safe file write using per-file lock
                var fileLock = _fileLocks.GetOrAdd(filePath, _ => new SemaphoreSlim(1, 1));
                await fileLock.WaitAsync();
                try
                {
                    await File.AppendAllTextAsync(filePath, line + Environment.NewLine);
                }
                finally
                {
                    fileLock.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'enregistrement de la métrique de doublon pour {DevEui}", devEui);
            }
        }

        public async Task<DuplicateStatsResult> GetStatsAsync(DateOnly from, DateOnly to)
        {
            var entries = await ReadEntriesAsync(from, to);

            var topDevices = entries
                .GroupBy(e => e.DevEui)
                .Select(g => new DuplicateDeviceSummary
                {
                    DevEui = g.Key,
                    DeviceName = g.Last().DeviceName,
                    CompanyName = g.Last().CompanyName,
                    Count = g.Count()
                })
                .OrderByDescending(d => d.Count)
                .Take(20)
                .ToList();

            var dailyBreakdown = entries
                .GroupBy(e => e.Date)
                .OrderBy(g => g.Key)
                .Select(g => new DuplicateDailySummary
                {
                    Date = g.Key,
                    TotalCount = g.Count(),
                    Devices = g.GroupBy(e => e.DevEui)
                        .Select(dg => new DuplicateDeviceSummary
                        {
                            DevEui = dg.Key,
                            DeviceName = dg.Last().DeviceName,
                            CompanyName = dg.Last().CompanyName,
                            Count = dg.Count()
                        })
                        .OrderByDescending(d => d.Count)
                        .ToList()
                })
                .ToList();

            return new DuplicateStatsResult
            {
                From = from,
                To = to,
                TotalDuplicates = entries.Count,
                TopDevices = topDevices,
                DailyBreakdown = dailyBreakdown
            };
        }

        public async Task<DuplicateDeviceStatsResult> GetDeviceStatsAsync(string devEui, DateOnly from, DateOnly to)
        {
            var allEntries = await ReadEntriesAsync(from, to);
            var entries = allEntries
                .Where(e => e.DevEui.Equals(devEui, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var dailyBreakdown = entries
                .GroupBy(e => e.Date)
                .OrderBy(g => g.Key)
                .Select(g => new DuplicateDailyCount { Date = g.Key, Count = g.Count() })
                .ToList();

            var hourlyBreakdown = entries
                .GroupBy(e =>
                {
                    if (TimeOnly.TryParseExact(e.Time, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out var t))
                        return t.Hour;
                    return -1;
                })
                .Where(g => g.Key >= 0)
                .OrderBy(g => g.Key)
                .Select(g => new DuplicateHourlyCount { Hour = g.Key, Count = g.Count() })
                .ToList();

            return new DuplicateDeviceStatsResult
            {
                DevEui = devEui,
                DeviceName = entries.LastOrDefault()?.DeviceName,
                CompanyName = entries.LastOrDefault()?.CompanyName,
                From = from,
                To = to,
                TotalDuplicates = entries.Count,
                DailyBreakdown = dailyBreakdown,
                HourlyBreakdown = hourlyBreakdown
            };
        }

        public List<DateOnly> GetAvailableDates()
        {
            if (!Directory.Exists(_doublonsDirectory))
                return [];

            return Directory.EnumerateFiles(_doublonsDirectory, "*.txt")
                .Select(f => TryParseDate(Path.GetFileNameWithoutExtension(f)))
                .Where(d => d.HasValue)
                .Select(d => d!.Value)
                .OrderDescending()
                .ToList();
        }

        private async Task<List<DuplicateEntry>> ReadEntriesAsync(DateOnly from, DateOnly to)
        {
            var entries = new List<DuplicateEntry>();

            for (var date = from; date <= to; date = date.AddDays(1))
            {
                var fileName = date.ToString(DateFormat, CultureInfo.InvariantCulture) + ".txt";
                var filePath = Path.Combine(_doublonsDirectory, fileName);

                if (!File.Exists(filePath))
                    continue;

                try
                {
                    var lines = await File.ReadAllLinesAsync(filePath);
                    foreach (var line in lines)
                    {
                        var entry = ParseLine(line, date);
                        if (entry != null)
                            entries.Add(entry);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Erreur lors de la lecture du fichier de doublons {File}", filePath);
                }
            }

            return entries;
        }

        private static DuplicateEntry? ParseLine(string line, DateOnly date)
        {
            if (string.IsNullOrWhiteSpace(line))
                return null;

            var parts = line.Split(Separator);
            if (parts.Length < 5)
                return null;

            return new DuplicateEntry
            {
                Time = parts[0],
                DevEui = parts[1],
                DeviceName = parts[2] == "N/A" ? null : parts[2],
                CompanyName = parts[3] == "N/A" ? null : parts[3],
                Endpoint = parts[4],
                Date = date
            };
        }

        private static DateOnly? TryParseDate(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return null;

            if (DateOnly.TryParseExact(fileName, DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date;

            return null;
        }
    }
}
