namespace KK.UG6x.StoreAndForward.Services;

public class DashboardStateService
{
    private readonly object _lock = new();
    private readonly List<LogEntry> _logs = new();
    private const int MaxLogEntries = 500;

    public bool IsOnline { get; private set; }
    public bool InternetAvailable { get; private set; }
    public bool IsForcedOffline { get; private set; }
    public bool IsOfflineOnlyMode { get; private set; }
    public string GatewayUrl { get; private set; } = string.Empty;
    public DateTime LastSync { get; private set; } = DateTime.MinValue;
    public int LastBatchCount { get; private set; }
    public int PendingCount { get; private set; }
    public int MappedDevicesCount { get; private set; }
    public DateTime ServiceStartTime { get; } = DateTime.Now;

    public void UpdateStatus(bool isOnline, string gatewayUrl, int lastBatchCount, int pendingCount, int mappedDevicesCount, bool internetAvailable, bool isForcedOffline, bool isOfflineOnlyMode)
    {
        lock (_lock)
        {
            IsOnline = isOnline;
            InternetAvailable = internetAvailable;
            IsForcedOffline = isForcedOffline;
            IsOfflineOnlyMode = isOfflineOnlyMode;
            GatewayUrl = gatewayUrl;
            LastSync = DateTime.Now;
            LastBatchCount = lastBatchCount;
            PendingCount = pendingCount;
            MappedDevicesCount = mappedDevicesCount;
        }
    }

    public void AddLog(string level, string message)
    {
        lock (_lock)
        {
            _logs.Add(new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message
            });

            while (_logs.Count > MaxLogEntries)
            {
                _logs.RemoveAt(0);
            }
        }
    }

    public List<LogEntry> GetLogs(int count = 100)
    {
        lock (_lock)
        {
            return _logs.TakeLast(count).Reverse().ToList();
        }
    }

    public DashboardData GetDashboardData()
    {
        lock (_lock)
        {
            return new DashboardData
            {
                IsOnline = IsOnline,
                InternetAvailable = InternetAvailable,
                IsForcedOffline = IsForcedOffline,
                IsOfflineOnlyMode = IsOfflineOnlyMode,
                GatewayUrl = GatewayUrl,
                LastSync = LastSync,
                LastBatchCount = LastBatchCount,
                PendingCount = PendingCount,
                MappedDevicesCount = MappedDevicesCount,
                ServiceStartTime = ServiceStartTime,
                Uptime = DateTime.Now - ServiceStartTime
            };
        }
    }
}

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class DashboardData
{
    public bool IsOnline { get; set; }
    public bool InternetAvailable { get; set; }
    public bool IsForcedOffline { get; set; }
    public bool IsOfflineOnlyMode { get; set; }
    public string GatewayUrl { get; set; } = string.Empty;
    public DateTime LastSync { get; set; }
    public int LastBatchCount { get; set; }
    public int PendingCount { get; set; }
    public int MappedDevicesCount { get; set; }
    public DateTime ServiceStartTime { get; set; }
    public TimeSpan Uptime { get; set; }
}
