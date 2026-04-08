using HfSqlForwarder.Settings;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace HfSqlForwarder.Services;

public class RuntimeSettingsService
{
    private ForwarderOptions _current;
    private readonly object _gate = new();
    private readonly string _runtimePath;

    public RuntimeSettingsService(IHostEnvironment env)
    {
        _current = new ForwarderOptions();
        _runtimePath = Path.Combine(env.ContentRootPath, "runtime-settings.json");
    }

    public void Initialize(IOptionsMonitor<ForwarderOptions> monitor)
    {
        _current = LoadRuntimeOverride() ?? Clone(monitor.CurrentValue);
        monitor.OnChange(opts =>
        {
            lock (_gate)
            {
                _current = Clone(opts);
            }
        });
    }

    public ForwarderOptions Get() { lock (_gate) return Clone(_current); }

    public void Update(ForwarderOptions newOptions)
    {
        if (newOptions is null) return;
        lock (_gate)
        {
            _current = Clone(newOptions);
            PersistToFile(_current);
        }
    }

    private static ForwarderOptions Clone(ForwarderOptions source)
    {
        return new ForwarderOptions
        {
            HfSql = new HfSqlOptions
            {
                Driver = source.HfSql.Driver,
                AnaPath = source.HfSql.AnaPath,
                RepPath = source.HfSql.RepPath,
                TablePrefix = source.HfSql.TablePrefix,
                TableSuffixFormat = source.HfSql.TableSuffixFormat,
                TableNameLogical = source.HfSql.TableNameLogical,
                CommandTimeoutSeconds = source.HfSql.CommandTimeoutSeconds
            },
            Scheduler = new SchedulerOptions
            {
                IntervalMinutes = source.Scheduler.IntervalMinutes
            },
            Api = new ApiOptions
            {
                BaseUrl = source.Api.BaseUrl,
                EndpointGlowflex = source.Api.EndpointGlowflex,
                ApiKey = source.Api.ApiKey,
                TimeoutSeconds = source.Api.TimeoutSeconds
            },
            Filtre = new FilterOptions
            {
                NumElt = source.Filtre.NumElt
            },
            State = new StateOptions
            {
                StatePath = source.State.StatePath
            },
            Admin = new AdminOptions
            {
                Secret = source.Admin.Secret,
                HeaderName = source.Admin.HeaderName
            },
            Logging = new LoggingOptions
            {
                LogDirectory = source.Logging.LogDirectory,
                FileNamePattern = source.Logging.FileNamePattern
            }
        };
    }

    private void PersistToFile(ForwarderOptions opts)
    {
        try
        {
            var json = JsonSerializer.Serialize(opts, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(_runtimePath, json);
        }
        catch
        {
            // On ne bloque pas l'exécution si l'écriture échoue (permissions, etc.)
        }
    }

    private ForwarderOptions? LoadRuntimeOverride()
    {
        try
        {
            if (File.Exists(_runtimePath))
            {
                var json = File.ReadAllText(_runtimePath);
                var opts = JsonSerializer.Deserialize<ForwarderOptions>(json);
                if (opts != null) return opts;
            }
        }
        catch
        {
            // Ignore erreurs de lecture/désérialisation
        }
        return null;
    }
}
