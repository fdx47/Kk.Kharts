using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Text.RegularExpressions;
using Kk.Kharts.Api.Utility.Constants;

namespace Kk.Kharts.Api.Controllers;

/// <summary>
/// Contrôleur pour la gestion des logs système.
/// Permet de récupérer les logs structurés et de télécharger les fichiers bruts.
/// Nécessite le rôle Root pour l'accès.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize(Roles = Roles.Root)]
public class LogsController : ControllerBase
{
    private const int DefaultLimit = 500;
    private const int MaxLimit = 2000;

    private readonly ILogger<LogsController> _logger;
    private readonly string _logsDirectory;

    public LogsController(ILogger<LogsController> logger)
    {
        _logger = logger;
        // Usar o mesmo diretório do DebugBotController
        _logsDirectory = Path.Combine(AppContext.BaseDirectory, GlobalConstants.LogsDirectoryName);

        if (!Directory.Exists(_logsDirectory))
        {
            Directory.CreateDirectory(_logsDirectory);
            _logger.LogInformation("Diretório kklogs criado em {Directory}", _logsDirectory);
        }
    }

    /// <summary>
    /// Récupère les logs système au format JSON structuré.
    /// </summary>
    /// <param name="date">Date des logs au format yyyy-MM-dd (optionnel). Si non spécifié, retourne les logs les plus récents.</param>
    /// <param name="limit">Nombre maximum d'entrées à retourner (défaut: 500, maximum: 2000).</param>
    /// <returns>Objet contenant les métadonnées du fichier et les entrées de logs paginées.</returns>
    [HttpGet]
    public IActionResult GetLogs([FromQuery] string? date = null, [FromQuery] int? limit = null)
    {
        try
        {
            var resolution = ResolveLogFile(date);
            if (resolution is null)
            {
                return NotFound(new { message = "Nenhum arquivo de log encontrado" });
            }

            if (!System.IO.File.Exists(resolution.Path))
            {
                return NotFound(new { message = $"Arquivo de logs não encontrado para a data: {date ?? resolution.DateDisplay}" });
            }

            var cappedLimit = Math.Clamp(limit ?? DefaultLimit, 50, MaxLimit);
            var entries = ReadLogEntries(resolution.Path, cappedLimit, out var totalEntries);
            var fileInfo = new FileInfo(resolution.Path);

            var response = new LogResponseDto
            {
                Date = resolution.DateDisplay,
                FileName = fileInfo.Name,
                FileSizeBytes = fileInfo.Length,
                LastModifiedUtc = fileInfo.LastWriteTimeUtc,
                TotalEntries = totalEntries,
                ReturnedEntries = entries.Count,
                HasMore = totalEntries > entries.Count,
                Entries = entries
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar logs estruturados");
            return StatusCode(500, new { message = "Erro interno ao buscar logs", error = ex.Message });
        }
    }

    /// <summary>
    /// Récupère tous les logs avec streaming transparent (pagination côté serveur).
    /// </summary>
    /// <param name="date">Date des logs au format yyyy-MM-dd (optionnel).</param>
    /// <param name="page">Numéro de la page (défaut: 1).</param>
    /// <param name="pageSize">Taille de la page (défaut: 500, max: 5000).</param>
    /// <returns>Logs paginés avec métadonnées complètes.</returns>
    [HttpGet("stream")]
    public async Task<IActionResult> StreamLogs([FromQuery] string? date = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 500)
    {
        try
        {
            var resolution = ResolveLogFile(date);
            if (resolution is null)
            {
                return NotFound(new { message = "Nenhum arquivo de log encontrado" });
            }

            if (!System.IO.File.Exists(resolution.Path))
            {
                return NotFound(new { message = $"Arquivo de logs não encontrado para a data: {date ?? resolution.DateDisplay}" });
            }

            var cappedPageSize = Math.Clamp(pageSize, 100, 5000);
            var allLines = new List<string>();
            var totalEntries = 0;

            // Ler todas as linhas primeiro para obter contagem total
            foreach (var line in System.IO.File.ReadLines(resolution.Path))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    totalEntries++;
                }
            }

            // Calcular skip/take para paginação
            var skip = (page - 1) * cappedPageSize;
            var take = Math.Min(cappedPageSize, totalEntries - skip);

            // Ler apenas a página solicitada
            var lines = System.IO.File.ReadLines(resolution.Path)
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Skip(skip)
                .Take(take)
                .Select(line => ParseLogLine(line.Trim()))
                .ToList();

            var fileInfo = new FileInfo(resolution.Path);

            var response = new LogStreamResponseDto
            {
                Date = resolution.DateDisplay,
                FileName = fileInfo.Name,
                FileSizeBytes = fileInfo.Length,
                LastModifiedUtc = fileInfo.LastWriteTimeUtc,
                TotalEntries = totalEntries,
                CurrentPage = page,
                PageSize = cappedPageSize,
                ReturnedEntries = lines.Count,
                HasNextPage = (skip + take) < totalEntries,
                HasPreviousPage = page > 1,
                TotalPages = (int)Math.Ceiling((double)totalEntries / cappedPageSize),
                Entries = lines
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du streaming des logs");
            return StatusCode(500, new { message = "Erreur interne lors du streaming des logs", error = ex.Message });
        }
    }

