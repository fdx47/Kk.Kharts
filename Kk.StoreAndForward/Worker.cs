using KK.UG6x.StoreAndForward.Domain.DTOs;
using KK.UG6x.StoreAndForward.Domain.Interfaces;
using KK.UG6x.StoreAndForward.Services;
using Spectre.Console;
using System.Text.Json;

namespace KK.UG6x.StoreAndForward;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IUG65Client _ug65Client;
    private readonly IKhartsApiClient _apiClient;
    private readonly ILocalStore _localStore;
    private readonly DashboardStateService _dashboardState;
    private readonly IConfiguration _configuration;
    private readonly AppSettingsService _settingsService;

    private TimeSpan _intervaloExecucao = TimeSpan.FromSeconds(60);
    private string _internetCheckHost = "8.8.8.8";
    private bool _forceOffline = false;
    private bool _offlineOnlyMode = false;

    // config class
    private class DeviceUplinkConfig
    {
        public string Url { get; set; } = string.Empty;
        public Dictionary<string, string> Headers { get; set; } = new();
    }

    // Cache: DevEUI -> DeviceUplinkConfig
    private Dictionary<string, DeviceUplinkConfig> _deviceUplinkMap = new Dictionary<string, DeviceUplinkConfig>();
    private DateTime _lastMetadataUpdate = DateTime.MinValue;
    private readonly TimeSpan _metadataRefreshInterval = TimeSpan.FromMinutes(10); // Atualiza metadata a cada 10 min

    // Controlo de estado para evitar queries desnecessárias à Gateway
    private bool _wasOfflineLastCycle = false;
    private bool _isFirstRun = true;

    public Worker(ILogger<Worker> logger, IUG65Client ug65Client, IKhartsApiClient apiClient, ILocalStore localStore, IConfiguration configuration, DashboardStateService dashboardState, AppSettingsService settingsService)
    {
        _logger = logger;
        _ug65Client = ug65Client;
        _apiClient = apiClient;
        _localStore = localStore;
        _dashboardState = dashboardState;
        _configuration = configuration;
        _settingsService = settingsService;

        LoadDefaultsFromConfiguration();
    }

    private void LoadDefaultsFromConfiguration()
    {
        var intervalSeconds = _configuration.GetValue<int>("WorkerSettings:IntervalSeconds");
        if (intervalSeconds > 0)
        {
            _intervaloExecucao = TimeSpan.FromSeconds(intervalSeconds);
        }

        var checkHost = _configuration.GetValue<string>("WorkerSettings:InternetCheckHost");
        if (!string.IsNullOrEmpty(checkHost))
        {
            _internetCheckHost = checkHost;
        }

        _forceOffline = _configuration.GetValue<bool>("WorkerSettings:ForceOffline");
    }

    private async Task RefreshConfigFromSettingsAsync()
    {
        try
        {
            var intervalDb = await _settingsService.GetSettingAsync("IntervalSeconds", "WorkerSettings");
            if (int.TryParse(intervalDb, out var intervalSeconds) && intervalSeconds > 0)
            {
                _intervaloExecucao = TimeSpan.FromSeconds(intervalSeconds);
            }

            var hostDb = await _settingsService.GetSettingAsync("InternetCheckHost", "WorkerSettings");
            if (!string.IsNullOrWhiteSpace(hostDb))
            {
                _internetCheckHost = hostDb;
            }

            var forceOfflineDb = await _settingsService.GetSettingAsync("ForceOffline", "WorkerSettings");
            if (!string.IsNullOrEmpty(forceOfflineDb) && bool.TryParse(forceOfflineDb, out var forceOfflineFlag))
            {
                _forceOffline = forceOfflineFlag;
            }

            var offlineOnlyDb = await _settingsService.GetSettingAsync("OfflineOnlyMode", "WorkerSettings");
            if (!string.IsNullOrEmpty(offlineOnlyDb) && bool.TryParse(offlineOnlyDb, out var offlineOnlyFlag))
            {
                _offlineOnlyMode = offlineOnlyFlag;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[Worker] Impossible de récupérer les paramètres depuis AppSettingsService. Utilisation des valeurs par défaut.");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (Environment.UserInteractive)
        {
            AnsiConsole.Clear();
            var header = new FigletText("Milesight UG65") { Color = Color.Aqua };
            AnsiConsole.Write(header);
            AnsiConsole.MarkupLine("[bold yellow]KK.UG6x.StoreAndForward[/] - [blue]Service de Synchronisation[/]");
            AnsiConsole.MarkupLine($"[grey]Inicié le: {DateTime.Now:dd/MM/yyyy HH:mm:ss}[/]");
            AnsiConsole.WriteLine();
        }

        await RefreshConfigFromSettingsAsync();

        _logger.LogInformation("KK.UG6x.StoreAndForward Worker démarré le: {time}", DateTimeOffset.Now);
        _logger.LogInformation($"Intervalle d'exécution configuré: {_intervaloExecucao.TotalSeconds} secondes. Host de test: {_internetCheckHost}");

        // Initial metadata fetch
        await RefreshMetadataAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (Environment.UserInteractive)
            {
                var status = AnsiConsole.Status();
                status.Spinner = Spinner.Known.Dots;
                await status.StartAsync("[yellow]Lancement du cycle...[/]", async ctx =>
                {
                    await DoWorkIterationAsync(stoppingToken, ctx);
                });
            }
            else
            {
                await DoWorkIterationAsync(stoppingToken, null);
            }

            await Task.Delay(_intervaloExecucao, stoppingToken);
        }
    }

    private async Task DoWorkIterationAsync(CancellationToken stoppingToken, StatusContext? ctx)
    {
        try
        {
            await RefreshConfigFromSettingsAsync();

            // Nettoyage console pour éviter les superpositions
            if (Environment.UserInteractive) AnsiConsole.Clear();

            // Vérification Internet
            if (ctx != null) ctx.Status = "[yellow]Vérification de la connexion internet...[/]";
            bool internetAvailable = CheckInternetConnection();
            bool isOnline = !_forceOffline && internetAvailable;

            if (Environment.UserInteractive)
            {
                AnsiConsole.WriteLine(); // Espace avant le dashboard
                AnsiConsole.WriteLine();

                var statusMessage = isOnline
                    ? (_offlineOnlyMode
                        ? "[yellow]⚠ Mode Stockage Uniquement (Connecté mais n'envoie pas vers l'API)[/]"
                        : "[green]✔ Connecté à Internet[/]")
                    : _forceOffline
                        ? (internetAvailable
                            ? "[yellow]⚠ Mode forcé Offline (Internet détecté)[/]"
                            : "[yellow]⚠ Mode forcé Offline (Internet indisponible)[/]")
                        : "[red]✘ Hors ligne (Mode Backup)[/]";

                AnsiConsole.MarkupLine(statusMessage);
                AnsiConsole.MarkupLine($"[blue]▶ Passerelle active :[/] [bold cyan]{_ug65Client.BaseUrl}[/]");
            }


            // Rafraîchit les métadonnées périodiquement
            if (DateTime.UtcNow - _lastMetadataUpdate > _metadataRefreshInterval)
            {
                if (ctx != null) ctx.Status = "[yellow]Mise à jour des configurations...[/]";
                await RefreshMetadataAsync(stoppingToken);
            }

            // Récupère les données de la Gateway si:
            // 1. C'est le premier démarrage
            // 2. Vient de récupérer d'une panne internet
            // 3. Est OFFLINE (pour éviter de perdre les données du buffer de 1000 de la Gateway)
            bool shouldFetchFromGateway = _isFirstRun || (_wasOfflineLastCycle && isOnline) || !isOnline;

            int newPacketsCount = 0;
            if (shouldFetchFromGateway)
            {
                if (ctx != null) ctx.Status = "[yellow]Téléchargement des données UG65...[/]";
                var payloads = await _ug65Client.FetchLatestDataAsync(stoppingToken);
                newPacketsCount = payloads.Count;
                
                if (payloads.Count > 0)
                {
                    if (Environment.UserInteractive)
                        AnsiConsole.MarkupLine($"[blue]ℹ {payloads.Count} paquets LoRaWAN reçus.[/]");
                    else
                        _logger.LogInformation($"Récupération de {payloads.Count} paquets.");

                    int count = 0;
                    foreach (var payload in payloads)
                    {
                        count++;
                        if (ctx != null)
                        {
                            var statusText = isOnline && !_offlineOnlyMode 
                                ? $"[yellow]Envoi vers API Cloud: {count}/{payloads.Count} ({payload.DevEui})...[/]"
                                : $"[yellow]Stockage local: {count}/{payloads.Count} ({payload.DevEui})...[/]";
                            ctx.Status = statusText;
                        }

                        // Verifica se já existe no SQLite (enviado OU pendente)
                        var jaExiste = await _localStore.AlreadyExistsAsync(payload);
                        if (jaExiste)
                        {
                            _logger.LogDebug($"[Worker] Paquet déjà présent dans la base de données pour {payload.DevEui}, ignoré.");
                            continue;
                        }

                        if (isOnline && !_offlineOnlyMode)
                        {
                            var config = GetUplinkConfigForDevice(payload.DevEui);
                            var success = await _apiClient.SendDataAsync(payload, stoppingToken, config?.Url, config?.Headers);
                            if (!success)
                            {
                                await _localStore.SavePayloadAsync(payload, "Generic");
                            }
                        }
                        else
                        {
                            // Mode Offline ou OfflineOnlyMode: sauvegarde uniquement en local
                            await _localStore.SavePayloadAsync(payload, "Generic");
                        }
                    }
                }

                _isFirstRun = false; // Marque que ce n'est plus le premier démarrage
            }
            else
            {
                if (Environment.UserInteractive)
                    AnsiConsole.MarkupLine("[dim]⏭ Connexion stable, pas de nouvelle synchronisation Gateway.[/]");
            }

            // Met à jour l'état de connectivité pour le prochain cycle
            _wasOfflineLastCycle = !isOnline;

            // Traite les données en attente si en ligne (mais PAS si OfflineOnlyMode est actif)
            if (isOnline && !_offlineOnlyMode)
            {
                if (ctx != null) ctx.Status = "[yellow]Envoi des données locales en attente...[/]";
                await ProcessarPendentesAsync(stoppingToken, ctx);
            }

            var pendingCount = await _localStore.GetPendingCountAsync();
            
            _dashboardState.UpdateStatus(
                isOnline,
                _ug65Client.BaseUrl,
                newPacketsCount,
                pendingCount,
                _deviceUplinkMap.Count,
                internetAvailable,
                _forceOffline,
                _offlineOnlyMode
            );

            if (Environment.UserInteractive)
            {
                RenderDashboard(isOnline, newPacketsCount, pendingCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur dans la boucle de travail.");
        }
    }

    private async Task RefreshMetadataAsync(CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("[Metadata] Rafraîchissement des mappages Device -> Uplink...");

            // Fetch APIs
            var devices = await _ug65Client.GetDevicesAsync(cancellationToken);
            
            if (devices != null && devices.Any())
            {
                var newMap = new Dictionary<string, DeviceUplinkConfig>();
                
                // Get unique ApplicationIDs from devices
                var activeAppIds = devices
                    .Select(d => d.ApplicationId)
                    .Where(id => !string.IsNullOrEmpty(id))
                    .Distinct()
                    .ToList();

                _logger.LogInformation($"[Metadata] {devices.Count} appareils trouvés. Récupération des intégrations pour {activeAppIds.Count} applications actives...");

                // Map AppID -> UplinkConfig
                var appConfigMap = new Dictionary<string, DeviceUplinkConfig>();
                foreach (var appId in activeAppIds)
                {
                    var integration = await _ug65Client.GetIntegrationAsync(appId, "http", cancellationToken);
                    if (integration != null && !string.IsNullOrEmpty(integration.DataUpUrl))
                    {
                        var config = new DeviceUplinkConfig 
                        { 
                            Url = integration.DataUpUrl 
                        };
                        
                        if (integration.Headers != null)
                        {
                            foreach (var h in integration.Headers)
                            {
                                if (!string.IsNullOrEmpty(h.Key))
                                {
                                    config.Headers[h.Key] = h.Value;
                                }
                            }
                        }
                        
                        appConfigMap[appId] = config;
                    }
                    else
                    {
                        _logger.LogWarning($"[Metadata] Application {appId} n'a pas d'intégration HTTP configurée.");
                    }
                }

                // Associate devices to configs
                foreach (var dev in devices)
                {
                    if (!string.IsNullOrEmpty(dev.ApplicationId) && appConfigMap.TryGetValue(dev.ApplicationId, out var config))
                    {
                        newMap[dev.DevEui] = config;
                    }
                }
                
                _deviceUplinkMap = newMap;
                _lastMetadataUpdate = DateTime.UtcNow;
                _logger.LogInformation($"[Metadata] Mappage terminé. {_deviceUplinkMap.Count}/{devices.Count} appareils prêts.");
            }
            else
            {
                _logger.LogWarning("[Metadata] Aucun appareil trouvé sur la Gateway.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du rafraîchissement des métadonnées.");
        }
    }

    private DeviceUplinkConfig? GetUplinkConfigForDevice(string devEui)
    {
        if (_deviceUplinkMap.TryGetValue(devEui, out var config))
        {
            return config;
        }
        return null; // Will fallback to default
    }

    private async Task ProcessarPendentesAsync(CancellationToken cancellationToken, StatusContext? ctx)
    {
        try
        {
            var itensPendentes = await _localStore.GetPendingPayloadsAsync(20); // Lot de 20 

            if (itensPendentes.Count == 0) return;

            if (ctx != null) ctx.Status = $"[yellow]Renvoi de {itensPendentes.Count} éléments locaux...[/]";
            _logger.LogInformation($"Tentative de renvoi de {itensPendentes.Count} éléments en attente (Forward)...");

            int processed = 0;
            foreach (var itemPendente in itensPendentes)
            {
                processed++;
                if (cancellationToken.IsCancellationRequested) break;
                if (cancellationToken.IsCancellationRequested) break;

                try
                {
                    // Désérialise le payload stocké
                    var dtoRecuperado = JsonSerializer.Deserialize<GatewayPayloadDTO>(itemPendente.PayloadJson);
                     if (dtoRecuperado != null)
                     {
                        if (ctx != null) ctx.Status = $"[yellow]Envoi vers API Cloud: {processed}/{itensPendentes.Count} ({dtoRecuperado.DevEui})...[/]";
                        // Determine URL again (in case it changed or wasn't available before)
                        var config = GetUplinkConfigForDevice(dtoRecuperado.DevEui);
                        
                        if (config == null || string.IsNullOrEmpty(config.Url))
                        {
                            _logger.LogWarning($"[RECOVERY] Impossible de renvoyer le paquet {itemPendente.Id} ({dtoRecuperado.DevEui}): URL d'intégration manquante dans la Gateway.");
                            await _localStore.IncrementRetryCountAsync(itemPendente.Id);
                            continue;
                        }

                        var reenvioSucesso = await _apiClient.SendDataAsync(dtoRecuperado, cancellationToken, config.Url, config.Headers);
                        if (reenvioSucesso)
                        {
                            await _localStore.MarkAsSentAsync(itemPendente.Id);
                            _logger.LogInformation($"[RECOVERY] Payload {itemPendente.Id} ({dtoRecuperado.DevEui}) envoyé avec succès à {config.Url} et marqué comme envoyé.");
                        }
                        else
                        {
                            // Failed again. Fail-fast: if the cloud is unreachable, stop the whole batch to avoid console floods.
                            _logger.LogWarning($"[RECOVERY] Échec de l'envoi pour {dtoRecuperado.DevEui}. Suspension du traitement des pendentes pour ce cycle.");
                            break; 
                        }
                     }

                    // Throttling: Pause réduite pour plus de rapidité (200ms)
                    await Task.Delay(200, cancellationToken);
                }
                catch (Exception exInterno)
                {
                    _logger.LogError(exInterno, $"Erreur lors du traitement de l'élément en attente {itemPendente.Id}");
                }
            }

            // À la fin du traitement des éléments en attente, nettoie les enregistrements envoyés depuis plus de 30 jours
            await _localStore.CleanupOldRecordsAsync(30);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'accès au LocalStore pour traiter les éléments en attente.");
        }
    }

    private void RenderDashboard(bool isOnline, int lastBatchCount, int pendingCount)
    {
        var grid = new Grid();
        grid.AddColumn(new GridColumn { NoWrap = true });
        grid.AddColumn(new GridColumn { Padding = new Padding(2, 0) });

        var statusTable = new Table { Border = TableBorder.Rounded };
        statusTable.AddColumn("[bold blue]Composant[/]");
        statusTable.AddColumn("[bold blue]État[/]");
        statusTable.AddRow("Connexion Internet", isOnline ? "[green]Connecté √[/]" : "[red]Hors ligne !![/]");
        statusTable.AddRow("Passerelle UG65", $"[cyan]{_ug65Client.BaseUrl}[/]");
        statusTable.AddRow("Dernière Sync", $"[white]{DateTime.Now:HH:mm:ss}[/]");
        statusTable.AddRow("Nouveaux Paquets", lastBatchCount > 0 ? $"[green]{lastBatchCount}[/]" : "[grey]0[/]");
        statusTable.AddRow("Paquets en attente", pendingCount > 0 ? $"[bold yellow]{pendingCount}[/]" : "[grey]0[/]");
        statusTable.AddRow("Dispositifs Mappés", $"[yellow]{_deviceUplinkMap.Count}[/]");

        var panel = new Panel(statusTable)
        {
            Header = new PanelHeader("[bold yellow] TABLEAU DE BORD DE SYNCHRONISATION [/]"),
            Border = BoxBorder.Double,
            Padding = new Padding(1, 1)
        };

        AnsiConsole.Write(panel);
    }

    private bool CheckInternetConnection()
    {
        try
        {
            // 1. Essaie PING (plus rapide)
            using (var ping = new System.Net.NetworkInformation.Ping())
            {
                var reply = ping.Send(_internetCheckHost, 1500);
                if (reply.Status == System.Net.NetworkInformation.IPStatus.Success) return true;
            }

            // 2. Fallback: Essaie connexion HTTP (si PING est bloqué sur le réseau)
            using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) })
            {
                using var response = client.GetAsync("http://www.google.com", HttpCompletionOption.ResponseHeadersRead).Result;
                return response.IsSuccessStatusCode;
            }
        }
        catch
        {
            return false;
        }
    }
}
