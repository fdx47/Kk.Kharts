using HfSqlForwarder.Services;
using HfSqlForwarder.Settings;
using HfSqlForwarder.State;
using HfSqlForwarder.Web;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// Désactive le provider EventLog (pas de traces dans l'observateur d'événements Windows)
builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole();

builder.Host.UseWindowsService();

builder.Services.Configure<ForwarderOptions>(builder.Configuration.GetSection("Forwarder"));
builder.Services.AddSingleton<RuntimeSettingsService>();
builder.Services.AddSingleton<ForwarderStateStore>();
builder.Services.AddHttpClient<KhartsApiClient>();
builder.Services.AddSingleton<HfSqlReader>();
builder.Services.AddHostedService<ForwarderWorker>();
builder.Services.AddSingleton<ILoggerProvider, FileLoggerProvider>();

var app = builder.Build();

app.Services.GetRequiredService<RuntimeSettingsService>()
    .Initialize(app.Services.GetRequiredService<IOptionsMonitor<ForwarderOptions>>());

app.MapDashboardEndpoints();

await app.RunAsync();