    /// <summary>
    /// Télécharge le fichier de logs brut au format .txt.
    /// </summary>
    /// <param name="date">Date des logs au format yyyy-MM-dd (optionnel). Si non spécifié, télécharge le fichier le plus récent.</param>
    /// <returns>Fichier .txt des logs en streaming.</returns>
    [HttpGet("download")]
    public IActionResult DownloadLog([FromQuery] string? date = null)
    {
        try
        {
            var resolution = ResolveLogFile(date);
            if (resolution is null)
            {
                return NotFound(new { message = "Nenhum arquivo de log encontrado" });
            }

            if (!System.IO.File.Exists(resolution.Path))
            {
                return NotFound(new { message = $"Arquivo de logs não encontrado para a data: {date ?? resolution.DateDisplay}" });
            }

            var stream = new FileStream(resolution.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileName = Path.GetFileName(resolution.Path);
            _logger.LogInformation("Iniciando download do log {FileName}", fileName);
            return File(stream, "text/plain", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao preparar download do log");
            return StatusCode(500, new { message = "Erro interno ao preparar download do log", error = ex.Message });
        }
    }

    private LogFileResolution? ResolveLogFile(string? dateParam)
    {
        if (string.IsNullOrWhiteSpace(dateParam))
        {
            var mostRecent = GetMostRecentLogFile();
            if (mostRecent is null)
            {
                return null;
            }

            return new LogFileResolution
            {
                Path = mostRecent,
                Date = TryParseDateFromFileName(Path.GetFileNameWithoutExtension(mostRecent))
            };
        }

        if (DateOnly.TryParseExact(dateParam, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsedDate))
        {
            return new LogFileResolution
            {
                Path = GetLogFilePath(parsedDate),
                Date = parsedDate
            };
        }

        throw new ArgumentException("Formato de data inválido. Use yyyy-MM-dd", nameof(dateParam));
    }

    private List<LogEntryDto> ReadLogEntries(string logFilePath, int maxEntries, out int totalEntries)
    {
        var entries = new List<LogEntryDto>(maxEntries);
        totalEntries = 0;

        foreach (var line in System.IO.File.ReadLines(logFilePath))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            totalEntries++;

            if (entries.Count >= maxEntries)
            {
                continue;
            }

            entries.Add(ParseLogLine(line.Trim()));
        }

        return entries;
    }

    private LogEntryDto ParseLogLine(string line)
    {
        var patterns = new[]
        {
            new Regex(@"^(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})\s+(\w+)\s+\[([^\]]+)\]\s+(.+)$"),
            new Regex(@"^\[(\w+)\]\s+([^:]+):\s+(.+)\s*-\s*(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})$"),
            new Regex(@"^(\d{2}:\d{2}:\d{2})\s+\[([^\]]+)\]\s+(\w+):\s+(.+)$"),
            new Regex(@"^(\w+)\s+(\w+)\s+(.+)\s+(\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2})$"),
            new Regex(@"^(\w+)\s+(\w+)\s+(.+)\s+(\d{2}:\d{2}:\d{2})$"),
            new Regex(@"^(.+)$")
        };

        string timestamp = string.Empty;
        string level = "INFO";
        string source = "UNKNOWN";
        string message = line;

        foreach (var pattern in patterns)
        {
            var match = pattern.Match(line);
            if (!match.Success)
            {
                continue;
            }

            if (pattern == patterns[0])
            {
                timestamp = match.Groups[1].Value;
                level = match.Groups[2].Value;
                source = match.Groups[3].Value;
                message = match.Groups[4].Value;
            }
            else if (pattern == patterns[1])
            {
                level = match.Groups[1].Value;
                source = match.Groups[2].Value;
                message = match.Groups[3].Value;
                timestamp = match.Groups[4].Value;
            }
            else if (pattern == patterns[2])
            {
                timestamp = $"{DateTime.UtcNow:yyyy-MM-dd} {match.Groups[1].Value}";
                source = match.Groups[2].Value;
                level = match.Groups[3].Value;
                message = match.Groups[4].Value;
            }
            else if (pattern == patterns[3])
            {
                level = match.Groups[1].Value;
                source = match.Groups[2].Value;
                message = match.Groups[3].Value;
                timestamp = match.Groups[4].Value;
            }
            else if (pattern == patterns[4])
            {
                level = match.Groups[1].Value;
                source = match.Groups[2].Value;
                message = match.Groups[3].Value;
                timestamp = $"{DateTime.UtcNow:yyyy-MM-dd} {match.Groups[4].Value}";
            }
            else
            {
                message = match.Groups[1].Value;
                timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
            }

            break;
        }

        return new LogEntryDto
        {
            Timestamp = timestamp,
            Level = level.ToUpperInvariant(),
            Source = source.Trim(),
            Message = message.Trim()
        };
    }

    /// <summary>
    /// Retourne des statistiques analytiques détaillées extraites des fichiers de logs.
    /// Inclut l'activité par utilisateur, par endpoint, par méthode HTTP, et par heure.
    /// </summary>
    /// <param name="date">Date au format yyyy-MM-dd (optionnel).</param>
    /// <returns>Objet analytique complet.</returns>
    [HttpGet("analytics")]
    public IActionResult GetAnalytics([FromQuery] string? date = null)
    {
        try
        {
            var resolution = ResolveLogFile(date);
            if (resolution is null)
                return NotFound(new { message = "Aucun fichier de log trouvé." });

            if (!System.IO.File.Exists(resolution.Path))
                return NotFound(new { message = $"Fichier de logs introuvable pour la date : {date ?? resolution.DateDisplay}" });

            var lines = System.IO.File.ReadAllLines(resolution.Path);
            var analytics = BuildAnalytics(lines, resolution.DateDisplay);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du calcul des analytics");
            return StatusCode(500, new { message = "Erreur interne", error = ex.Message });
        }
    }

    /// <summary>
    /// Retourne la liste des dates disponibles dans le répertoire de logs.
    /// </summary>
    [HttpGet("available-dates")]
    public IActionResult GetAvailableDates()
    {
        try
        {
            if (!Directory.Exists(_logsDirectory))
                return Ok(new List<string>());

            var dates = Directory.EnumerateFiles(_logsDirectory, "*.txt")
                .Select(f => TryParseDateFromFileName(Path.GetFileNameWithoutExtension(f)))
                .Where(d => d.HasValue)
                .Select(d => d!.Value)
                .OrderByDescending(d => d)
                .Select(d => d.ToString("yyyy-MM-dd"))
                .ToList();

            return Ok(dates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des dates disponibles");
            return StatusCode(500, new { message = "Erreur interne", error = ex.Message });
        }
    }

    private LogAnalyticsDto BuildAnalytics(string[] lines, string dateDisplay)
    {
        var users = new Dictionary<string, UserAnalyticsDto>(StringComparer.OrdinalIgnoreCase);
        var endpointCounts = new Dictionary<string, EndpointAnalyticsDto>(StringComparer.OrdinalIgnoreCase);
        var methodCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        var hourlyActivity = new int[24];
        var methodHourlyActivity = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
        var endpointHourlyTotals = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
        var endpointHourlyByUser = new Dictionary<string, Dictionary<string, int[]>>(StringComparer.OrdinalIgnoreCase);
        var statusCodes = new Dictionary<string, int>();

        string? currentUser = null;
        string? currentMethod = null;
        string? currentEndpoint = null;
        DateTime? currentTimestamp = null;

        int totalEntries = 0;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrEmpty(line) || line.StartsWith("-----")) continue;

            // 👤 Utilisateur
            if (line.StartsWith("\U0001F464", StringComparison.Ordinal) || line.StartsWith("Utilisateur", StringComparison.OrdinalIgnoreCase))
            {
                // Commit previous entry
                CommitLogEntry(ref currentUser, ref currentMethod, ref currentEndpoint, ref currentTimestamp,
                    users, endpointCounts, methodCounts, hourlyActivity, methodHourlyActivity,
                    endpointHourlyTotals, endpointHourlyByUser, ref totalEntries);

                currentUser = ExtractValue(line);
                currentMethod = null;
                currentEndpoint = null;
                currentTimestamp = null;
            }
            // 📥 Méthode HTTP
            else if (line.StartsWith("\U0001F4E5", StringComparison.Ordinal) || line.StartsWith("M\u00e9thode", StringComparison.OrdinalIgnoreCase) || line.StartsWith("Methode", StringComparison.OrdinalIgnoreCase))
            {
                currentMethod = ExtractValue(line).ToUpperInvariant();
            }
            // 🌐 Endpoint
            else if (line.StartsWith("\U0001F310", StringComparison.Ordinal) || line.StartsWith("Endpoint", StringComparison.OrdinalIgnoreCase))
            {
                currentEndpoint = ExtractValue(line);
            }
            // 🕒 Timestamp
            else if (line.StartsWith("\U0001F552", StringComparison.Ordinal) || line.StartsWith("@ Heure", StringComparison.OrdinalIgnoreCase) || line.StartsWith("Heure", StringComparison.OrdinalIgnoreCase))
            {
                var tsValue = ExtractValue(line).Replace("UTC", "", StringComparison.OrdinalIgnoreCase).Trim();
                if (DateTime.TryParseExact(tsValue, "dd/MM/yy HH:mm:ss", CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var ts))
                {
                    currentTimestamp = ts;
                }
            }
        }

        // Commit last entry
        CommitLogEntry(ref currentUser, ref currentMethod, ref currentEndpoint, ref currentTimestamp,
            users, endpointCounts, methodCounts, hourlyActivity, methodHourlyActivity,
            endpointHourlyTotals, endpointHourlyByUser, ref totalEntries);

        // Build response
        var userList = users.Values
            .OrderByDescending(u => u.TotalRequests)
            .ToList();

        var endpointList = endpointCounts.Values
            .OrderByDescending(e => e.Count)
            .Take(30)
            .ToList();

        var methodList = methodCounts
            .Select(m => new MethodAnalyticsDto { Method = m.Key, Count = m.Value })
            .OrderByDescending(m => m.Count)
            .ToList();

        var hourlyList = hourlyActivity
            .Select((count, hour) => new HourlyAnalyticsDto { Hour = hour, Label = $"{hour:D2}h", Count = count })
            .ToList();

        var methodHourlyList = methodHourlyActivity
            .Select(kvp => new MethodHourlyAnalyticsDto
            {
                Method = kvp.Key.ToUpperInvariant(),
                HourlyCounts = (int[])kvp.Value.Clone()
            })
            .ToList();

        var endpointHourlyBreakdown = endpointHourlyTotals
            .Select(kvp => new EndpointHourlyAnalyticsDto
            {
                Endpoint = kvp.Key,
                TotalHourlyCounts = (int[])kvp.Value.Clone(),
                Users = endpointHourlyByUser.TryGetValue(kvp.Key, out var userDict)
                    ? userDict.Select(u => new UserEndpointHourlyDto
                        {
                            User = u.Key,
                            HourlyCounts = (int[])u.Value.Clone()
                        })
                        .OrderByDescending(u => u.HourlyCounts.Sum())
                        .ToList()
                    : new List<UserEndpointHourlyDto>()
            })
            .ToList();

        // Peak hour
        var peakHour = hourlyActivity.Select((count, hour) => new { hour, count }).OrderByDescending(x => x.count).First();

        return new LogAnalyticsDto
        {
            Date = dateDisplay,
            TotalRequests = totalEntries,
            UniqueUsers = users.Count,
            UniqueEndpoints = endpointCounts.Count,
            PeakHour = $"{peakHour.hour:D2}h",
            PeakHourRequests = peakHour.count,
            Users = userList,
            Endpoints = endpointList,
            Methods = methodList,
            HourlyActivity = hourlyList,
            MethodHourlyActivity = methodHourlyList,
            EndpointHourlyBreakdown = endpointHourlyBreakdown
        };
    }

