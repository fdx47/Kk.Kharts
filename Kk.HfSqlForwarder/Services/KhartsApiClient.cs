using HfSqlForwarder.Models;
using HfSqlForwarder.Settings;
using Microsoft.Extensions.Options;

namespace HfSqlForwarder.Services;

public class KhartsApiClient
{
    private readonly HttpClient _client;
    private readonly ILogger<KhartsApiClient> _logger;
    private readonly IOptionsMonitor<ForwarderOptions> _options;

    public KhartsApiClient(HttpClient client, ILogger<KhartsApiClient> logger, IOptionsMonitor<ForwarderOptions> options)
    {
        _client = client;
        _logger = logger;
        _options = options;
    }

    public async Task<bool> SendBatchAsync(IEnumerable<RegaRecord> records, CancellationToken ct)
    {
        var list = records.ToList();
        if (!list.Any()) return true;

        var opts = _options.CurrentValue.Api;
        _client.Timeout = TimeSpan.FromSeconds(opts.TimeoutSeconds);
        _client.BaseAddress = new Uri(opts.BaseUrl.TrimEnd('/'));
        var apiKey = !string.IsNullOrWhiteSpace(opts.ApiKey)
            ? opts.ApiKey
            : LoadApiKeyFromSecret(opts.ApiKeySecretPath);
        if (!string.IsNullOrWhiteSpace(apiKey))
        {
            var headerName = string.IsNullOrWhiteSpace(opts.ApiKeyHeaderName) ? "X-API-KEY" : opts.ApiKeyHeaderName;
            if (_client.DefaultRequestHeaders.Contains(headerName))
            {
                _client.DefaultRequestHeaders.Remove(headerName);
            }
            _client.DefaultRequestHeaders.Add(headerName, apiKey);
        }

        var url = opts.EndpointGlowflex.StartsWith("/") ? opts.EndpointGlowflex : $"/{opts.EndpointGlowflex}";

        try
        {
            var response = await _client.PostAsJsonAsync(url, list, cancellationToken: ct).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
                _logger.LogWarning("[Glowflex] Envoi KO {Status} {Body}", response.StatusCode, body);
                return false;
            }
            _logger.LogInformation("[Glowflex] Batch envoyé ({Count} éléments)", list.Count);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[Glowflex] Erreur lors de l'appel API");
            return false;
        }
    }

    private string? LoadApiKeyFromSecret(string? path)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        try
        {
            var fullPath = Path.IsPathRooted(path)
                ? path
                : Path.Combine(AppContext.BaseDirectory, path);
            if (!File.Exists(fullPath)) return null;
            var content = File.ReadAllText(fullPath).Trim();
            return string.IsNullOrWhiteSpace(content) ? null : content;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Impossible de lire le fichier secret APIKey {Path}", path);
            return null;
        }
    }
}
