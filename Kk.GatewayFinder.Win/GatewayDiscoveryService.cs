using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Text.RegularExpressions;

namespace Kk.GatewayFinder.Win
{
    internal sealed class GatewayDiscoveryService
    {
        private static readonly int[] CandidatePorts = { 8080 };
        private const int GatewayPort = 8080;
        private readonly HttpClient _httpClient;

        public GatewayDiscoveryService()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };

            _httpClient = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(3)
            };
        }

        public async Task<List<GatewayCandidate>> FindGatewaysAsync(string? lastKnownIp, bool stopOnFirst, Action<string>? logCallback, CancellationToken cancellationToken)
        {
            var results = new List<GatewayCandidate>();

            async Task<bool> ProbeIpAsync(string ip)
            {
                foreach (var port in CandidatePorts)
                {
                    var candidate = await ProbeCandidateAsync(ip, port, logCallback, cancellationToken);
                    if (candidate != null)
                    {
                        results.Add(candidate);
                        if (stopOnFirst)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            if (!string.IsNullOrWhiteSpace(lastKnownIp))
            {
                logCallback?.Invoke($"Test du dernier IP connu ({lastKnownIp})...");
                var foundLastKnown = await ProbeIpAsync(lastKnownIp);
                if (foundLastKnown && stopOnFirst)
                {
                    logCallback?.Invoke("Gateway confirmée via l'historique.");
                    return results;
                }

                if (!foundLastKnown)
                {
                    logCallback?.Invoke("Dernier IP invalide, lancement d'un scan complet...");
                }
            }

            var hosts = EnumerateSubnetHosts(logCallback).ToList();
            var portList = string.Join(", ", CandidatePorts);
            logCallback?.Invoke($"Scan de {hosts.Count} adresses locales sur les ports {portList}...");

            if (stopOnFirst)
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                var tasks = hosts.Select(async ip =>
                {
                    if (cts.Token.IsCancellationRequested) return null;
                    var candidate = await ProbeCandidateAsync(ip, GatewayPort, logCallback, cts.Token);
                    if (candidate != null)
                    {
                        cts.Cancel();
                    }
                    return candidate;
                }).ToList();

                var candidates = await Task.WhenAll(tasks);
                var first = candidates.FirstOrDefault(c => c != null);
                if (first != null)
                {
                    results.Add(first);
                }
            }
            else
            {
                var bag = new ConcurrentBag<GatewayCandidate>();
                var tasks = hosts.Select(async ip =>
                {
                    if (cancellationToken.IsCancellationRequested) return;
                    var candidate = await ProbeCandidateAsync(ip, GatewayPort, logCallback, cancellationToken);
                    if (candidate != null) bag.Add(candidate);
                }).ToList();

                await Task.WhenAll(tasks);
                results.AddRange(bag.OrderBy(c => c.IpAddress));
            }

            if (results.Count == 0)
            {
                logCallback?.Invoke("Aucune passerelle détectée.");
            }
            else
            {
                logCallback?.Invoke($"{results.Count} gateway(s) détectée(s).");
            }

            return results;
        }

        private async Task<GatewayCandidate?> ProbeCandidateAsync(string ip, int port, Action<string>? logCallback, CancellationToken cancellationToken)
        {
            // Step 1: Check if port 8080 is open (API port - indicator of Milesight gateway)
            if (!await IsPortOpenAsync(ip, GatewayPort, cancellationToken))
            {
                return null;
            }

            logCallback?.Invoke($"{ip}: Port {GatewayPort} ouvert (API Milesight détectée)");

            // Step 2: Check port 80 for web interface with signature
            var signature = await TryIdentifyGatewayAsync(ip, 80, logCallback, cancellationToken);
            if (string.IsNullOrWhiteSpace(signature))
            {
                logCallback?.Invoke($"{ip}: Port 80 ouvert mais signature inconnue");
                return null;
            }

            logCallback?.Invoke($"Signature Milesight confirmée sur {ip}:80 ({signature})");
            
            return new GatewayCandidate(ip, 80)
            {
                IsPortOpen = true,
                VerificationDetails = signature
            };
        }

        private async Task<string?> TryIdentifyGatewayAsync(string ip, int port, Action<string>? logCallback, CancellationToken cancellationToken)
        {
            var schemes = new[] { "https", "http" };
            var paths = new[] { "/", "/login", "/login.html", "/index.html" };

            foreach (var scheme in schemes)
            {
                foreach (var path in paths)
                {
                    try
                    {
                        var url = BuildUrl(scheme, ip, port, path);
                        using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                        {
                            var response = await _httpClient.SendAsync(request, cancellationToken);
                            var body = await response.Content.ReadAsStringAsync();

                            if (response.IsSuccessStatusCode)
                            {
                                var signature = IdentifyMilesightSignature(body);
                                if (!string.IsNullOrEmpty(signature))
                                {
                                    return $"{scheme.ToUpperInvariant()} {path} · {signature}";
                                }

                                // Debug: log HTML content if no signature found
                                var preview = body?.Length > 500 ? body.Substring(0, 500) + "..." : body;
                                logCallback?.Invoke($"{ip}:{port}: HTTP {(int)response.StatusCode} sans signature ({scheme.ToUpperInvariant()} {path}) - HTML preview: {preview}");
                            }

                            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                            {
                                logCallback?.Invoke($"{ip}:{port}: Réponse {response.StatusCode} ({scheme.ToUpperInvariant()} {path})");
                            }
                        }
                    }
                    catch (HttpRequestException httpEx)
                    {
                        if (httpEx.Message.IndexOf("Client sent an HTTP request to an HTTPS server", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            logCallback?.Invoke($"{ip}:{port}: serveur HTTPS détecté – tentative HTTP ignorée.");
                        }
                        else
                        {
                            logCallback?.Invoke($"{ip}:{port}: erreur de requête ({scheme.ToUpperInvariant()} {path}) – {httpEx.Message}");
                        }
                    }
                    catch (Exception ex)
                    {
                        logCallback?.Invoke($"{ip}:{port}: erreur ({scheme.ToUpperInvariant()} {path}) – {ex.Message}");
                    }
                }
            }

            return null;
        }

        private static string BuildUrl(string scheme, string ip, int port, string path)
        {
            var defaultPort = scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? 443 : 80;
            var builder = new StringBuilder();
            builder.Append(scheme).Append("://").Append(ip);
            if (port != defaultPort)
            {
                builder.Append(':').Append(port);
            }

            builder.Append(path);
            return builder.ToString();
        }

        private static async Task<bool> IsPortOpenAsync(string ip, int port, CancellationToken cancellationToken)
        {
            try
            {
                using (var tcp = new TcpClient())
                {
                    var connectTask = tcp.ConnectAsync(ip, port);
                    var timeoutTask = Task.Delay(TimeSpan.FromMilliseconds(200), cancellationToken);
                    var completed = await Task.WhenAny(connectTask, timeoutTask);
                    return completed == connectTask && tcp.Connected;
                }
            }
            catch
            {
                return false;
            }
        }

        private static readonly Regex MilesightTitleRegex = new Regex("<title\\s+id\\s*=\\s*\"rt_title\"\\s*>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly string[] SignatureKeywords =
        {
            "LoRaWAN Gateway",
            "login_logo.png",
            "Milesight Cloud",
            "Milesight LoRaWAN",
            "UG65",
            "particles-js",
            "login-bg",
            "CryptoJS",
            "rt_title"
        };

        private static string? IdentifyMilesightSignature(string? html)
        {
            if (string.IsNullOrWhiteSpace(html))
            {
                return null;
            }

            foreach (var keyword in SignatureKeywords)
            {
                if (html.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return $"mot-clé '{keyword}'";
                }
            }

            var match = MilesightTitleRegex.Match(html);
            if (match.Success)
            {
                return "balise <title id='rt_title'>";
            }

            return null;
        }

        private static List<string> EnumerateSubnetHosts(Action<string>? logCallback = null)
        {
            var allInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            logCallback?.Invoke($"DEBUG: Total interfaces found: {allInterfaces.Length}");
            
            foreach (var ni in allInterfaces)
            {
                logCallback?.Invoke($"DEBUG: Interface '{ni.Name}' - Status: {ni.OperationalStatus}, Type: {ni.NetworkInterfaceType}, Desc: {ni.Description}");
            }
            
            var interfaces = allInterfaces
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up &&
                             ni.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                             ni.Description.IndexOf("Virtual", StringComparison.OrdinalIgnoreCase) < 0 &&
                             ni.Description.IndexOf("Docker", StringComparison.OrdinalIgnoreCase) < 0 &&
                             ni.Description.IndexOf("WSL", StringComparison.OrdinalIgnoreCase) < 0 &&
                             ni.Description.IndexOf("Hyper-V", StringComparison.OrdinalIgnoreCase) < 0);
            
            logCallback?.Invoke($"DEBUG: Interfaces after filtering: {interfaces.Count()}");

            var subnets = new List<string>();
            var allIps = new List<string>();

            foreach (var ni in interfaces)
            {
                logCallback?.Invoke($"DEBUG: Processing interface '{ni.Name}'...");
                IPInterfaceProperties properties;
                try
                {
                    properties = ni.GetIPProperties();
                }
                catch
                {
                    logCallback?.Invoke($"DEBUG: Failed to get IP properties for '{ni.Name}'");
                    continue;
                }

                var ipv4Addresses = properties.UnicastAddresses.Where(u => u.Address.AddressFamily == AddressFamily.InterNetwork).ToList();
                logCallback?.Invoke($"DEBUG: Interface '{ni.Name}' has {ipv4Addresses.Count} IPv4 addresses");

                foreach (var unicast in ipv4Addresses)
                {
                    var address = unicast.Address;
                    var mask = unicast.IPv4Mask;
                    if (mask == null)
                    {
                        logCallback?.Invoke($"DEBUG: No mask for {address} on '{ni.Name}'");
                        continue;
                    }

                    var addressBytes = address.GetAddressBytes();
                    var maskBytes = mask.GetAddressBytes();
                    var prefix = $"{addressBytes[0] & maskBytes[0]}.{addressBytes[1] & maskBytes[1]}.{addressBytes[2] & maskBytes[2]}";
                    var subnet = $"{prefix}.0/24";
                    
                    if (!subnets.Contains(subnet))
                    {
                        subnets.Add(subnet);
                        logCallback?.Invoke($"Interface: {ni.Name} - Subnet: {subnet} (IP: {address})");
                    }

                    for (var host = 1; host < 255; host++)
                    {
                        var ip = $"{prefix}.{host}";
                        if (!ip.Equals(address.ToString(), StringComparison.OrdinalIgnoreCase))
                        {
                            allIps.Add(ip);
                        }
                    }
                }
            }

            logCallback?.Invoke($"Total subnets: {subnets.Count}, Total IPs: {allIps.Count}");
            return allIps;
        }
    }
}
