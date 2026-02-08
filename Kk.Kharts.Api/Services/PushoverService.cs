using System.Text;
using System.Text.Json;
using Kk.Kharts.Api.Services.IService;

namespace Kk.Kharts.Api.Services;

public class PushoverService : IPushoverService
{
    private readonly HttpClient _httpClient;

    public PushoverService(HttpClient httpClient)
    {
        _httpClient = httpClient;

        if (_httpClient.BaseAddress is null)
            _httpClient.BaseAddress = new Uri("https://api.pushover.net/1/");
    }

    public async Task SendAsync(PushoverMessageOptions options, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(options.UserKey))
            throw new ArgumentException("La clé utilisateur Pushover est obligatoire.", nameof(options));

        if (string.IsNullOrWhiteSpace(options.Message))
            throw new ArgumentException("Le contenu du message Pushover est obligatoire.", nameof(options));

        if (string.IsNullOrWhiteSpace(options.AppToken))
            throw new ArgumentException("Le token applicatif Pushover est obligatoire.", nameof(options));

        var payload = new
        {
            token = options.AppToken,
            user = options.UserKey,
            title = options.Title,
            message = options.Message,
            sound = options.Sound,
            device = options.Device,
            priority = options.Priority,
            retry = options.RetrySeconds,
            expire = options.ExpireSeconds
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        using var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("messages.json", content, ct);
        response.EnsureSuccessStatusCode();
    }
}