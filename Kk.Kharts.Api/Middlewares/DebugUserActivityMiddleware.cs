using System.Security.Claims;
using System.Text.Json;
using Kk.Kharts.Api.Services;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Services.Telegram;
using Microsoft.Extensions.Logging;

public class DebugUserActivityMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly object _fileLock = new();
    private readonly IKkTimeZoneService _timeZoneService;
    private readonly ILogger<DebugUserActivityMiddleware> _logger;

    public DebugUserActivityMiddleware(RequestDelegate next, IKkTimeZoneService timeZoneService, ILogger<DebugUserActivityMiddleware> logger)
    {
        _next = next;
        _timeZoneService = timeZoneService;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userName = context.User.FindFirst(ClaimTypes.Name)?.Value ?? context.User.Identity?.Name ?? "inconnu";
            var path = context.Request.Path;
            var method = context.Request.Method;

            var queryParams = context.Request.Query;
            string queryJson = queryParams.Count > 0
                ? JsonSerializer.Serialize(
                    queryParams.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.ToString()),
                    new JsonSerializerOptions { WriteIndented = true })
                : "{}";

            var logMsg = $"""
                  👤 Utilisateur connecté : {userName}
                  🌐 Endpoint appelé : {path}
                  📥 Méthode HTTP : {method}
                  🧾 Paramètres :
                  {queryJson}
                  🕒 Heure : {DateTime.UtcNow:dd/MM/yy HH:mm:ss} UTC
                  ----------------------------------------------------------------------
                  """;

            var logDir = Path.Combine(AppContext.BaseDirectory, Kk.Kharts.Api.Utility.Constants.GlobalConstants.LogsDirectoryName);
            Directory.CreateDirectory(logDir);

            var fileName = DateTime.UtcNow.ToString("ddMMyy") + ".txt";
            var filePath = Path.Combine(logDir, fileName);

            try
            {
                using var stream = new FileStream(filePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);
                using var writer = new StreamWriter(stream);
                writer.WriteLine(logMsg);
                writer.WriteLine();
            }
            catch (IOException ioEx)
            {
                _logger.LogWarning(ioEx, "Impossible d'écrire dans {FilePath}.", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'enregistrement de l'activité utilisateur dans {FilePath}.", filePath);
            }
        }

        await _next(context);
    }
}