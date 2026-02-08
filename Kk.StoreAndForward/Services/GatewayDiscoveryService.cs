using KK.UG6x.StoreAndForward.Domain.Interfaces;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace KK.UG6x.StoreAndForward.Services;

public class GatewayDiscoveryService : IGatewayDiscoveryService
{
    private const int GatewayApiPort = 8080;
    private readonly ILogger<GatewayDiscoveryService> _logger;
    private readonly HttpClient _httpClient;

    public GatewayDiscoveryService(ILogger<GatewayDiscoveryService> logger)
    {
        _logger = logger;
        // Bypassing SSL for discovery as it's common for gateways to have self-signed certs
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true
        };
        _httpClient = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(3) };
    }

    public async Task<string?> FindGatewayAsync(string? lastKnownIp, string username, string password, CancellationToken ct)
    {
        _logger.LogInformation("Démarrage du processus de découverte de la passerelle...");

        // 1. Tentar IP anterior
        if (!string.IsNullOrEmpty(lastKnownIp))
        {
            _logger.LogInformation($"Tentative sur le dernier IP connu: {lastKnownIp}");
            if (await IsGatewayValidAsync(lastKnownIp, username, password, ct))
            {
                _logger.LogInformation($"Gateway trouvée à nouveau sur {lastKnownIp}");
                return lastKnownIp;
            }
        }

        // 2. Scan réseau
        _logger.LogInformation("Début du scan réseau local sur le port 8080...");
        var candidates = EnumerateLocalSubnetHosts();

        // Scan en parallèle pour plus de rapidité
        var results = new System.Collections.Concurrent.ConcurrentBag<string>();
        var parallelOptions = new ParallelOptions
        {
            CancellationToken = ct,
            MaxDegreeOfParallelism = 20
        };

        await Parallel.ForEachAsync(candidates, parallelOptions, async (ip, token) =>
        {
            if (await IsGatewayPortOpenAsync(ip, GatewayApiPort))
            {
                _logger.LogInformation($"[Discovery] Port 8080 OUVERT sur {ip}. Vérification de l'identité...");
                if (await IsGatewayValidAsync(ip, username, password, token))
                {
                    results.Add(ip);
                }
            }
        });

        var foundIp = results.FirstOrDefault();
        if (foundIp != null)
        {
            _logger.LogInformation($"Scanner: Gateway Milesight détectée sur {foundIp}");
        }
        else
        {
            _logger.LogWarning("Scanner: Aucune passerelle UG65 détectée sur le réseau local.");
        }

        return foundIp;
    }

    private async Task<bool> IsGatewayPortOpenAsync(string ip, int port)
    {
        try
        {
            using var tcp = new TcpClient();
            var connectTask = tcp.ConnectAsync(ip, port);
            var timeoutTask = Task.Delay(500); // Increased to 500ms
            var completed = await Task.WhenAny(connectTask, timeoutTask);
            return completed == connectTask && tcp.Connected;
        }
        catch { return false; }
    }

    private async Task<bool> IsGatewayValidAsync(string ip, string username, string password, CancellationToken ct)
    {
        // On teste les deux protocoles, en priorisant HTTP qui est la norme locale Milesight
        string[] protocols = { "http", "https" };

        foreach (var proto in protocols)
        {
            try
            {
                var url = $"{proto}://{ip}:{GatewayApiPort}/api/internal/login";
                _logger.LogDebug($"[Discovery] Testing {url}...");

                string finalPassword = IsAlreadyEncrypted(password) ? password : EncryptPassword(password);
                var payload = JsonSerializer.Serialize(new { username, password = finalPassword });
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content, ct);

                // Si succès, c'est gagné
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"[Discovery] Gateway trouvée à {ip} via {proto}://");
                    return true;
                }

                // Si erreur 500 avec "wrong password", c'est aussi signe que c'est une gateway Milesight
                if (response.StatusCode == HttpStatusCode.InternalServerError)
                {
                    var responseBody = await response.Content.ReadAsStringAsync(ct);
                    if (responseBody.Contains("wrong password"))
                    {
                        _logger.LogWarning($"[Discovery] Gateway trouvée à {ip} ({proto}://) mais ÉCHEC LOGIN (Mot de passe incorrect).");
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug($"[Discovery] {proto} failed on {ip}: {ex.Message}");
            }
        }
        return false;
    }

    // Alias pour compatibilité
    private Task<bool> IsGatewayValidInternalAsync(string ip, string username, string password, CancellationToken ct) => IsGatewayValidAsync(ip, username, password, ct);

    private bool IsAlreadyEncrypted(string input)
    {
        try
        {
            var keyRes = "1111111111111111";
            var ivRes = "2222222222222222";
            if (string.IsNullOrEmpty(input) || input.Length < 16) return false;
            using var aes = System.Security.Cryptography.Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(keyRes);
            aes.IV = Encoding.UTF8.GetBytes(ivRes);
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
            aes.Key = Encoding.UTF8.GetBytes(keyRes);
            aes.IV = Encoding.UTF8.GetBytes(ivRes);
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

    private IEnumerable<string> EnumerateLocalSubnetHosts()
    {
        var interfaces = NetworkInterface.GetAllNetworkInterfaces()
            .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                         ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                         !ni.Description.Contains("Virtual", StringComparison.OrdinalIgnoreCase) &&
                         !ni.Description.Contains("Pseudo-Interface", StringComparison.OrdinalIgnoreCase));

        foreach (var ni in interfaces)
        {
            _logger.LogInformation($"Scanning interface: {ni.Name} ({ni.Description})");
            var ipProps = ni.GetIPProperties();
            foreach (var unicast in ipProps.UnicastAddresses.Where(u => u.Address.AddressFamily == AddressFamily.InterNetwork))
            {
                var address = unicast.Address;
                var mask = unicast.IPv4Mask;
                if (mask == null)
                {
                    _logger.LogDebug($"  Skipping {address} - no mask found.");
                    continue;
                }

                _logger.LogInformation($"  Found IP: {address} with Mask: {mask}");

                var bytes = address.GetAddressBytes();
                var maskBytes = mask.GetAddressBytes();

                // On scanne par segments /24 pour rester rapide
                var networkPrefix = $"{bytes[0] & maskBytes[0]}.{bytes[1] & maskBytes[1]}.{bytes[2] & maskBytes[2]}";
                _logger.LogInformation($"  Scanning network segment: {networkPrefix}.0/24");

                for (int i = 1; i < 255; i++)
                {
                    var ip = $"{networkPrefix}.{i}";
                    // Ne pas se scanner soi-même
                    if (ip != address.ToString()) yield return ip;
                }
            }
        }
    }
}