    private static void CommitLogEntry(
        ref string? user, ref string? method, ref string? endpoint, ref DateTime? timestamp,
        Dictionary<string, UserAnalyticsDto> users,
        Dictionary<string, EndpointAnalyticsDto> endpoints,
        Dictionary<string, int> methods,
        int[] hourlyActivity,
        Dictionary<string, int[]> methodHourlyActivity,
        Dictionary<string, int[]> endpointHourlyTotals,
        Dictionary<string, Dictionary<string, int[]>> endpointHourlyByUser,
        ref int totalEntries)
    {
        if (string.IsNullOrWhiteSpace(user) && string.IsNullOrWhiteSpace(endpoint)) return;

        totalEntries++;
        var userName = user ?? "Inconnu";
        var methodName = method ?? "GET";
        var endpointName = endpoint ?? "N/A";

        // User stats
        if (!users.TryGetValue(userName, out var userStats))
        {
            userStats = new UserAnalyticsDto { Name = userName };
            users[userName] = userStats;
        }
        userStats.TotalRequests++;
        userStats.Methods[methodName] = userStats.Methods.GetValueOrDefault(methodName) + 1;

        if (!userStats.Endpoints.TryGetValue(endpointName, out _))
            userStats.Endpoints[endpointName] = 0;
        userStats.Endpoints[endpointName]++;

        if (timestamp.HasValue)
        {
            if (userStats.FirstSeenUtc is null || timestamp < userStats.FirstSeenUtc)
                userStats.FirstSeenUtc = timestamp;
            if (userStats.LastSeenUtc is null || timestamp > userStats.LastSeenUtc)
                userStats.LastSeenUtc = timestamp;

            var hour = timestamp.Value.Hour;
            if (hour >= 0 && hour < 24)
            {
                hourlyActivity[hour]++;

                if (!methodHourlyActivity.TryGetValue(methodName, out var methodHours))
                {
                    methodHours = new int[24];
                    methodHourlyActivity[methodName] = methodHours;
                }
                methodHours[hour]++;
            }

            // Hourly breakdown per user
            userStats.HourlyActivity[hour]++;

            if (!endpointHourlyTotals.TryGetValue(endpointName, out var endpointTotals))
            {
                endpointTotals = new int[24];
                endpointHourlyTotals[endpointName] = endpointTotals;
            }
            endpointTotals[hour]++;

            if (!endpointHourlyByUser.TryGetValue(endpointName, out var endpointUsersDict))
            {
                endpointUsersDict = new Dictionary<string, int[]>(StringComparer.OrdinalIgnoreCase);
                endpointHourlyByUser[endpointName] = endpointUsersDict;
            }

            if (!endpointUsersDict.TryGetValue(userName, out var endpointUserHours))
            {
                endpointUserHours = new int[24];
                endpointUsersDict[userName] = endpointUserHours;
            }
            endpointUserHours[hour]++;
        }

        // Endpoint stats
        if (!endpoints.TryGetValue(endpointName, out var epStats))
        {
            epStats = new EndpointAnalyticsDto { Endpoint = endpointName };
            endpoints[endpointName] = epStats;
        }
        epStats.Count++;
        epStats.Methods[methodName] = epStats.Methods.GetValueOrDefault(methodName) + 1;
        if (!epStats.Users.Contains(userName))
            epStats.Users.Add(userName);

        // Method stats
        methods[methodName] = methods.GetValueOrDefault(methodName) + 1;

        // Reset
        user = null;
        method = null;
        endpoint = null;
        timestamp = null;
    }

