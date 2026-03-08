using HfSqlForwarder.Models;
using HfSqlForwarder.Services;
using HfSqlForwarder.Settings;
using HfSqlForwarder.State;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HfSqlForwarder.Services;

public class ForwarderWorker : BackgroundService
{
    private readonly ILogger<ForwarderWorker> _logger;
    private readonly IOptionsMonitor<ForwarderOptions> _options;
    private readonly HfSqlReader _reader;
    private readonly KhartsApiClient _apiClient;
    private readonly ForwarderStateStore _stateStore;

    public ForwarderWorker(
        ILogger<ForwarderWorker> logger,
        IOptionsMonitor<ForwarderOptions> options,
        HfSqlReader reader,
        KhartsApiClient apiClient,
        ForwarderStateStore stateStore)
    {
        _logger = logger;
        _options = options;
        _reader = reader;
        _apiClient = apiClient;
        _stateStore = stateStore;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Forwarder démarré");

        while (!stoppingToken.IsCancellationRequested)
        {
            var interval = TimeSpan.FromMinutes(Math.Max(1, _options.CurrentValue.Scheduler.IntervalMinutes));
            await RunCycleAsync(stoppingToken);
            try
            {
                await Task.Delay(interval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }
    }

    private async Task RunCycleAsync(CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var jour = DateOnly.FromDateTime(now.UtcDateTime);
        var state = _stateStore.Current;
        state.LastRunUtc = now;
        state.LastError = null;

        try
        {
            var lignes = await _reader.ReadAsync(jour, ct);

            // Reset si changement de date
            if (state.LastDate != jour)
            {
                state.LastDate = jour;
                state.LastCycleNumber = 0;
                state.Pending.Clear();
            }

            var nouveaux = lignes
                .Where(r => r.CycleNumber > state.LastCycleNumber)
                .Select(r => new PendingSend
                {
                    NumElt = r.NumElt,
                    CycleNumber = r.CycleNumber,
                    StartMinutes = r.StartMinutes,
                    EndMinutes = r.EndMinutes,
                    WaterVolume = r.WaterVolume,
                    Jour = r.Jour
                })
                .ToList();

            // Merge pending + nouveaux
            var toSend = state.Pending.Concat(nouveaux)
                .OrderBy(p => p.CycleNumber)
                .ThenBy(p => p.Jour)
                .ToList();

            if (!toSend.Any())
            {
                _logger.LogInformation("Aucun nouvel enregistrement à envoyer (LastCycle={LastCycle})", state.LastCycleNumber);
                await _stateStore.SaveAsync(state);
                return;
            }

            // Convert back to DTO for API
            var payload = toSend.Select(p => new RegaRecord(
                p.NumElt,
                p.CycleNumber,
                p.StartMinutes,
                p.EndMinutes,
                p.WaterVolume,
                p.Jour)).ToList();

            var success = await _apiClient.SendBatchAsync(payload, ct);
            if (success)
            {
                var maxCycle = toSend.Max(p => p.CycleNumber);
                state.LastCycleNumber = maxCycle;
                state.Pending.Clear();
                state.LastSuccessUtc = DateTimeOffset.UtcNow;
            }
            else
            {
                state.Pending = toSend;
                state.LastError = "Échec d'envoi vers Glowflex";
            }

            await _stateStore.SaveAsync(state);
        }
        catch (Exception ex)
        {
            state.LastError = ex.Message;
            _logger.LogError(ex, "Erreur dans le cycle Forwarder");
            await _stateStore.SaveAsync(state);
        }
    }
}
