using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using HfSqlForwarder.Settings;

namespace HfSqlForwarder.State;

public class ForwarderStateStore
{
    private readonly ILogger<ForwarderStateStore> _logger;
    private readonly string _statePath;
    private readonly JsonSerializerOptions _jsonOpts = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };
    private readonly object _gate = new();
    private ForwarderState _cached = new();

    public ForwarderStateStore(ILogger<ForwarderStateStore> logger, IOptions<ForwarderOptions> options)
    {
        _logger = logger;
        var configuredPath = options.Value.State.StatePath;
        _statePath = string.IsNullOrWhiteSpace(configuredPath)
            ? Path.Combine(AppContext.BaseDirectory, "forwarder_state.json")
            : configuredPath;
        EnsureDirectory();
        _cached = LoadInternal();
    }

    public ForwarderState Current => _cached;

    public Task<ForwarderState> GetAsync() => Task.FromResult(_cached);

    public async Task SaveAsync(ForwarderState state)
    {
        lock (_gate)
        {
            _cached = state;
        }

        try
        {
            var json = JsonSerializer.Serialize(state, _jsonOpts);
            await File.WriteAllTextAsync(_statePath, json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la sauvegarde du state forwarder");
        }
    }

    private ForwarderState LoadInternal()
    {
        try
        {
            if (File.Exists(_statePath))
            {
                var json = File.ReadAllText(_statePath);
                var state = JsonSerializer.Deserialize<ForwarderState>(json, _jsonOpts);
                if (state != null) return state;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Impossible de charger forwarder_state.json, utilisation des valeurs par défaut");
        }

        return new ForwarderState();
    }

    private void EnsureDirectory()
    {
        try
        {
            var dir = Path.GetDirectoryName(_statePath);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Impossible de créer le dossier pour le state ({Path})", _statePath);
        }
    }
}