    private static string ExtractValue(string line)
    {
        var colonIndex = line.IndexOf(':');
        return colonIndex == -1 ? line.Trim() : line[(colonIndex + 1)..].Trim();
    }

    // ── Analytics DTOs ──────────────────────────────────────────────────

    private sealed class LogAnalyticsDto
    {
        public string Date { get; set; } = string.Empty;
        public int TotalRequests { get; set; }
        public int UniqueUsers { get; set; }
        public int UniqueEndpoints { get; set; }
        public string PeakHour { get; set; } = string.Empty;
        public int PeakHourRequests { get; set; }
        public List<UserAnalyticsDto> Users { get; set; } = new();
        public List<EndpointAnalyticsDto> Endpoints { get; set; } = new();
        public List<MethodAnalyticsDto> Methods { get; set; } = new();
        public List<HourlyAnalyticsDto> HourlyActivity { get; set; } = new();
        public List<MethodHourlyAnalyticsDto> MethodHourlyActivity { get; set; } = new();
        public List<EndpointHourlyAnalyticsDto> EndpointHourlyBreakdown { get; set; } = new();
    }

    private sealed class UserAnalyticsDto
    {
        public string Name { get; set; } = string.Empty;
        public int TotalRequests { get; set; }
        public DateTime? FirstSeenUtc { get; set; }
        public DateTime? LastSeenUtc { get; set; }
        public Dictionary<string, int> Methods { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public Dictionary<string, int> Endpoints { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public int[] HourlyActivity { get; set; } = new int[24];
    }

    private sealed class EndpointAnalyticsDto
    {
        public string Endpoint { get; set; } = string.Empty;
        public int Count { get; set; }
        public Dictionary<string, int> Methods { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public List<string> Users { get; set; } = new();
    }

    private sealed class MethodAnalyticsDto
    {
        public string Method { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    private sealed class HourlyAnalyticsDto
    {
        public int Hour { get; set; }
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    private sealed class MethodHourlyAnalyticsDto
    {
        public string Method { get; set; } = string.Empty;
        public int[] HourlyCounts { get; set; } = new int[24];
    }

    private sealed class EndpointHourlyAnalyticsDto
    {
        public string Endpoint { get; set; } = string.Empty;
        public int[] TotalHourlyCounts { get; set; } = new int[24];
        public List<UserEndpointHourlyDto> Users { get; set; } = new();
    }

    private sealed class UserEndpointHourlyDto
    {
        public string User { get; set; } = string.Empty;
        public int[] HourlyCounts { get; set; } = new int[24];
    }

    private string? GetMostRecentLogFile()
    {
        return Directory.EnumerateFiles(_logsDirectory, "*.txt")
            .OrderByDescending(f => new FileInfo(f).LastWriteTimeUtc)
            .FirstOrDefault();
    }

    private string GetLogFilePath(DateOnly date) =>
        Path.Combine(_logsDirectory, date.ToString("ddMMyy", CultureInfo.InvariantCulture) + ".txt");

    private static DateOnly? TryParseDateFromFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return null;
        }

        return DateOnly.TryParseExact(fileName, "ddMMyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed)
            ? parsed
            : null;
    }

    private sealed class LogFileResolution
    {
        public required string Path { get; init; }
        public DateOnly? Date { get; init; }
        public string DateDisplay => Date?.ToString("yyyy-MM-dd") ?? System.IO.Path.GetFileNameWithoutExtension(Path);
    }

    private sealed class LogResponseDto
    {
        public string Date { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public DateTime LastModifiedUtc { get; set; }
        public int TotalEntries { get; set; }
        public int ReturnedEntries { get; set; }
        public bool HasMore { get; set; }
        public List<LogEntryDto> Entries { get; set; } = new();
    }

    private sealed class LogStreamResponseDto
    {
        public string Date { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public DateTime LastModifiedUtc { get; set; }
        public int TotalEntries { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int ReturnedEntries { get; set; }
        public bool HasNextPage { get; set; }
        public bool HasPreviousPage { get; set; }
        public int TotalPages { get; set; }
        public List<LogEntryDto> Entries { get; set; } = new();
    }

    private sealed class LogEntryDto
    {
        public string Timestamp { get; set; } = string.Empty;
        public string Level { get; set; } = "INFO";
        public string Source { get; set; } = "UNKNOWN";
        public string Message { get; set; } = string.Empty;
    }
}
