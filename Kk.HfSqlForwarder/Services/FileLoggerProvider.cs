using HfSqlForwarder.Settings;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;

namespace HfSqlForwarder.Services;

public class FileLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, FileLogger> _loggers = new(StringComparer.OrdinalIgnoreCase);
    private readonly IOptionsMonitor<ForwarderOptions> _options;

    public FileLoggerProvider(IOptionsMonitor<ForwarderOptions> options)
    {
        _options = options;
    }

    public ILogger CreateLogger(string categoryName) => _loggers.GetOrAdd(categoryName, name => new FileLogger(name, _options));

    public void Dispose()
    {
        foreach (var logger in _loggers.Values)
        {
            logger.Dispose();
        }
    }
}

internal class FileLogger : ILogger, IDisposable
{
    private readonly string _category;
    private readonly IOptionsMonitor<ForwarderOptions> _options;
    private readonly object _gate = new();

    public FileLogger(string category, IOptionsMonitor<ForwarderOptions> options)
    {
        _category = category;
        _options = options;
    }

    IDisposable ILogger.BeginScope<TState>(TState state) => NullScope.Instance;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;
        var message = formatter(state, exception);
        WriteLine(logLevel, message, exception);
    }

    private void WriteLine(LogLevel level, string message, Exception? ex)
    {
        try
        {
            var opts = _options.CurrentValue.Logging;
            var dir = opts.LogDirectory;
            var file = opts.FileNamePattern.Replace("{date}", DateTime.UtcNow.ToString("yyyy-MM-dd"));
            var fullPath = Path.Combine(dir, file);
            lock (_gate)
            {
                Directory.CreateDirectory(dir);
                File.AppendAllText(fullPath, $"{DateTime.UtcNow:O} [{level}] {_category} - {message}{(ex != null ? " | " + ex : string.Empty)}{Environment.NewLine}");
            }
        }
        catch
        {
            // Ignorer les erreurs de log pour ne pas bloquer le flux principal
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

internal sealed class NullScope : IDisposable
{
    public static readonly NullScope Instance = new();
    private NullScope() { }
    public void Dispose() { }
}
