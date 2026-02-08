using Kk.Kharts.Api.Services;
using Kk.Kharts.Api.Services.IService;
using Kk.Kharts.Api.Services.Telegram;
using System.Security.Claims;
using System.Text.Json;

public class DebugUserActivityMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly object _fileLock = new();
    private readonly IKkTimeZoneService _timeZoneService;

    public DebugUserActivityMiddleware(RequestDelegate next, IKkTimeZoneService timeZoneService)
    {
        _next = next;
        _timeZoneService = timeZoneService;
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

            var logDir = Path.Combine(AppContext.BaseDirectory, "kklogs");
            Directory.CreateDirectory(logDir);

            var fileName = DateTime.UtcNow.ToString("ddMMyy") + ".txt";
            var filePath = Path.Combine(logDir, fileName);

            // ✅ PROTEGE ACESSO AO ARQUIVO
            lock (_fileLock)
            {
                File.AppendAllText(filePath, logMsg + Environment.NewLine);
            }
        }

        await _next(context);
    }
}