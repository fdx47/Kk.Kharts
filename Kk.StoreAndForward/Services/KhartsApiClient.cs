using KK.UG6x.StoreAndForward.Domain.DTOs;
using KK.UG6x.StoreAndForward.Domain.Interfaces;

namespace KK.UG6x.StoreAndForward.Services;

public class KhartsApiClient : IKhartsApiClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KhartsApiClient> _logger;

    public KhartsApiClient(HttpClient httpClient, IConfiguration configuration, ILogger<KhartsApiClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendDataAsync(GatewayPayloadDTO dadosParaEnvio, CancellationToken cancellationToken, string? destinationUrl = null, Dictionary<string, string>? customHeaders = null)
    {
        try
        {
            // Validação: Agora dependemos exclusivamente da descoberta dinâmica ou parâmetros passados.
            if (string.IsNullOrEmpty(destinationUrl))
            {
                _logger.LogWarning($"[KhartsApiClient] URL de destination manquante pour {dadosParaEnvio.DevEui}. Impossible d'envoyer.");
                return false;
            }

            _httpClient.DefaultRequestHeaders.Clear();

            // Aplica APENAS headers dinâmicos recuperados da Gateway
            if (customHeaders != null && customHeaders.Count > 0)
            {
                foreach (var header in customHeaders)
                {
                    _httpClient.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            var responseEnvio = await _httpClient.PostAsJsonAsync(destinationUrl, dadosParaEnvio, cancellationToken);

            // Aceita 200 (OK) e 208 (AlreadyReported) como sucesso
            if (responseEnvio.IsSuccessStatusCode || responseEnvio.StatusCode == System.Net.HttpStatusCode.AlreadyReported)
            {
                var responseBody = await responseEnvio.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation($"[API Response] Succès ({responseEnvio.StatusCode}): {responseBody}");
                return true;
            }
            else
            {
                var responseBody = await responseEnvio.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning($"[API Response] Échec ({responseEnvio.StatusCode}): {responseBody} | Appareil: {dadosParaEnvio.DevEui}");
                return false;
            }
        }
        catch (HttpRequestException ex) when (ex.InnerException is System.Net.Sockets.SocketException { SocketErrorCode: System.Net.Sockets.SocketError.HostNotFound or System.Net.Sockets.SocketError.ConnectionRefused })
        {
            _logger.LogWarning($"[KhartsApiClient] Erreur de connexion au Cloud: {ex.Message} (Hôte: {destinationUrl ?? "inconnu"})");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Exception lors de l'envoi des données à l'API Cloud (Dyn). Appareil: {dadosParaEnvio.DevEui}");
            return false;
        }
    }
}
