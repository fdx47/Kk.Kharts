using Serilog.Core;
using Serilog.Events;
using KK.UG6x.StoreAndForward.Services;

namespace KK.UG6x.StoreAndForward.Logging;

public class DashboardLogSink : ILogEventSink
{
    private readonly DashboardStateService _state;

    public DashboardLogSink(DashboardStateService state) => _state = state;

    public void Emit(LogEvent logEvent)
    {
        var level = logEvent.Level switch
        {
            LogEventLevel.Information => "INF",
            LogEventLevel.Warning => "WRN",
            LogEventLevel.Error => "ERR",
            LogEventLevel.Debug => "DBG",
            _ => "INF"
        };
        _state.AddLog(level, logEvent.RenderMessage());
    }
}
