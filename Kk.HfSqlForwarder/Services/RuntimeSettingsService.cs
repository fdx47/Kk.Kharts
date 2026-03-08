using HfSqlForwarder.Settings;
using Microsoft.Extensions.Options;

namespace HfSqlForwarder.Services;

public class RuntimeSettingsService
{
    private ForwarderOptions _current;
    private readonly object _gate = new();

    public RuntimeSettingsService()
    {
        _current = new ForwarderOptions();
    }

    public void Initialize(IOptionsMonitor<ForwarderOptions> monitor)
    {
        _current = Clone(monitor.CurrentValue);
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
}
