using Kk.Kharts.Api.Services.Telegram;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Text;
using Telegram.Bot.Types.Enums;

namespace Kk.Kharts.Api.Controllers;

/// <summary>
/// Controller de debug unificado.
/// </summary>
[ApiController]
[Route("api/v1/debugs")]
public class DebugController(ITelegramService telegram, IOptions<TelegramOptions> telegramOptions) : ControllerBase
{
    private readonly string _logDir = Path.Combine(AppContext.BaseDirectory, Utility.Constants.GlobalConstants.LogsDirectoryName);
    private readonly TelegramOptions _options = telegramOptions.Value;


    [HttpPost("receive-unknown-json")]
    public async Task<IActionResult> ReceiveUnknownJson()
    {
        try
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var requestBody = await reader.ReadToEndAsync();

            static string FormatDictionary(IEnumerable<KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues>> source)
            {
                var entries = source
                    .Select(pair => $"{pair.Key}: {string.Join(", ", pair.Value.ToArray())}")
                    .ToList();

                return entries.Count == 0 ? "(aucune donnée)" : string.Join("\n", entries);
            }

            var headersText = FormatDictionary(Request.Headers);
            var queryText = FormatDictionary(Request.Query);

            var timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            var message = $"🛠️ <b>DebugController</b> - {timestamp}\n" +
                          "\n📄 <b>Headers</b>:\n<pre>" + telegram.EscapeHtml(headersText) + "</pre>" +
                          "\n⚙️ <b>Paramètres</b>:\n<pre>" + telegram.EscapeHtml(queryText) + "</pre>" +
                          "\n📩 <b>Requête reçue</b>:\n<pre>" + telegram.EscapeHtml(requestBody) + "</pre>";
            await telegram.SendToDebugTopicAsync(message, ParseMode.Html);

            return Ok("JSON reçu avec succès!");
        }
        catch (Exception ex)
        {
            await telegram.SendToDebugTopicAsync($"🔴 Erreur: {ex.Message}", ParseMode.None);
            return BadRequest($"Erreur: {ex.Message}");
        }
    }


    [Authorize(Roles = "Root")]
    [HttpGet("endpoints/{date}")]
    public async Task<IActionResult> GetLogByDate(string date)
    {
        var filePath = Path.Combine(_logDir, $"{date}.txt");
        if (!System.IO.File.Exists(filePath))
            return NotFound($"Log para {date} não encontrado.");

        var content = await System.IO.File.ReadAllTextAsync(filePath);
        return Content(content, "text/plain");
    }


    [Authorize(Roles = "Root")]
    [HttpGet("endpoints/stats/{date}")]
    public async Task<IActionResult> GetLogStats(string date)
    {
        var filePath = Path.Combine(_logDir, $"{date}.txt");
        if (!System.IO.File.Exists(filePath))
            return NotFound($"Aucun Log trouvé pour le {date}.");

        var lines = await System.IO.File.ReadAllLinesAsync(filePath);
        var userStats = new Dictionary<string, Dictionary<string, int>>();
        string? currentUser = null;

        foreach (var line in lines)
        {
            if (line.StartsWith("👤"))
                currentUser = line.Split(':').Last().Trim();
            else if (line.StartsWith("📥") && !string.IsNullOrEmpty(currentUser))
            {
                var method = line.Split(':').Last().Trim().ToUpper();
                if (!userStats.TryGetValue(currentUser, out var methods))
                    userStats[currentUser] = methods = new(StringComparer.OrdinalIgnoreCase);
                methods[method] = methods.GetValueOrDefault(method) + 1;
                currentUser = null;
            }
        }
        return Ok(userStats);
    }


    [HttpPost("tests/receive-unknown-json")]
    public async Task<IActionResult> ReceiveUnknownJsonTest()
    {
        try
        {
            using var reader = new StreamReader(Request.Body, Encoding.UTF8);
            var requestBody = await reader.ReadToEndAsync();

            var timestamp = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            var message = $"🛠️ <b>DebugController-TEST</b> - {timestamp}\n\n🟢 Requête TEST:\n<pre>{telegram.EscapeHtml(requestBody)}</pre>";
            await telegram.SendToDebugTopicAsync(message, ParseMode.Html);

            return Ok("JSON TEST reçu!");
        }
        catch (Exception ex)
        {
            await telegram.SendToDebugTopicAsync($"🔴 Erreur TEST: {ex.Message}", ParseMode.None);
            return BadRequest($"Erreur: {ex.Message}");
        }
    }


    [HttpGet("tests/health")]
    public IActionResult HealthCheck() => Ok(new
    {
        status = "OK",
        timestamp = DateTime.UtcNow,
        controller = "DebugController"
    });


    [HttpGet("topics-info")]
    public IActionResult GetTopicsInfo() => Ok(new
    {
        telegram_topics = new
        {
            errors = new { id = _options.ErrorsTopicId, name = "Erreurs SDI12" },
            debug = new { id = _options.DebugTopicId, name = "Debug" },
            device_status = new { id = _options.DeviceStatusTopicId, name = "Device Status" },
            support = new { id = _options.SupportTopicId, name = "Support" }
        }
    });
}
