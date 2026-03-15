namespace HfSqlForwarder.Settings;

public class ForwarderOptions
{
    public HfSqlOptions HfSql { get; set; } = new();
    public SchedulerOptions Scheduler { get; set; } = new();
    public ApiOptions Api { get; set; } = new();
    public FilterOptions Filtre { get; set; } = new();
    public StateOptions State { get; set; } = new();
    public AdminOptions Admin { get; set; } = new();
    public LoggingOptions Logging { get; set; } = new();
}

public class HfSqlOptions
{
    public string Driver { get; set; } = "HFSQL";
    public string AnaPath { get; set; } = @"c:\\NetGlobal\\NetGlobal.wdd";
    public string RepPath { get; set; } = @"c:\\NetGlobal\\Historique\\";
    public string TablePrefix { get; set; } = "AMOY";
    public string TableSuffixFormat { get; set; } = "yyyyMMdd";
    public string TableNameLogical { get; set; } = string.Empty;
    public int CommandTimeoutSeconds { get; set; } = 15;
}

public class SchedulerOptions
{
    public int IntervalMinutes { get; set; } = 3;
}

public class ApiOptions
{
    public string BaseUrl { get; set; } = "https://kropkontrol.premiumasp.net";
    public string EndpointGlowflex { get; set; } = "/api/v1/growflex/hfsql";
    public string ApiKey { get; set; } = string.Empty;
    public string ApiKeyHeaderName { get; set; } = "Invenio";
    public string ApiKeySecretPath { get; set; } = "C:/ProgramData/HfSqlForwarder/apikey.secret";
    public int TimeoutSeconds { get; set; } = 30;
}

public class FilterOptions
{
    public int NumElt { get; set; } = 0;
    public List<int> NumEltList { get; set; } = new();
}

public class StateOptions
{
    public string StatePath { get; set; } = "C:/ProgramData/HfSqlForwarder/forwarder_state.json";
}

public class AdminOptions
{
    public string Secret { get; set; } = "25102017";
    public string HeaderName { get; set; } = "X-Admin-Secret";
}

public class LoggingOptions
{
    public string LogDirectory { get; set; } = "C:/ProgramData/HfSqlForwarder/logs";
    public string FileNamePattern { get; set; } = "log-{date}.txt";
}
