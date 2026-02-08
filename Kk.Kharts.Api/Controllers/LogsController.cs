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
        _logsDirectory = Path.Combine(AppContext.BaseDirectory, "kklogs");

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
