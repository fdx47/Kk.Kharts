using KK.UG6x.StoreAndForward.Domain.DTOs;
using KK.UG6x.StoreAndForward.Domain.Interfaces;
using KK.UG6x.StoreAndForward.Domain.Models;
using System.Text.Json;

namespace KK.UG6x.StoreAndForward.Services;

public class UG65Client : IUG65Client
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UG65Client> _logger;
    private readonly IGatewayDiscoveryService _discoveryService;
    private string _tokenAtual = string.Empty;
    private string? _discoveredIp;
    private string _organizationId = "1"; // Par défaut 1, sera mis à jour dynamiquement
    private readonly string _stateFilePath = Path.Combine(AppContext.BaseDirectory, "gateway_state.json");

    public string BaseUrl => !string.IsNullOrEmpty(_discoveredIp) ? $"https://{_discoveredIp}:8080" : $"https://{_configuration["GatewaySettings:IP"] ?? "192.168.1.150"}:8080";
    public UG65Client(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<UG65Client> logger, IGatewayDiscoveryService discoveryService)
    {
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
        };
        _httpClient = new HttpClient(handler);
        _configuration = configuration;
        _logger = logger;
        _discoveryService = discoveryService;

        LoadDiscoveredIp();
    }


    private void LoadDiscoveredIp()
    {
        try
        {
            if (File.Exists(_stateFilePath))
            {
                var json = File.ReadAllText(_stateFilePath);
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("LastIp", out var ipProp))
                {
                    _discoveredIp = ipProp.GetString();
                    _logger.LogInformation($"IP de la passerelle chargé depuis l'état: {_discoveredIp}");
                }
            }
        }
        catch { }
    }


    private void SaveDiscoveredIp(string ip)
    {
        try
        {
            _discoveredIp = ip;
            var json = JsonSerializer.Serialize(new { LastIp = ip, DiscoveryTime = DateTime.UtcNow });
            File.WriteAllText(_stateFilePath, json);
        }
        catch { }
    }


    private void HandleNetworkError(Exception ex, string context)
    {
        _logger.LogWarning($"[UG65] Erreur réseau détectée dans {context}: {ex.Message}. Réinitialisation de l'IP découverte pour une nouvelle tentative de recherche.");
        _discoveredIp = null;
        _tokenAtual = string.Empty;
        // Optionnel : ne pas supprimer le fichier d'état pour permettre au scan de re-tester l'ancien IP en premier (LIFO)
    }


    private async Task<string> GetBaseUrlAsync(CancellationToken ct)
    {
        // On essaye de garder le protocole qui marche
        if (!string.IsNullOrEmpty(_discoveredIp))
        {
            // On tente HTTPS sur le port 8080 (standard Milesight securisé)
            return $"https://{_discoveredIp}:8080";
        }

        await DiscoverGatewayAsync(ct);

        var finalIp = _discoveredIp ?? _configuration["GatewaySettings:IP"] ?? "192.168.1.150";
        return $"https://{finalIp}:8080";
    }


    private async Task DiscoverGatewayAsync(CancellationToken ct)
    {
        //var user = _configuration["GatewaySettings:Username"] ?? "admin";
        // var pass = _configuration["GatewaySettings:Password"] ?? "L2eA6K11q9SPgBGBG8dAeg==";

        var user = string.IsNullOrEmpty(_configuration["GatewaySettings:Username"])
           ? "admin"
           : _configuration["GatewaySettings:Username"];


        var pass = string.IsNullOrEmpty(_configuration["GatewaySettings:Password"])
           ? "L2eA6K11q9SPgBGBG8dAeg=="
           : _configuration["GatewaySettings:Password"];

        // FindGatewayAsync já tenta o _discoveredIp internamente (LIFO) se passarmos como lastKnownIp
        var ipFound = await _discoveryService.FindGatewayAsync(_discoveredIp, user, pass, ct);

        if (ipFound != null)
        {
            SaveDiscoveredIp(ipFound);
        }
    }

    public async Task<bool> PurgeUrPacketsAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(_tokenAtual))
            {
                await RealizarLoginAsync(cancellationToken);
            }

            if (string.IsNullOrEmpty(_tokenAtual))
            {
                _logger.LogWarning("[UG65] Impossible de purger les paquets: token invalide.");
                return false;
            }

            var baseUrl = await GetBaseUrlAsync(cancellationToken);
            var url = $"{baseUrl}/api/urpackets?OrganizationID={_organizationId}";
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenAtual);

            var response = await _httpClient.DeleteAsync(url, cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                _logger.LogWarning("[UG65] Token expiré lors du purge. Tentative de reconnexion...");
                _tokenAtual = string.Empty;
                await RealizarLoginAsync(cancellationToken);

                if (string.IsNullOrEmpty(_tokenAtual))
                {
                    return false;
                }

                baseUrl = await GetBaseUrlAsync(cancellationToken);
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenAtual);
                response = await _httpClient.DeleteAsync(url, cancellationToken);
            }

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("[UG65] Purge des paquets effectuée avec succès côté gateway.");
                return true;
            }

            var detail = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning($"[UG65] Purge des paquets refusée. Statut: {response.StatusCode}. Détails: {detail}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UG65] Erreur lors de la purge des paquets côté gateway.");
            return false;
        }
    }

    public async Task<List<GatewayPayloadDTO>> FetchLatestDataAsync(CancellationToken cancellationToken)
    {
        var listaDeDados = new List<GatewayPayloadDTO>();
        try
        {
            if (string.IsNullOrEmpty(_tokenAtual)) await RealizarLoginAsync(cancellationToken);
            if (string.IsNullOrEmpty(_tokenAtual)) return listaDeDados;

            var baseUrl = await GetBaseUrlAsync(cancellationToken);
            // On ajoute OrganizationID dynamique récupéré après login
            var urlPacotes = $"{baseUrl}/api/urpackets?OrganizationID={_organizationId}&limit=100";
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenAtual);

            var responsePacotes = await _httpClient.GetAsync(urlPacotes, cancellationToken);

            if (responsePacotes.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                responsePacotes.StatusCode == System.Net.HttpStatusCode.Forbidden ||
                responsePacotes.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Connexion perdue ou session expirée. Tentative de reconnexion...");
                _tokenAtual = string.Empty;
                await RealizarLoginAsync(cancellationToken);

                if (!string.IsNullOrEmpty(_tokenAtual))
                {
                    baseUrl = await GetBaseUrlAsync(cancellationToken);
                    _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenAtual);
                    responsePacotes = await _httpClient.GetAsync($"{baseUrl}/api/urpackets?OrganizationID={_organizationId}&limit=100", cancellationToken);
                }
            }

            if (!responsePacotes.IsSuccessStatusCode)
            {
                _logger.LogError($"Échec de la récupération des paquets. Statut: {responsePacotes.StatusCode}");
                return listaDeDados;
            }

            var conteudoResposta = await responsePacotes.Content.ReadAsStringAsync(cancellationToken);
            using var docJson = JsonDocument.Parse(conteudoResposta);
            var elementoRaiz = docJson.RootElement;

            if (elementoRaiz.TryGetProperty("packets", out var propriedadePackets) && propriedadePackets.ValueKind == JsonValueKind.Array)
            {
                foreach (var pacoteJson in propriedadePackets.EnumerateArray())
                {
                    try
                    {
                        // Só processa se houver payloadJson decodificado pela Gateway
                        if (!IsUplinkPacket(pacoteJson))
                        {
                            continue;
                        }

                        if (pacoteJson.TryGetProperty("payloadJson", out var payloadProp) && !string.IsNullOrWhiteSpace(payloadProp.GetString()))
                        {
                            var devEui = pacoteJson.GetProperty("devEUI").GetString() ?? string.Empty;
                            var timeStr = pacoteJson.GetProperty("time").GetString() ?? string.Empty;
                            var payloadJsonStr = payloadProp.GetString();

                            if (string.IsNullOrWhiteSpace(payloadJsonStr) || payloadJsonStr.Trim() == "-")
                            {
                                continue;
                            }

                            // Logique robuste pour le payloadJson
                            GatewayPayloadDTO? genericPayload = null;
                            try
                            {
                                if (!string.IsNullOrWhiteSpace(payloadJsonStr) && payloadJsonStr.TrimStart().StartsWith("{"))
                                {
                                    genericPayload = JsonSerializer.Deserialize<GatewayPayloadDTO>(payloadJsonStr!);
                                }
                                else
                                {
                                    // Payload n'est pas un objet JSON (ex: valeur brute "12.5")
                                    genericPayload = new GatewayPayloadDTO();
                                    genericPayload.AdditionalData["raw_value"] = payloadJsonStr ?? "";
                                }
                            }
                            catch (Exception exJson)
                            {
                                _logger.LogWarning($"[UG65] Erreur parsing payload pour {devEui}: {exJson.Message}. Contenu: {payloadJsonStr}");
                                genericPayload = new GatewayPayloadDTO();
                                genericPayload.AdditionalData["error"] = "Invalid JSON";
                                genericPayload.AdditionalData["raw"] = payloadJsonStr ?? "";
                            }

                            if (genericPayload != null)
                            {
                                // Garante que timestamp e devEUI estão no root conforme pedido
                                genericPayload.DevEui = devEui;

                                var hasSensorTimestamp = genericPayload.Timestamp != default;
                                if (hasSensorTimestamp)
                                {
                                    genericPayload.Timestamp = DateTime.SpecifyKind(genericPayload.Timestamp, DateTimeKind.Utc);
                                }
                                else if (DateTime.TryParse(timeStr, null, System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal, out var dataParsed))
                                {
                                    genericPayload.Timestamp = dataParsed;
                                }
                                else
                                {
                                    genericPayload.Timestamp = DateTime.UtcNow;
                                }

                                listaDeDados.Add(genericPayload);
                            }
                        }
                    }
                    catch (Exception exLoop)
                    {
                        _logger.LogError(exLoop, "Erreur de traitement paquet individuel.");
                    }
                }
            }
            return listaDeDados;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur fatale dans FetchLatestDataAsync");
            return listaDeDados;
        }
    }

    private async Task RealizarLoginAsync(CancellationToken cancellationToken)
    {
        var baseUrl = await GetBaseUrlAsync(cancellationToken);
        var urlAuth = $"{baseUrl}/api/internal/login";
        //var userNome = _configuration["GatewaySettings:Username"];
        //var senhaUser = _configuration["GatewaySettings:Password"];
        var userNome = string.IsNullOrEmpty(_configuration["GatewaySettings:Username"])
               ? "admin"
               : _configuration["GatewaySettings:Username"];

        var senhaUser = string.IsNullOrEmpty(_configuration["GatewaySettings:Password"])
                        ? "L2eA6K11q9SPgBGBG8dAeg=="
                        : _configuration["GatewaySettings:Password"];

        string senhaFinal = IsAlreadyEncrypted(senhaUser) ? senhaUser : EncryptPassword(senhaUser);

        var corpoLogin = new { username = userNome, password = senhaFinal };
        var jsonLogin = JsonSerializer.Serialize(corpoLogin);
        var conteudoLogin = new StringContent(jsonLogin, System.Text.Encoding.UTF8, "application/json");

        try
        {
            _logger.LogInformation($"Tentative d'authentification sur {urlAuth}");
            var responseLogin = await _httpClient.PostAsync(urlAuth, conteudoLogin, cancellationToken);
            if (responseLogin.IsSuccessStatusCode)
            {
                var resultadoJson = await responseLogin.Content.ReadFromJsonAsync<JsonElement>(cancellationToken: cancellationToken);
                if (resultadoJson.TryGetProperty("jwt", out var jwtTokenProp))
                {
                    _tokenAtual = jwtTokenProp.GetString() ?? string.Empty;
                    _logger.LogInformation("Authentification réussie sur UG65.");

                    // Une fois loggué, on découvre dynamiquement l'OrganizationID
                    await DiscoverOrganizationIdAsync(cancellationToken);
                }
            }
            else
            {
                var erroDetalhe = await responseLogin.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning($"Échec de l'authentification (Statut: {responseLogin.StatusCode}). Détail: {erroDetalhe}");

                if (responseLogin.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    _logger.LogError("[UG65] Erreur 400 (Bad Request). Vérifiez si le mot de passe dans appsettings.json est correct ou s'il doit être en texte brut.");
                }
            }
        }
        catch (HttpRequestException exReq)
        {
            HandleNetworkError(exReq, "RealizarLoginAsync");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception lors de l'authentification.");
        }
    }

    private async Task DiscoverOrganizationIdAsync(CancellationToken cancellationToken)
    {
        try
        {
            var baseUrl = await GetBaseUrlAsync(cancellationToken);
            var urlOrg = $"{baseUrl}/api/urorganizations?limit=10";
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenAtual);

            var response = await _httpClient.GetAsync(urlOrg, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(content);
                // Tenta "result" primeiro (padrão verificado) ou "organizations"
                //var orgsProp = new JsonProperty();
                if (!doc.RootElement.TryGetProperty("result", out var resultProp))
                {
                    doc.RootElement.TryGetProperty("organizations", out resultProp);
                }

                if (resultProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var org in resultProp.EnumerateArray())
                    {
                        if (org.TryGetProperty("id", out var idProp) || org.TryGetProperty("organizationID", out idProp))
                        {
                            _organizationId = idProp.GetString() ?? "1";
                            _logger.LogInformation($"[UG65] OrganizationID découvert dynamiquement : {_organizationId}");
                            return;
                        }
                    }
                }
            }
            _logger.LogWarning($"[UG65] Impossible de découvrir l'OrganizationID (Statut: {response.StatusCode}). Utilisation de la valeur par défaut: {_organizationId}");
        }
        catch (Exception ex)
        {
            _logger.LogWarning($"[UG65] Erreur lors de la découverte de l'OrganizationID: {ex.Message}. Utilisation de {_organizationId}");
        }
    }

    public async Task<List<UG65Device>> GetDevicesAsync(CancellationToken cancellationToken)
    {
        var listaDevices = new List<UG65Device>();
        try
        {
            if (string.IsNullOrEmpty(_tokenAtual)) await RealizarLoginAsync(cancellationToken);
            var baseUrl = await GetBaseUrlAsync(cancellationToken);
            var urlDevices = $"{baseUrl}/api/urdevices?OrganizationID={_organizationId}&limit=100";
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenAtual);
            var response = await _httpClient.GetAsync(urlDevices, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var wrapper = JsonSerializer.Deserialize<UG65DeviceListResponse>(content);
                if (wrapper?.Devices != null)
                {
                    listaDevices = wrapper.Devices;
                }
            }
        }
        catch (HttpRequestException exReq)
        {
            HandleNetworkError(exReq, "GetDevicesAsync");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur GetDevices");
        }
        return listaDevices;
    }

    public async Task<List<UG65Application>> GetApplicationsAsync(CancellationToken cancellationToken)
    {
        var listaApps = new List<UG65Application>();
        try
        {
            if (string.IsNullOrEmpty(_tokenAtual)) await RealizarLoginAsync(cancellationToken);
            var baseUrl = await GetBaseUrlAsync(cancellationToken);
            var urlApps = $"{baseUrl}/api/urapplications?OrganizationID={_organizationId}&limit=100";
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenAtual);
            var response = await _httpClient.GetAsync(urlApps, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                var wrapper = JsonSerializer.Deserialize<UG65ApplicationListResponse>(content);
                if (wrapper?.Applications != null)
                {
                    listaApps = wrapper.Applications;
                }
            }
        }
        catch (HttpRequestException exReq)
        {
            HandleNetworkError(exReq, "GetApplicationsAsync");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur GetApplications");
        }
        return listaApps;
    }

    public async Task<UG65Integration?> GetIntegrationAsync(string appId, string type, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrEmpty(_tokenAtual)) await RealizarLoginAsync(cancellationToken);
            var baseUrl = await GetBaseUrlAsync(cancellationToken);
            var urlIntegration = $"{baseUrl}/api/urapplications/{appId}/integrations/{type}";

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _tokenAtual);
            var response = await _httpClient.GetAsync(urlIntegration, cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<UG65Integration>(content);
            }
        }
        catch (HttpRequestException exReq)
        {
            HandleNetworkError(exReq, "GetIntegrationAsync");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur GetIntegration");
        }
        return null;
    }

    private bool IsAlreadyEncrypted(string input)
    {
        try
        {
            var keyRes = "1111111111111111";
            var ivRes = "2222222222222222";
            if (string.IsNullOrEmpty(input)) return false;
            using var aes = System.Security.Cryptography.Aes.Create();
            aes.Key = System.Text.Encoding.UTF8.GetBytes(keyRes);
            aes.IV = System.Text.Encoding.UTF8.GetBytes(ivRes);
            aes.Mode = System.Security.Cryptography.CipherMode.CBC;
            aes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
            using var decryptor = aes.CreateDecryptor();
            using var ms = new System.IO.MemoryStream(Convert.FromBase64String(input));
            using var cs = new System.Security.Cryptography.CryptoStream(ms, decryptor, System.Security.Cryptography.CryptoStreamMode.Read);
            using var sr = new System.IO.StreamReader(cs);
            sr.ReadToEnd();
            return true;
        }
        catch { return false; }
    }

    private string EncryptPassword(string password)
    {
        try
        {
            var keyRes = "1111111111111111";
            var ivRes = "2222222222222222";
            using var aes = System.Security.Cryptography.Aes.Create();
            aes.Key = System.Text.Encoding.UTF8.GetBytes(keyRes);
            aes.IV = System.Text.Encoding.UTF8.GetBytes(ivRes);
            aes.Mode = System.Security.Cryptography.CipherMode.CBC;
            aes.Padding = System.Security.Cryptography.PaddingMode.PKCS7;
            using var encryptor = aes.CreateEncryptor();
            using var ms = new System.IO.MemoryStream();
            using var cs = new System.Security.Cryptography.CryptoStream(ms, encryptor, System.Security.Cryptography.CryptoStreamMode.Write);
            using var sw = new System.IO.StreamWriter(cs);
            sw.Write(password);
            sw.Flush();
            cs.FlushFinalBlock();
            return Convert.ToBase64String(ms.ToArray());
        }
        catch { return password; }
    }

    private static bool IsUplinkPacket(JsonElement pacoteJson)
    {
        if (!pacoteJson.TryGetProperty("type", out var typeProp))
        {
            return false;
        }

        var typeValue = typeProp.GetString();
        return string.Equals(typeValue, "UpUnc", StringComparison.OrdinalIgnoreCase);
    }
}
